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
    /// becomes a point. v2 quirk kept: the row index is not offset by the band start.
    public static func calculateRotationalSpeed(_ spectrogram: Spectrogram,
                                                intensityPercent: Double = Defaults.intensity) -> [DrehzahlPoint] {
        let intensity = intensityPercent / 100
        let stepLengthSec = spectrogram.settings.stepLengthSec
        let hzPerPixel = spectrogram.settings.hzPerPixel
        var points: [DrehzahlPoint] = []
        for (col, column) in spectrogram.ffts.enumerated() {
            for row in 0..<column.count where min(column[row] * intensity, 255) == 255 {
                points.append(DrehzahlPoint(
                    x: Rounding.roundTo(Double(col) * stepLengthSec * 1000, 4),
                    y: Rounding.roundTo(Double(row) * hzPerPixel * 60, 4)
                ))
            }
        }
        return points
    }

    /// v2 `CalculateClusters` (minimum 5 points per cluster).
    public static func calculateClusters<T: Point2D>(_ points: [T], epsilon: Double = Defaults.epsilon) -> [[T]] {
        DBSCAN.cluster(points, epsilon: epsilon, minimumPointsPerCluster: Defaults.minimumPointsPerCluster)
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
        public let clusters: [[DrehzahlPoint]]
    }

    /// Runs the whole pipeline in one go — call it off the main actor.
    public static func analyze(_ samples: [Double], sampleRate: Double, options: Options = Options()) throws
        -> Analysis {
        let spectrogram = try calculateSpectrogram(samples, sampleRate: sampleRate, options: options)
        let points = calculateRotationalSpeed(spectrogram, intensityPercent: options.intensity)
        let clusters = calculateClusters(points, epsilon: options.epsilon)
        return Analysis(spectrogram: spectrogram, points: points, clusters: clusters)
    }
}
