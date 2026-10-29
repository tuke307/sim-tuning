import Foundation

/// Audio dyno pipeline, port of `packages/common/src/audio/rotational-speed.ts` (v2 `AudioLogic`
/// + `DynoAudioViewModel`): spectrogram → "hot" bins (magnitude·intensity ≥ 255) → (ms, U/min)
/// points → DBSCAN clusters → degree-5 polynomial over the chosen cluster, sampled every 5 ms.
public enum AudioDyno {
    /// v2 UI defaults: 3000–12000 U/min, ε = 160, FFT 2^15, intensity 100 %.
    public enum Defaults {
        public static let frequenzbeginn = 3000.0
        public static let frequenzende = 12000.0
        public static let epsilon = 160.0
        public static let fftSize = 32768
        public static let intensity = 100.0
        public static let minimumPointsPerCluster = 5
        public static let fitOrder = 5
        public static let fitStepMs = 5.0
    }

    /// FFT sizes offered by v2 (2^13 … 2^17).
    public static let fftSizes = [8192, 16384, 32768, 65536, 131072]

    public struct DrehzahlPoint: Point2D, Equatable, Hashable {
        /// Time in ms.
        public let x: Double
        /// Engine speed in U/min (single-cylinder: 1 Hz = 60 U/min).
        public let y: Double
        public init(x: Double, y: Double) {
            self.x = x
            self.y = y
        }
    }

    public struct Options: Sendable, Equatable {
        public var fftSize: Int
        public var frequenzbeginn: Double
        public var frequenzende: Double
        public var intensity: Double
        public var epsilon: Double

        public init(fftSize: Int = Defaults.fftSize, frequenzbeginn: Double = Defaults.frequenzbeginn,
                    frequenzende: Double = Defaults.frequenzende, intensity: Double = Defaults.intensity,
                    epsilon: Double = Defaults.epsilon) {
            self.fftSize = fftSize
            self.frequenzbeginn = frequenzbeginn
            self.frequenzende = frequenzende
            self.intensity = intensity
            self.epsilon = epsilon
        }
    }

    /// v2 `AudioLogic.CalculateSpectrogram` (stepSize = samples / 1000, band = rpm / 60 Hz).
    public static func calculateSpectrogram(_ samples: [Double], sampleRate: Double,
                                            options: Options = Options()) throws -> Spectrogram {
        let minFreq = (options.frequenzbeginn / 60).rounded(.towardZero)
        let maxFreq = (options.frequenzende / 60).rounded(.towardZero)
        let stepSize = max(1, samples.count / 1000)
        let settings = try SpectrogramSettings(sampleRate: sampleRate, fftSize: options.fftSize,
                                               stepSize: stepSize, minFreq: minFreq, maxFreq: maxFreq)
        return Spectrogram.compute(samples, settings: settings)
    }

    /// v2 `CalculateRotationalSpeed`: every bin whose magnitude × intensity saturates at 255
    /// becomes a point. The bin index is offset by the band start (`fftIndex1`); v2 omitted it and
    /// reported every speed ≈ `frequenzbeginn` too low.
    public static func calculateRotationalSpeed(_ spectrogram: Spectrogram,
                                                intensityPercent: Double = Defaults.intensity) -> [DrehzahlPoint] {
        let intensity = intensityPercent / 100
        let stepLengthSec = spectrogram.settings.stepLengthSec
        let hzPerPixel = spectrogram.settings.hzPerPixel
        let bandStart = spectrogram.settings.fftIndex1
        var points: [DrehzahlPoint] = []
        for (col, column) in spectrogram.ffts.enumerated() {
            for row in 0..<column.count where min(column[row] * intensity, 255) == 255 {
                points.append(DrehzahlPoint(
                    x: Rounding.roundTo(Double(col) * stepLengthSec * 1000, 4),
                    y: Rounding.roundTo(Double(bandStart + row) * hzPerPixel * 60, 4)
                ))
            }
        }
        return points
    }

    /// v2 `CalculateClusters` (minimum 5 points per cluster).
    public static func calculateClusters<T: Point2D>(_ points: [T], epsilon: Double = Defaults.epsilon) -> [[T]] {
        DBSCAN.cluster(points, epsilon: epsilon, minimumPointsPerCluster: Defaults.minimumPointsPerCluster)
    }

    /// Track joining tolerances: time overlap/gap between clusters, relative rpm jump and the window
    /// (ms) at each cluster end whose median speed is compared.
    public enum TrackDefaults {
        public static let maxGapMs = 500.0
        public static let maxJumpRatio = 0.1
        public static let edgeMs = 200.0
    }

    private struct TrackEnds {
        var start: Double
        var end: Double
        var rpmStart: Double
        var rpmEnd: Double
    }

    private static func median(_ values: [Double]) -> Double {
        let sorted = values.sorted()
        let mid = sorted.count / 2
        return sorted.count % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2
    }

    private static func trackEnds(_ points: [DrehzahlPoint]) -> TrackEnds {
        let xs = points.map(\.x)
        let start = xs.min() ?? 0
        let end = xs.max() ?? 0
        return TrackEnds(
            start: start, end: end,
            rpmStart: median(points.filter { $0.x <= start + TrackDefaults.edgeMs }.map(\.y)),
            rpmEnd: median(points.filter { $0.x >= end - TrackDefaults.edgeMs }.map(\.y))
        )
    }

    /// Joins DBSCAN clusters that continue each other into one engine-speed track (port of
    /// `mergeClusterTracks`). DBSCAN uses one ε for ms and U/min, so one pull often breaks into
    /// several clusters; a cluster is appended to the track whose end it starts near in time and
    /// speed. Tracks are returned by start time.
    public static func mergeClusterTracks(_ clusters: [[DrehzahlPoint]]) -> [[DrehzahlPoint]] {
        // Stable sort by start time (ties keep the DBSCAN order, like the TypeScript port).
        let ordered = clusters.filter { !$0.isEmpty }
            .map { (points: $0, ends: trackEnds($0)) }
            .enumerated()
            .sorted { lhs, rhs in
                lhs.element.ends.start == rhs.element.ends.start
                    ? lhs.offset < rhs.offset
                    : lhs.element.ends.start < rhs.element.ends.start
            }
            .map(\.element)
        var tracks: [(points: [DrehzahlPoint], ends: TrackEnds)] = []
        for cluster in ordered {
            var best: Int?
            var bestJump = Double.infinity
            for (index, track) in tracks.enumerated() {
                guard cluster.ends.start >= track.ends.start,
                      abs(cluster.ends.start - track.ends.end) <= TrackDefaults.maxGapMs else { continue }
                let jump = abs(cluster.ends.rpmStart - track.ends.rpmEnd) / track.ends.rpmEnd
                if jump <= TrackDefaults.maxJumpRatio && jump < bestJump {
                    best = index
                    bestJump = jump
                }
            }
            if let best, cluster.ends.end > tracks[best].ends.end {
                tracks[best].points += cluster.points
                tracks[best].ends.end = cluster.ends.end
                tracks[best].ends.rpmEnd = cluster.ends.rpmEnd
            } else {
                tracks.append(cluster)
            }
        }
        return tracks.map(\.points)
    }

    /// Index of the track most likely to be the firing frequency: among tracks lasting at least
    /// half as long as the longest, the one with the lowest median speed (harmonics are often
    /// the larger clusters). `nil` when there are no tracks.
    public static func pickFundamentalTrack(_ tracks: [[DrehzahlPoint]]) -> Int? {
        let spans = tracks.map { track -> Double in
            guard track.count >= Defaults.minimumPointsPerCluster else { return 0 }
            let xs = track.map(\.x)
            return (xs.max() ?? 0) - (xs.min() ?? 0)
        }
        let longest = spans.max() ?? 0
        guard longest > 0 else { return nil }
        return tracks.indices
            .filter { spans[$0] >= longest / 2 }
            .min { median(tracks[$0].map(\.y)) < median(tracks[$1].map(\.y)) }
    }

    /// v2 `LoadSpecificCluster`: fits a degree-5 polynomial through a cluster and samples it every
    /// 5 ms from the first to the last point (exclusive), producing the stored `Drehzahl` curve.
    public static func fitDrehzahlCurve(_ cluster: [DrehzahlPoint], order: Int = Defaults.fitOrder,
                                        stepMs: Double = Defaults.fitStepMs) -> [DrehzahlPoint] {
        guard cluster.count > order,
              let coefficients = try? Polyfit.fit(xs: cluster.map(\.x), ys: cluster.map(\.y), order: order)
        else { return [] }
        var xMin = Double.infinity
        var xMax = -Double.infinity
        for p in cluster {
            xMin = min(xMin, p.x)
            xMax = max(xMax, p.x)
        }
        var curve: [DrehzahlPoint] = []
        var x = xMin
        while x < xMax {
            curve.append(DrehzahlPoint(x: x, y: Polyfit.evaluate(coefficients, at: x)))
            x += stepMs
        }
        return curve
    }

    public struct Analysis: Sendable, Equatable {
        public let spectrogram: Spectrogram
        public let points: [DrehzahlPoint]
        /// Engine-speed tracks: DBSCAN clusters joined by `mergeClusterTracks`.
        public let clusters: [[DrehzahlPoint]]
        /// Suggested track (`pickFundamentalTrack`).
        public let fundamental: Int?
    }

    /// Runs the whole pipeline in one go — call it off the main actor.
    public static func analyze(_ samples: [Double], sampleRate: Double, options: Options = Options()) throws
        -> Analysis {
        let spectrogram = try calculateSpectrogram(samples, sampleRate: sampleRate, options: options)
        let points = calculateRotationalSpeed(spectrogram, intensityPercent: options.intensity)
        let clusters = mergeClusterTracks(calculateClusters(points, epsilon: options.epsilon))
        return Analysis(spectrogram: spectrogram, points: points, clusters: clusters,
                        fundamental: pickFundamentalTrack(clusters))
    }
}
