import Foundation
import Testing
@testable import SimTuningLogic

/// Swift mirror of `packages/common/src/audio/v2-parity.spec.ts`, running the full audio dyno
/// pipeline on the bundled v2 demo recording.
@Suite("v2 parity — audio dyno on the bundled v2 demo recording", .serialized)
struct AudioParityTests {
    static let audio: PcmAudio = try! Wav.readMono(Data(contentsOf: Golden.url("dyno-acceleration.wav")))
    static let spectrogram: Spectrogram = try! AudioDyno.calculateSpectrogram(
        audio.samples, sampleRate: Double(audio.sampleRate),
        options: .init(fftSize: 32768, frequenzbeginn: 3000, frequenzende: 12000)
    )
    static let points = AudioDyno.calculateRotationalSpeed(spectrogram, intensityPercent: 100)
    static let clusters = AudioDyno.calculateClusters(points, epsilon: 160)
    /// v2 reported speeds relative to the band start; v3 adds it back (audit D1).
    static let bandStartRpm = Double(spectrogram.settings.fftIndex1) * spectrogram.settings.hzPerPixel * 60

    static func v2(_ y: Double) -> Double { ((y - bandStartRpm) * 1000).rounded() / 1000 }
    static func v2Pairs(_ key: String) -> [[Double]] {
        Golden.pairs(key).map { [$0[0], ($0[1] * 1000).rounded() / 1000] }
    }

    @Test func readsWavAndBuildsSameSpectrogramGrid() {
        let spectrogram = Self.spectrogram
        #expect(Self.audio.sampleRate == 44100)
        #expect(spectrogram.ffts.count == Int(Golden.number("specCols")))
        #expect(spectrogram.ffts[0].count == Int(Golden.number("specRows")))
        #expect(spectrogram.settings.hzPerPixel == Golden.number("specHzPerPx"))
        #expect(spectrogram.settings.stepLengthSec == Golden.number("specSecPerPx"))
        let sample = Golden.numbers("specSample")
        let last = spectrogram.ffts[spectrogram.ffts.count - 1]
        expectRelClose(spectrogram.ffts[0][0], sample[0], 1e-9)
        expectRelClose(spectrogram.ffts[0][10], sample[1], 1e-9)
        expectRelClose(spectrogram.ffts[100][50], sample[2], 1e-9)
        expectRelClose(last[last.count - 1], sample[3], 1e-9)
    }

    @Test func findsSameRpmPointsShiftedByBandStart() {
        let points = Self.points
        #expect(points.count == Int(Golden.number("rpmCount")))
        #expect(points.prefix(10).map { [$0.x, Self.v2($0.y)] } == Self.v2Pairs("rpmHead"))
        #expect(points.suffix(5).map { [$0.x, Self.v2($0.y)] } == Self.v2Pairs("rpmTail"))
    }

    @Test func producesSameClustersInSameOrder() {
        #expect(Self.clusters.map(\.count) == Golden.ints("clusterSizes"))
        #expect(Self.clusters.map { [$0[0].x, Self.v2($0[0].y)] } == Self.v2Pairs("clusterFirst"))
    }

    @Test func fitsSameDegree5CurveThroughLargestCluster() throws {
        // Stable sort by size (descending) like JS `Array.prototype.sort`.
        let largest = Self.clusters.enumerated()
            .sorted { $0.element.count != $1.element.count ? $0.element.count > $1.element.count : $0.offset < $1.offset }
            .first!.element
        let coefficients = try Polyfit.fit(xs: largest.map(\.x), ys: largest.map(\.y), order: 5)
        let fitAt = Golden.numbers("fitAt")
        expectRelClose(Polyfit.evaluate(coefficients, at: fitAt[0]) - Self.bandStartRpm, fitAt[1], 1e-6, "start")
        expectRelClose(Polyfit.evaluate(coefficients, at: fitAt[2]) - Self.bandStartRpm, fitAt[3], 1e-6, "middle")
        expectRelClose(Polyfit.evaluate(coefficients, at: fitAt[4]) - Self.bandStartRpm, fitAt[5], 1e-6, "end")
        let curve = AudioDyno.fitDrehzahlCurve(largest)
        #expect(curve[0].x == fitAt[0])
        #expect(curve.indices.allSatisfy { $0 == 0 || curve[$0].x - curve[$0 - 1].x == 5 })
    }

    @Test func roundTripsWavEncoding() throws {
        let head = Array(Self.audio.samples.prefix(1000))
        let encoded = try Wav.readMono(Wav.encodeMono(head, sampleRate: Self.audio.sampleRate))
        #expect(encoded.samples == head)
        #expect(encoded.sampleRate == Self.audio.sampleRate)
    }
}

/// Mirror of the TS "SIP reference" suite: the demo recording is the SIP HorsePower Dyno sample run,
/// whose own result (`dyno-acceleration.sip-result.txt`) is an independent reference.
@Suite("SIP reference — demo recording", .serialized)
struct SipReferenceTests {
    static let analysis: AudioDyno.Analysis = try! AudioDyno.analyze(
        AudioParityTests.audio.samples, sampleRate: Double(AudioParityTests.audio.sampleRate)
    )
    static let sip: [(rpm: Double, ps: Double)] = try! String(
        contentsOf: Golden.url("dyno-acceleration.sip-result.txt"), encoding: .utf8
    )
    .split(separator: "\n")
    .filter { $0.first?.isNumber == true && $0.contains(";") }
    .map { line in
        let parts = line.split(separator: ";").map { Double($0.trimmingCharacters(in: .whitespacesAndNewlines))! }
        return (rpm: parts[0], ps: parts[1])
    }

    @Test func picksTheJoinedFiringFrequencyTrack() throws {
        let fundamental = try #require(Self.analysis.fundamental)
        let ys = AudioDyno.fitDrehzahlCurve(Self.analysis.clusters[fundamental]).map(\.y)
        expectRelClose(ys.min()!, Self.sip.map(\.rpm).min()!, 0.1, "start rpm")
        expectRelClose(ys.max()!, Self.sip.map(\.rpm).max()!, 0.05, "end rpm")
    }

    @Test func reproducesTheSipPeak() throws {
        let fundamental = try #require(Self.analysis.fundamental)
        let curve = AudioDyno.fitDrehzahlCurve(Self.analysis.clusters[fundamental])
        let run = curve.enumerated().filter { $0.offset % 20 == 0 }.map { (zeit: $0.element.x, drehzahl: $0.element.y) }
        let power = DynoLogic.getLeistungskurve(
            run, gesamtuebersetzung: 2 * Double.pi * 0.277 * 5.5, radius: 0.277,
            .init(masse: 220, luftdichte: 1.2, cw: 0.5, frontA: 0.75, rollwiderstand: 0.015, steigung: 0)
        )
        let peak = try #require(power.max { $0.leistung < $1.leistung })
        let sipPeak = Self.sip.max { $0.ps < $1.ps }!
        expectRelClose(peak.drehzahl, sipPeak.rpm, 0.05, "peak rpm")
        expectRelClose(DynoLogic.wattToPs(peak.leistung), sipPeak.ps, 0.2, "peak PS")
    }

    @Test func diagnosisCurveIsPhysical() {
        let run = (0..<50).map { (zeit: Double($0) * 100, drehzahl: 4000 + Double($0) * 80) }
        let curve = DynoLogic.diagnosisCurve(drehzahl: run, cw: 0.6, gesamtuebersetzung: 14, gewicht: 170, frontA: 0.6)
        #expect(!curve.isEmpty)
        #expect(curve.allSatisfy { DynoLogic.wattToPs($0.leistung) > 0 && DynoLogic.wattToPs($0.leistung) < 50 })
    }
}
