import Foundation

/// Exhaust calculations, ported 1:1 from `packages/common/src/logic/auslass.ts` (v2 `AuslassLogic`).
public enum AuslassLogic {
    /// Speed of sound in the exhaust gas (m/s) from the gas temperature (°C): `c = √(κ·R·T)` with
    /// κ = 1.32, R = 295 J/(kg·K) (Rechensheet-2Takt, typically 540–560 m/s). v2 used `331 + 0.6·T`.
    public static func getGasGeschwindigkeit(abgastemperatur: Double) -> Double {
        Rounding.roundTo((1.32 * 295 * (abgastemperatur + 273.15)).squareRoot(), 2)
    }

    /// Header (flange) diameter: port area enlarged by `percentage` (Rechensheet-2Takt: 1.1–1.5 ×,
    /// i.e. 10–50 %). v2 truncated `percentage / 100`, so percentages below 100 had no effect.
    public static func getKruemmerDurchmesser(auslassflaeche: Double, percentage: Double = 10) -> Double {
        let factor = 1 + percentage / 100
        return Rounding.roundTo(((auslassflaeche * factor).squareRoot() / Double.pi.squareRoot()) * 2, 2)
    }

    /// Header length = torque factor · header diameter − port length.
    public static func getKruemmerLaenge(
        kruemmerdurchmesser: Double, drehmomentfaktor: Double, auslassLaenge: Double
    ) -> Double {
        Rounding.roundTo(drehmomentfaktor * kruemmerdurchmesser - auslassLaenge, 2)
    }

    /// Tuned length (mm) `l = c·φ / (12·n)` from exhaust timing `φ` (°KW), speed of sound `c`
    /// (m/s) and resonance rpm `n` (Bitterli §7.2; Rechensheet-2Takt). v2 passed °C as `c`.
    public static func getResonanzLaenge(
        auslassSteuerwinkel: Double, schallgeschwindigkeit: Double, resonanzDrehzahl: Double
    ) -> Double {
        Rounding.roundTo(
            ((auslassSteuerwinkel * schallgeschwindigkeit) / ((360 * 2 * resonanzDrehzahl) / 60)) * 1000,
            2
        )
    }

    /// Tuned length (mm) directly from the exhaust gas temperature (°C).
    public static func getResonanzLaenge(
        auslassSteuerwinkel: Double, abgasTemperatur: Double, resonanzDrehzahl: Double
    ) -> Double {
        getResonanzLaenge(auslassSteuerwinkel: auslassSteuerwinkel,
                          schallgeschwindigkeit: getGasGeschwindigkeit(abgastemperatur: abgasTemperatur),
                          resonanzDrehzahl: resonanzDrehzahl)
    }

    /// Port open duration (v2 formula, unrounded).
    public static func getVehiclePortDuration(auslassSteuerzeit: Double, drehzahl: Double) -> Double {
        ((60 * auslassSteuerzeit) / drehzahl) * 360
    }

    // MARK: - Expansion chamber (Auspuff)

    public struct AuspuffInput: Sendable, Equatable {
        public struct Auslass: Sendable, Equatable {
            public var laengeL: Double?
            public var steuerzeitSZ: Double?
            public var durchmesserD: Double?
            public var flaecheA: Double?
            public init(laengeL: Double? = nil, steuerzeitSZ: Double? = nil,
                        durchmesserD: Double? = nil, flaecheA: Double? = nil) {
                self.laengeL = laengeL
                self.steuerzeitSZ = steuerzeitSZ
                self.durchmesserD = durchmesserD
                self.flaecheA = flaecheA
            }
        }

        public struct Auspuff: Sendable, Equatable {
            public var abgasT: Double?
            public var diffusorStage: Int
            public var diffusorW1: Double?
            public var kruemmerF: Double?
            public var kruemmerW: Double?
            public var mittelteilF: Double?
            public var gegenKonusW: Double?
            public var endrohrL: Double?
            public var endrohrD: Double?
            public init(abgasT: Double? = nil, diffusorStage: Int = 0, diffusorW1: Double? = nil,
                        kruemmerF: Double? = nil, kruemmerW: Double? = nil, mittelteilF: Double? = nil,
                        gegenKonusW: Double? = nil, endrohrL: Double? = nil, endrohrD: Double? = nil) {
                self.abgasT = abgasT
                self.diffusorStage = diffusorStage
                self.diffusorW1 = diffusorW1
                self.kruemmerF = kruemmerF
                self.kruemmerW = kruemmerW
                self.mittelteilF = mittelteilF
                self.gegenKonusW = gegenKonusW
                self.endrohrL = endrohrL
                self.endrohrD = endrohrD
            }
        }

        public var resonanzU: Double?
        public var auslass: Auslass
        public var auspuff: Auspuff

        public init(resonanzU: Double?, auslass: Auslass, auspuff: Auspuff) {
            self.resonanzU = resonanzU
            self.auslass = auslass
            self.auspuff = auspuff
        }
    }

    public struct AuspuffResult: Sendable, Equatable {
        public var resonanzL: Double
        public var kruemmerD: Double
        public var kruemmerL: Double
        public var mittelteilD: Double
        public var diffusorD1: Double
        public var diffusorL1: Double
        public var diffusorD2: Double
        public var diffusorL2: Double
        public var diffusorD3: Double
        public var diffusorL3: Double
        public var gegenkonusL: Double
        public var gegenkonusD: Double
        public var mittelteilL: Double

        /// Values keyed like the v2 golden fixture (`ResonanzL`, `KruemmerD`, …).
        public var byKey: [String: Double] {
            [
                "ResonanzL": resonanzL, "KruemmerD": kruemmerD, "KruemmerL": kruemmerL,
                "MittelteilD": mittelteilD, "DiffusorD1": diffusorD1, "DiffusorL1": diffusorL1,
                "DiffusorD2": diffusorD2, "DiffusorL2": diffusorL2, "DiffusorD3": diffusorD3,
                "DiffusorL3": diffusorL3, "GegenkonusL": gegenkonusL, "GegenkonusD": gegenkonusD,
                "MittelteilL": mittelteilL,
            ]
        }
    }

    /// v2 `AuslassLogic.Calculate`. Only the one-stage diffusor is implemented in v2 — stages 2 and
    /// 3 reset every diffusor section to 0; that behaviour is preserved.
    public static func calculateAuspuff(_ input: AuspuffInput) -> AuspuffResult {
        let auslass = input.auslass
        let auspuff = input.auspuff
        let resonanzL = getResonanzLaenge(
            auslassSteuerwinkel: auslass.steuerzeitSZ ?? 0,
            abgasTemperatur: auspuff.abgasT ?? 0,
            resonanzDrehzahl: input.resonanzU ?? 0
        )
        let kruemmerD = auslass.durchmesserD ?? 0
        let kruemmerL = getKruemmerLaenge(
            kruemmerdurchmesser: auslass.durchmesserD ?? 0,
            drehmomentfaktor: auspuff.kruemmerF ?? 0,
            auslassLaenge: auslass.laengeL ?? 0
        )
        let mittelteilD = Rounding.roundTo(
            (((auslass.flaecheA ?? 0) * 4) / Double.pi).squareRoot() * (auspuff.mittelteilF ?? 0),
            2
        )

        var diffusorD1 = 0.0
        var diffusorL1 = 0.0
        if auspuff.diffusorStage == 1 {
            diffusorD1 = Rounding.roundTo(
                2 * tan(((auspuff.kruemmerW ?? 0) * Double.pi) / 360) * kruemmerL + kruemmerD,
                2
            )
            diffusorL1 = Rounding.roundTo(
                (mittelteilD - diffusorD1) / (2 * tan((auspuff.diffusorW1 ?? 0) * ((2 * Double.pi) / 360))),
                2
            )
        }

        let gegenkonusL = Rounding.roundTo(
            (mittelteilD - (auspuff.endrohrD ?? 0)) / (2 * tan(((auspuff.gegenKonusW ?? 0) * Double.pi) / 180)),
            2
        )
        let mittelteilL = Rounding.roundTo(resonanzL - (kruemmerL + diffusorL1 + gegenkonusL / 2), 2)

        return AuspuffResult(
            resonanzL: resonanzL, kruemmerD: kruemmerD, kruemmerL: kruemmerL,
            mittelteilD: mittelteilD, diffusorD1: diffusorD1, diffusorL1: diffusorL1,
            diffusorD2: 0, diffusorL2: 0, diffusorD3: 0, diffusorL3: 0,
            gegenkonusL: gegenkonusL, gegenkonusD: mittelteilD, mittelteilL: mittelteilL
        )
    }

    // MARK: - Chamber drawing geometry

    public enum SegmentKind: String, Sendable { case frame, boundary, dimension }

    public struct Segment: Sendable, Equatable {
        public let kind: SegmentKind
        public let x1: Double
        public let y1: Double
        public let x2: Double
        public let y2: Double
    }

    public struct Rect: Sendable, Equatable {
        public let x: Double
        public let y: Double
        public let width: Double
        public let height: Double
    }

    public enum PartLabelKey: String, Sendable, CaseIterable {
        case kruemmer, konus, mittelteil, gegenkonus, endrohr
    }

    public struct ValueLabel: Sendable, Equatable {
        public let text: String
        public let x: Double
        public let y: Double
    }

    public struct PartLabel: Sendable, Equatable {
        public let key: PartLabelKey
        public let x: Double
        public let y: Double
    }

    public struct AuspuffGeometry: Sendable, Equatable {
        public let width: Double
        public let height: Double
        public let segments: [Segment]
        public let rects: [Rect]
        public let valueLabels: [ValueLabel]
        public let partLabels: [PartLabel]
    }

    /// v2 `ComputeGeometry` + `Draw*` as vector primitives (2000×1000 canvas, px = mm).
    public static func getAuspuffGeometry(_ result: AuspuffResult, _ input: AuspuffInput) -> AuspuffGeometry {
        let width = 2000.0
        let height = 1000.0
        let i = Rounding.truncate
        let w1 = i(result.kruemmerL)
        let w2 = i(result.diffusorL1)
        let w3 = i(result.diffusorL2)
        let w4 = i(result.diffusorL3)
        let w5 = i(result.mittelteilL)
        let w6 = i(result.gegenkonusL)
        let w7 = i(input.auspuff.endrohrL ?? 0)
        // v2: `(int)value * 2` casts first, then doubles; ydiff = height / 2 (integer division).
        func yd(_ d: Double) -> Double { i((i(d) * 2) / 2) }
        let yd1 = yd(result.kruemmerD)
        let yd2 = yd(result.diffusorD1)
        let yd3 = yd(result.diffusorD2)
        let yd4 = yd(result.diffusorD3)
        let yd5 = yd(result.mittelteilD)
        let yd6 = yd(result.gegenkonusD)
        let yd7 = yd(input.auspuff.endrohrD ?? 0)
        let h5 = i(result.mittelteilD) * 2
        let h7 = i(input.auspuff.endrohrD ?? 0) * 2

        let x1 = 100.0
        let x2 = x1 + w1
        let x3 = x2 + w2
        let x4 = x3 + w3
        let x5 = x4 + w4
        let x6 = x5 + w5
        let x7 = x6 + w6
        let ym = height / 2 + height / 4

        let hasD1 = result.diffusorL1 != 0
        let hasD2 = hasD1 && result.diffusorL2 != 0
        let hasD3 = hasD2 && result.diffusorL3 != 0

        var segments: [Segment] = []
        var rects: [Rect] = []
        func line(_ kind: SegmentKind, _ ax: Double, _ ay: Double, _ bx: Double, _ by: Double) {
            segments.append(Segment(kind: kind, x1: ax, y1: ay, x2: bx, y2: by))
        }
        func cone(_ xa: Double, _ ya: Double, _ xb: Double, _ yb: Double) {
            line(.frame, xa, ym - ya, xb, ym - yb)
            line(.frame, xa, ym + ya, xb, ym + yb)
            line(.frame, xa, ym - ya, xa, ym + ya)
        }

        // Framework
        cone(x1, yd1, x2, yd2)
        if hasD1 {
            if hasD2 {
                if hasD3 { cone(x4, yd4, x5, yd5) } else { cone(x3, yd3, x4, yd4) }
            } else {
                cone(x2, yd2, x5, yd5)
            }
        }
        rects.append(Rect(x: x5, y: ym - yd5, width: w5, height: h5))
        line(.frame, x6, ym - yd6, x7, ym - yd7)
        line(.frame, x6, ym + yd6, x7, ym + yd7)
        rects.append(Rect(x: x7, y: ym - yd7, width: w7, height: h7))

        // Length dimensions
        func dim(_ xa: Double, _ ya: Double, _ xb: Double) {
            line(.boundary, xa, ym + ya, xa, ym + 200)
            line(.dimension, xa, ym + 175, xb, ym + 175)
        }
        dim(x1, yd1, x2)
        if hasD1 {
            if hasD2 {
                dim(x2, yd2, x3)
                dim(x4, yd3, x5)
                line(.dimension, x4, ym + 175, x5, ym + 175)
                if hasD3 {
                    line(.boundary, x6, ym + yd4, x6, ym + 200)
                    line(.dimension, x4, ym + 175, x5, ym + 175)
                }
            } else {
                dim(x2, yd2, x3)
            }
        }
        dim(x5, yd5, x6)
        dim(x6, yd6, x7)
        line(.boundary, x7, ym + yd7, x7, ym + 200)
        line(.boundary, x7 + w7, ym + yd7, x7 + w7, ym + 200)
        line(.dimension, x7, ym + 175, x7 + w7, ym + 175)
        line(.dimension, x2, ym + 175, x5, ym + 175)

        // Diameter dimensions (header and stinger)
        line(.boundary, x1, ym - yd1, x1 - 50, ym - yd1)
        line(.boundary, x1, ym + yd1, x1 - 50, ym + yd1)
        line(.dimension, x1 - 35, ym - yd1, x1 - 35, ym + yd1)
        line(.boundary, x7 + w7, ym - yd7, x7 + w7 + 50, ym - yd7)
        line(.boundary, x7 + w7, ym + yd7, x7 + w7 + 50, ym + yd7)
        line(.dimension, x7 + w7 + 35, ym - yd7, x7 + w7 + 35, ym + yd7)

        var valueLabels = [ValueLabel(text: "LK", x: x1 + i(w1 / 2) - 15, y: ym + 200)]
        if hasD1 {
            if hasD2 {
                valueLabels.append(ValueLabel(text: "LD1", x: x2 + i(w2 / 2) - 15, y: ym + 150))
                valueLabels.append(ValueLabel(text: "LD2", x: x3 + i(w3 / 2) - 15, y: ym + 150))
                if hasD3 { valueLabels.append(ValueLabel(text: "LD3", x: x4 + i(w4 / 2) - 15, y: ym + 150)) }
            } else {
                valueLabels.append(ValueLabel(text: "LD", x: x2 + i(w2 / 2) - 15, y: ym + 200))
            }
        }
        valueLabels += [
            ValueLabel(text: "LM", x: x5 + i(w5 / 2) - 15, y: ym + 200),
            ValueLabel(text: "LG", x: x6 + i(w6 / 2) - 15, y: ym + 200),
            ValueLabel(text: "LE", x: x7 + i(w7 / 2) - 15, y: ym + 200),
            ValueLabel(text: "D1", x: x1 - 90, y: ym - 15),
            ValueLabel(text: "D2", x: x7 + w7 + 65, y: ym - 15),
        ]

        return AuspuffGeometry(
            width: width, height: height, segments: segments, rects: rects,
            valueLabels: valueLabels,
            partLabels: [
                PartLabel(key: .kruemmer, x: x1 + 20, y: ym),
                PartLabel(key: .konus, x: x2 + 20, y: ym),
                PartLabel(key: .mittelteil, x: x5 + 20, y: ym),
                PartLabel(key: .gegenkonus, x: x6 + 20, y: ym),
                PartLabel(key: .endrohr, x: x7 + 20, y: ym),
            ]
        )
    }
}
