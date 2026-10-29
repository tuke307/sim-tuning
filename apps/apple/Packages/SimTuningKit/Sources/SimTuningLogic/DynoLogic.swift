import Foundation

/// Dyno calculations, ported 1:1 from `packages/common/src/logic/dyno.ts` (v2 `DynoLogic` + v3
/// wheel power).
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

    // MARK: - Physically correct wheel power (port of `packages/common/src/logic/dyno.ts`, v3)

    /// Driving-resistance parameters: mass (kg), air density (kg/m³), c_w, frontal area (m²),
    /// rolling resistance coefficient, road grade (rise/run).
    public struct FahrwiderstandParams: Sendable, Equatable {
        public var masse: Double
        public var luftdichte: Double
        public var cw: Double
        public var frontA: Double
        public var rollwiderstand: Double
        public var steigung: Double
        public var gravity: Double

        public init(masse: Double, luftdichte: Double, cw: Double, frontA: Double,
                    rollwiderstand: Double, steigung: Double, gravity: Double = 9.81) {
            self.masse = masse
            self.luftdichte = luftdichte
            self.cw = cw
            self.frontA = frontA
            self.rollwiderstand = rollwiderstand
            self.steigung = steigung
            self.gravity = gravity
        }
    }

    /// Road speed (m/s) from engine speed (U/min), overall gear ratio and wheel radius (m).
    public static func drehzahlToGeschwindigkeit(_ drehzahl: Double, gesamtuebersetzung: Double,
                                                 radius: Double) -> Double {
        (2 * Double.pi * radius * drehzahl) / (60 * gesamtuebersetzung)
    }

    /// Wheel power in watt: (rolling + aerodynamic + slope + inertial force) · v.
    public static func getRadleistung(v: Double, a: Double, _ p: FahrwiderstandParams) -> Double {
        let fr = p.rollwiderstand * p.masse * p.gravity
        let fw = 0.5 * p.luftdichte * p.cw * p.frontA * v * v
        let fs = p.masse * p.gravity * p.steigung
        let fa = p.masse * a
        return (fr + fw + fs + fa) * v
    }

    /// Power curve from an RPM run (`zeit` in ms): speed from the gearing, acceleration by central
    /// differences of neighbouring samples. Points without a usable Δt are skipped.
    public static func getLeistungskurve(
        _ run: [(zeit: Double, drehzahl: Double)], gesamtuebersetzung: Double, radius: Double,
        _ params: FahrwiderstandParams
    ) -> [(drehzahl: Double, leistung: Double)] {
        guard run.count >= 3 else { return [] }
        let speed = { drehzahlToGeschwindigkeit($0, gesamtuebersetzung: gesamtuebersetzung, radius: radius) }
        return (1..<(run.count - 1)).compactMap { i in
            let dt = (run[i + 1].zeit - run[i - 1].zeit) / 1000
            guard dt > 0 else { return nil }
            let a = (speed(run[i + 1].drehzahl) - speed(run[i - 1].drehzahl)) / dt
            let watt = getRadleistung(v: speed(run[i].drehzahl), a: a, params)
            return watt.isFinite ? (drehzahl: run[i].drehzahl, leistung: watt) : nil
        }
    }

    /// v2 `DynoDiagnosisViewModel.RefreshPlot` range (curve up to its RPM maximum) with the v3
    /// physics, like the web `computePowerCurve`: v2 `getLeistung` multiplied by 60 instead of
    /// dividing and used the absolute time as Δt (≈10¹⁴ PS). Constants as in v2: wheel radius
    /// 0.277 m, rolling resistance 0.005, flat road.
    public static func diagnosisCurve(
        drehzahl: [(zeit: Double, drehzahl: Double)],
        cw: Double, gesamtuebersetzung: Double, gewicht: Double, frontA: Double, luftdichte: Double = 1.2
    ) -> [(drehzahl: Double, leistung: Double)] {
        guard drehzahl.count >= 3,
              let maxIndex = drehzahl.indices.max(by: { drehzahl[$0].drehzahl < drehzahl[$1].drehzahl })
        else { return [] }
        return getLeistungskurve(
            Array(drehzahl[...maxIndex]), gesamtuebersetzung: gesamtuebersetzung, radius: 0.277,
            FahrwiderstandParams(masse: gewicht, luftdichte: luftdichte, cw: cw, frontA: frontA,
                                 rollwiderstand: 0.005, steigung: 0)
        )
    }
}
