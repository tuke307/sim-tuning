import Foundation

/// Dyno calculations, ported 1:1 from `packages/common/src/logic/dyno.ts` (v2 `DynoLogic`).
public enum DynoLogic {
    /// Wheel power from a speed sample. Parameters as in v2: `n` engine speed (U/min), `t` time
    /// (s), `p` air density, `cw` drag coefficient, `i` gear ratio, `r` wheel radius (m), `m` mass
    /// (kg), `c` rolling resistance, `g` gravity, `aw` frontal area (m²), `s` slope.
    ///
    /// v2 quirks kept: the aero term uses km/h, a zero slope is treated as `1`, and the result is
    /// returned in watts.
    public static func getLeistung(
        n: Double, t: Double, p: Double, cw: Double, i: Double, r: Double,
        m: Double, c: Double, g: Double, aw: Double, s: Double
    ) -> Double {
        let v = geschwindigkeit(r: r, n: n, i: i)
        let a = v / t
        let fr = c * m * g
        let fa = m * a
        let vkmh = v * 3.6
        let fw = (p / 2) * cw * aw * (vkmh * vkmh)
        let fs = m * g * (s == 0 ? 1 : s)
        let leistung = (fr + fw + fs + fa) * v
        return ((leistung * 1000) / n) * 2 * Double.pi * (n / 60)
    }

    /// Watt → PS (metric horsepower).
    public static func wattToPs(_ watt: Double) -> Double {
        watt / 735.49875
    }

    static func geschwindigkeit(r: Double, n: Double, i: Double) -> Double {
        2 * r * Double.pi * 60 * (n / i)
    }

    /// v2 `DynoDiagnosisViewModel.RefreshPlot`: power curve (U/min → W) up to the maximum speed,
    /// with the v2 constants (air density 1.2, wheel radius 0.277 m, rolling resistance 0.005).
    public static func diagnosisCurve(
        drehzahl: [(zeit: Double, drehzahl: Double)],
        cw: Double, gesamtuebersetzung: Double, gewicht: Double, frontA: Double
    ) -> [(drehzahl: Double, leistung: Double)] {
        guard let maxDrehzahl = drehzahl.map(\.drehzahl).max(),
              let maxIndex = drehzahl.firstIndex(where: { $0.drehzahl == maxDrehzahl }),
              maxIndex > 1 else { return [] }
        return drehzahl[1..<maxIndex].map { item in
            (
                drehzahl: item.drehzahl,
                leistung: getLeistung(n: item.drehzahl, t: item.zeit / 1000, p: 1.2, cw: cw,
                                      i: gesamtuebersetzung, r: 0.277, m: gewicht, c: 0.005,
                                      g: 9.81, aw: frontA, s: 0)
            )
        }
    }
}
