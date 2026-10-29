import Foundation

/// Port of `packages/common/src/audio/fft.ts`: in-place iterative radix-2 FFT (forward,
/// unnormalised) — same transform and twiddle recurrence as FftSharp `FFT.Forward` used by v2, so
/// the results match the TS/.NET implementations to the last bits. (Accelerate's vDSP uses a
/// different twiddle table and therefore is not used here.)
public enum FFT {
    public struct SizeError: Error, Equatable {}

    public static func isPowerOfTwo(_ value: Int) -> Bool {
        value > 0 && (value & (value - 1)) == 0
    }

    /// v2 `Functions.GetPowersOf2(startN, maxN)`: 2^startN … 2^(maxN−1).
    public static func powersOf2(start: Int, max: Int) -> [Int] {
        guard start < max else { return [] }
        return (start..<max).map { 1 << $0 }
    }

    public static func forward(re: inout [Double], im: inout [Double]) throws {
        let n = re.count
        guard n == im.count, isPowerOfTwo(n) else { throw SizeError() }
        re.withUnsafeMutableBufferPointer { rp in
            im.withUnsafeMutableBufferPointer { ip in
                forward(re: rp.baseAddress!, im: ip.baseAddress!, count: n)
            }
        }
    }

    /// Pointer variant used by the spectrogram hot loop. `count` must be a power of two.
    public static func forward(re: UnsafeMutablePointer<Double>, im: UnsafeMutablePointer<Double>, count n: Int) {
        var j = 0
        for i in 1..<n {
            var bit = n >> 1
            while j & bit != 0 {
                j ^= bit
                bit >>= 1
            }
            j ^= bit
            if i < j {
                swap(&re[i], &re[j])
                swap(&im[i], &im[j])
            }
        }

        var len = 2
        while len <= n {
            let angle = (-2 * Double.pi) / Double(len)
            let wRe = cos(angle)
            let wIm = sin(angle)
            let half = len >> 1
            var start = 0
            while start < n {
                var curRe = 1.0
                var curIm = 0.0
                for k in 0..<half {
                    let a = start + k
                    let b = a + half
                    let tRe = re[b] * curRe - im[b] * curIm
                    let tIm = re[b] * curIm + im[b] * curRe
                    re[b] = re[a] - tRe
                    im[b] = im[a] - tIm
                    re[a] = re[a] + tRe
                    im[a] = im[a] + tIm
                    let nextRe = curRe * wRe - curIm * wIm
                    curIm = curRe * wIm + curIm * wRe
                    curRe = nextRe
                }
                start += len
            }
            len <<= 1
        }
    }
}
