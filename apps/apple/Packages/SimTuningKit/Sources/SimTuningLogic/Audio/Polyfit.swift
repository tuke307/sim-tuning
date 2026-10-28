import Foundation

/// Port of `packages/common/src/audio/polyfit.ts`: least-squares polynomial fit via Householder
/// QR on the Vandermonde matrix (same approach as MathNet `Fit.PolynomialFunc` used by v2).
public enum Polyfit {
    public enum FitError: Error, Equatable {
        case lengthMismatch
        case notEnoughPoints(needed: Int)
    }

    /// Returns coefficients c₀…c_order (ascending powers).
    public static func fit(xs: [Double], ys: [Double], order: Int) throws -> [Double] {
        let m = xs.count
        let n = order + 1
        guard m == ys.count else { throw FitError.lengthMismatch }
        guard m >= n else { throw FitError.notEnoughPoints(needed: n) }

        var a = [[Double]](repeating: [Double](repeating: 0, count: n), count: m)
        var b = [Double](repeating: 0, count: m)
        for i in 0..<m {
            var p = 1.0
            for j in 0..<n {
                a[i][j] = p
                p *= xs[i]
            }
            b[i] = ys[i]
        }

        for k in 0..<n {
            var norm = 0.0
            for i in k..<m { norm += a[i][k] * a[i][k] }
            norm = norm.squareRoot()
            if norm == 0 { continue }
            let alpha = a[k][k] > 0 ? -norm : norm
            var vk = [Double](repeating: 0, count: m - k)
            for i in k..<m { vk[i - k] = a[i][k] }
            vk[0] -= alpha
            var vnorm2 = 0.0
            for value in vk { vnorm2 += value * value }
            if vnorm2 == 0 { continue }
            for j in k..<n {
                var dot = 0.0
                for i in k..<m { dot += vk[i - k] * a[i][j] }
                let f = (2 * dot) / vnorm2
                for i in k..<m { a[i][j] -= f * vk[i - k] }
            }
            var dot = 0.0
            for i in k..<m { dot += vk[i - k] * b[i] }
            let f = (2 * dot) / vnorm2
            for i in k..<m { b[i] -= f * vk[i - k] }
        }

        var coefficients = [Double](repeating: 0, count: n)
        for k in stride(from: n - 1, through: 0, by: -1) {
            var sum = b[k]
            for j in (k + 1)..<max(k + 1, n) { sum -= a[k][j] * coefficients[j] }
            coefficients[k] = sum / a[k][k]
        }
        return coefficients
    }

    /// Evaluates ascending-power coefficients with Horner's scheme.
    public static func evaluate(_ coefficients: [Double], at x: Double) -> Double {
        var y = 0.0
        for c in coefficients.reversed() { y = y * x + c }
        return y
    }
}
