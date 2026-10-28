import Foundation

/// Port of `packages/common/src/audio/spectrogram.ts` (vendored v2 `Spectrogram` library,
/// originally swharden/Spectrogram, MIT License, Copyright (c) 2020 Scott W Harden). Hann window,
/// magnitude / fftSize, frequency band limited to [minFreq, maxFreq].
public struct SpectrogramSettings: Sendable, Equatable {
    public let sampleRate: Double
    public let fftSize: Int
    public let stepSize: Int
    public let fftIndex1: Int
    public let fftIndex2: Int
    public let height: Int
    public let hzPerPixel: Double
    public let stepLengthSec: Double
    public let window: [Double]

    public enum SettingsError: Error, Equatable { case fftSize, stepSize }

    public init(sampleRate: Double, fftSize: Int, stepSize: Int,
                minFreq: Double = 0, maxFreq: Double = .infinity) throws {
        guard FFT.isPowerOfTwo(fftSize) else { throw SettingsError.fftSize }
        guard stepSize >= 1 else { throw SettingsError.stepSize }
        let minimum = max(minFreq, 0)
        let nyquist = (sampleRate / 2).rounded(.towardZero)
        let hzPerPixel = sampleRate / Double(fftSize)
        let fftIndex1 = minimum == 0 ? 0 : Int((minimum / hzPerPixel).rounded(.towardZero))
        let fftIndex2 = maxFreq >= nyquist ? fftSize / 2 : Int((maxFreq / hzPerPixel).rounded(.towardZero))
        var window = [Double](repeating: 0, count: fftSize)
        for n in 0..<fftSize {
            window[n] = 0.5 - 0.5 * cos((2 * Double.pi * Double(n)) / Double(fftSize - 1))
        }
        self.sampleRate = sampleRate
        self.fftSize = fftSize
        self.stepSize = stepSize
        self.fftIndex1 = fftIndex1
        self.fftIndex2 = fftIndex2
        self.height = fftIndex2 - fftIndex1
        self.hzPerPixel = hzPerPixel
        self.stepLengthSec = Double(stepSize) / sampleRate
        self.window = window
    }
}

public struct Spectrogram: Sendable, Equatable {
    public let settings: SpectrogramSettings
    /// One magnitude column per time step, `settings.height` rows each.
    public let ffts: [[Double]]

    /// Computes every complete FFT window of `audio` (v2 `Spectrogram.Add(audio, process: true)`).
    public static func compute(_ audio: [Double], settings: SpectrogramSettings) -> Spectrogram {
        let fftSize = settings.fftSize
        let stepSize = settings.stepSize
        let height = settings.height
        let fftIndex1 = settings.fftIndex1
        let count = max(0, (audio.count - fftSize) / stepSize)
        var ffts: [[Double]] = []
        ffts.reserveCapacity(count)
        let re = UnsafeMutablePointer<Double>.allocate(capacity: fftSize)
        let im = UnsafeMutablePointer<Double>.allocate(capacity: fftSize)
        defer {
            re.deallocate()
            im.deallocate()
        }
        settings.window.withUnsafeBufferPointer { window in
            audio.withUnsafeBufferPointer { samples in
                for col in 0..<count {
                    let offset = col * stepSize
                    for i in 0..<fftSize {
                        let index = offset + i
                        re[i] = (index < samples.count ? samples[index] : 0) * window[i]
                        im[i] = 0
                    }
                    FFT.forward(re: re, im: im, count: fftSize)
                    var column = [Double](repeating: 0, count: height)
                    for i in 0..<height {
                        let k = fftIndex1 + i
                        column[i] = hypot(re[k], im[k]) / Double(fftSize)
                    }
                    ffts.append(column)
                }
            }
        }
        return Spectrogram(settings: settings, ffts: ffts)
    }
}
