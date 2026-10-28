import Foundation

/// Rounding helpers that reproduce the TypeScript port (`packages/common/src/utils/round.util.ts`)
/// bit for bit, which itself reproduces .NET `Math.Round` (banker's rounding) used by v2.
public enum Rounding {
    /// JavaScript `Math.round`: nearest integer, ties toward +∞.
    @inlinable
    public static func jsRound(_ value: Double) -> Double {
        guard value.isFinite else { return value }
        let floor = value.rounded(.down)
        // `value - floor` is exact for every finite double, so the tie test is exact as well.
        return value - floor >= 0.5 ? floor + 1 : floor
    }

    /// Round half to even, matching .NET `Math.Round(double)`.
    @inlinable
    public static func roundHalfEven(_ value: Double) -> Double {
        guard value.isFinite else { return value }
        if abs(value.truncatingRemainder(dividingBy: 1)) == 0.5 {
            return 2 * jsRound(value / 2)
        }
        return jsRound(value)
    }

    /// Exact powers of ten (what JS `10 ** digits` yields for small integer digits).
    @usableFromInline
    static let powersOf10: [Double] = [1, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11, 1e12,
                                       1e13, 1e14, 1e15]

    /// .NET `Math.Round(value, digits)` (MidpointRounding.ToEven) semantics.
    @inlinable
    public static func roundTo(_ value: Double, _ digits: Int) -> Double {
        guard value.isFinite else { return value }
        if digits <= 0 { return roundHalfEven(value) }
        // .NET skips rounding when the value cannot carry more fractional precision.
        if abs(value) >= 1e16 { return value }
        let power10 = digits < powersOf10.count ? powersOf10[digits] : pow(10, Double(digits))
        return roundHalfEven(value * power10) / power10
    }

    /// C# `(int)x`: truncation toward zero (kept as Double like the TS port).
    @inlinable
    public static func truncate(_ value: Double) -> Double {
        value.rounded(.towardZero)
    }

    /// C# `Convert.ToInt32(double)`: banker's rounding to an integer.
    @inlinable
    public static func toInt32(_ value: Double) -> Double {
        roundHalfEven(value)
    }
}
