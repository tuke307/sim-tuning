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

    @Test func findsSameRpmPoints() {
        let points = Self.points
        #expect(points.count == Int(Golden.number("rpmCount")))
        #expect(points.prefix(10).map { [$0.x, $0.y] } == Golden.pairs("rpmHead"))
        #expect(points.suffix(5).map { [$0.x, $0.y] } == Golden.pairs("rpmTail"))
    }

    @Test func producesSameClustersInSameOrder() {
        #expect(Self.clusters.map(\.count) == Golden.ints("clusterSizes"))
        #expect(Self.clusters.map { [$0[0].x, $0[0].y] } == Golden.pairs("clusterFirst"))
    }

    @Test func fitsSameDegree5CurveThroughLargestCluster() throws {
        // Stable sort by size (descending) like JS `Array.prototype.sort`.
        let largest = Self.clusters.enumerated()
            .sorted { $0.element.count != $1.element.count ? $0.element.count > $1.element.count : $0.offset < $1.offset }
            .first!.element
        let coefficients = try Polyfit.fit(xs: largest.map(\.x), ys: largest.map(\.y), order: 5)
        let fitAt = Golden.numbers("fitAt")
        expectRelClose(Polyfit.evaluate(coefficients, at: fitAt[0]), fitAt[1], 1e-6, "start")
        expectRelClose(Polyfit.evaluate(coefficients, at: fitAt[2]), fitAt[3], 1e-6, "middle")
        expectRelClose(Polyfit.evaluate(coefficients, at: fitAt[4]), fitAt[5], 1e-6, "end")
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
