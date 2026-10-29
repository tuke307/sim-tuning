import Foundation

/// Engine calculations, ported 1:1 from `packages/common/src/logic/engine.ts` (itself a port of
/// v2 `SimTuning.Core.ModuleLogic.EngineLogic`). Units follow the v2 base units (mm, mm³, °KW,
/// U/min). Known v2 quirks are kept on purpose and pinned by golden tests.
public enum EngineLogic {
    /// Verdichtung ε = (Vh + Vc) / Vc, rounded to 1 decimal.
    public static func getCompression(hubraum: Double, brennraum: Double) -> Double {
        Rounding.roundTo((hubraum + brennraum) / brennraum, 1)
    }

    /// Bohrungsdurchmesser d = 2·√(Vh / h) / √π, rounded to 2 decimals.
    public static func getCylinderHoleDiameter(hubraum: Double, hub: Double) -> Double {
        Rounding.roundTo((2 * (hubraum / hub).squareRoot()) / Double.pi.squareRoot(), 2)
    }

    /// Displacement `π·d²/4·h` (mm³ from mm), rounded to 1 decimal. v2 omitted the square.
    public static func getDisplacement(bohrungsdurchmesser: Double, hub: Double) -> Double {
        Rounding.roundTo(((Double.pi * bohrungsdurchmesser * bohrungsdurchmesser) / 4) * hub, 1)
    }

    /// Piston distance below TDC (mm) for a crank angle in °KW, with connecting rod and offset.
    public static func getDistanceToOT(
        pleullaenge: Double, hubradius: Double, deachsierung: Double, kwgrad: Double
    ) -> Double {
        let kw = (kwgrad / 180) * Double.pi * 1
        let beta = asin(deachsierung / (pleullaenge + hubradius))
        let lr = pleullaenge + hubradius
        let inner = sin(beta + kw) * hubradius - deachsierung
        let mmVorOt = (lr * lr - deachsierung * deachsierung).squareRoot()
            - (hubradius * cos(beta + kw) + (pleullaenge * pleullaenge - inner * inner).squareRoot())
        return Rounding.roundTo(mmVorOt, 2)
    }

    public struct GrindingDiameters: Sendable, Equatable {
        public let diameter1: Double
        public let diameter2: Double
        public let diameter3: Double
        public let diameter4: Double
    }

    /// Oversize (Schleifmaß) bores in 0.2 mm steps.
    public static func getGrindingDiameters(_ diameter: Double) -> GrindingDiameters {
        GrindingDiameters(
            diameter1: diameter + 0.2,
            diameter2: diameter + 0.4,
            diameter3: diameter + 0.6,
            diameter4: diameter + 0.8
        )
    }

    /// Crank radius from stroke, connecting rod length and offset (Deachsierung).
    public static func getHubRadius(hub: Double, pleullaenge: Double, deachsierung: Double) -> Double {
        let d4 = 4 * deachsierung
        let numerator = hub * abs(d4 * d4 + hub * hub - 4 * (pleullaenge * pleullaenge)).squareRoot()
        let denominator = abs(4 * (hub * hub) - 16 * (pleullaenge * pleullaenge)).squareRoot()
        return Rounding.roundTo(numerator / denominator, 2)
    }

    /// Piston diameter = bore − clearance/100 (clearance in 1/100 mm).
    public static func getKolbenDurchmesser(bohrungsdurchmesser: Double, einbauspiel: Double) -> Double {
        bohrungsdurchmesser - einbauspiel / 100
    }

    /// Mean piston speed `h·n/30` (v2 formula), rounded to 2 decimals.
    public static func getKolbenGeschwindigkeit(hub: Double, drehzahl: Double) -> Double {
        Rounding.roundTo((hub * drehzahl) / 30, 2)
    }

    /// Difference of two port timings, in °KW or (if `inMm`) in mm of piston travel.
    public static func getPortTimingDifference(
        inMm: Bool, vorher: Double, nachher: Double,
        pleullaenge: Double = 0, hubradius: Double = 0, deachsierung: Double = 0
    ) -> Double {
        var before = vorher
        var after = nachher
        if inMm {
            before = getDistanceToOT(pleullaenge: pleullaenge, hubradius: hubradius,
                                     deachsierung: deachsierung, kwgrad: before)
            after = getDistanceToOT(pleullaenge: pleullaenge, hubradius: hubradius,
                                    deachsierung: deachsierung, kwgrad: after)
        }
        return Rounding.roundTo(abs(after - before), 2)
    }

    public struct SteuerwinkelResult: Sendable, Equatable {
        public let oeffnetVorher: Double
        public let schliesstVorher: Double
        public let oeffnetNachher: Double
        public let schliesstNachher: Double
    }

    /// Opening/closing crank angles before and after a timing change.
    public static func getSteuerwinkel(
        vorherSteuerzeit: Double, nachherSteuerzeit: Double,
        kolbenoberkante: Bool, kolbenunterkante: Bool
    ) -> SteuerwinkelResult {
        if kolbenunterkante {
            return SteuerwinkelResult(
                oeffnetVorher: 180 - vorherSteuerzeit / 2,
                schliesstVorher: 180 + vorherSteuerzeit / 2,
                oeffnetNachher: 180 - nachherSteuerzeit / 2,
                schliesstNachher: 180 + nachherSteuerzeit / 2
            )
        }
        if kolbenoberkante {
            return SteuerwinkelResult(
                oeffnetVorher: 360 - vorherSteuerzeit / 2,
                schliesstVorher: 0 + vorherSteuerzeit / 2,
                oeffnetNachher: 360 - nachherSteuerzeit / 2,
                schliesstNachher: 0 + nachherSteuerzeit / 2
            )
        }
        return SteuerwinkelResult(oeffnetVorher: 0, schliesstVorher: 0, oeffnetNachher: 0, schliesstNachher: 0)
    }

    /// Opening angle; the first non-zero timing wins (intake, exhaust, transfer), like v2.
    public static func getSteuerwinkelOeffnet(
        einlass: Double = 0, auslass: Double = 0, ueberstroemer: Double = 0
    ) -> Double {
        var angle = 0.0
        if einlass != 0 { angle = 360 - einlass / 2 }
        else if auslass != 0 { angle = 180 - auslass / 2 }
        else if ueberstroemer != 0 { angle = 180 - ueberstroemer / 2 }
        return Rounding.roundTo(angle, 2)
    }

    /// Closing angle; the first non-zero timing wins (intake, exhaust, transfer), like v2.
    public static func getSteuerwinkelSchliesst(
        einlass: Double = 0, auslass: Double = 0, ueberstroemer: Double = 0
    ) -> Double {
        var angle = 0.0
        if einlass != 0 { angle = einlass / 2 }
        else if auslass != 0 { angle = 180 + auslass / 2 }
        else if ueberstroemer != 0 { angle = 180 + ueberstroemer / 2 }
        return Rounding.roundTo(angle, 2)
    }

    /// How much (mm) to machine off the cylinder to reach a target compression ratio.
    public static func getToDecreasingLength(
        hubraum: Double, brennraum: Double, durchmesser: Double, zielVerdichtung: Double
    ) -> Double {
        let d2 = durchmesser * durchmesser
        let abdrehen = (4 * (brennraum * zielVerdichtung - brennraum - hubraum))
            / (Double.pi * (d2 * zielVerdichtung - d2))
        return Rounding.roundTo(abdrehen, 2)
    }

    /// Vorauslass = (exhaust timing − transfer timing) / 2.
    public static func getVorauslass(auslassSW: Double, ueberstroemerSW: Double) -> Double {
        Rounding.roundTo((auslassSW - ueberstroemerSW) / 2, 2)
    }

    // MARK: - Steuerdiagramm geometry

    public enum SteuerdiagrammPort: String, Sendable, CaseIterable {
        case einlass, auslass, ueberstroemer
    }

    public enum SteuerdiagrammEdge: String, Sendable {
        case oeffnet, schliesst
    }

    public struct SteuerdiagrammLine: Sendable, Equatable {
        public let port: SteuerdiagrammPort
        public let edge: SteuerdiagrammEdge
        public let angle: Double
        public let x1: Double
        public let y1: Double
        public let x2: Double
        public let y2: Double
    }

    public struct SteuerdiagrammLabel: Sendable, Equatable {
        public let text: String
        public let x: Double
        public let y: Double
    }

    public struct SteuerdiagrammGeometry: Sendable, Equatable {
        public let size: Double
        public let center: Double
        public let radius: Double
        public let lines: [SteuerdiagrammLine]
        public let labels: [SteuerdiagrammLabel]
    }

    static let diagramDiameter = 500.0
    static let diagramMargin = 50.0

    /// v2 `Functions.GetPointOnCircle` (integer pixel coordinates, y pointing down).
    public static func getPointOnCircle(angle: Double, radius: Double) -> (x: Double, y: Double) {
        let xr = radius * cos((angle * Double.pi) / 180)
        let yr = radius * sin((angle * Double.pi) / 180)
        return (
            x: Rounding.toInt32(radius + xr),
            y: yr >= 0 ? Rounding.toInt32(radius - yr) : Rounding.toInt32(radius + abs(yr))
        )
    }

    /// Same picture as v2 `GetSteuerdiagramm` (600×600 px), rotated so TDC (OT) is on top.
    public static func getSteuerdiagramm(einlass: Double, auslass: Double, ueberstroemer: Double)
        -> SteuerdiagrammGeometry {
        let radius = diagramDiameter / 2
        let size = diagramDiameter + diagramMargin * 2
        let center = size / 2
        func rotate(_ x: Double, _ y: Double) -> (x: Double, y: Double) {
            (x: center + (y - center), y: center - (x - center))
        }
        let edges: [(SteuerdiagrammPort, SteuerdiagrammEdge, Double)] = [
            (.einlass, .oeffnet, getSteuerwinkelOeffnet(einlass: einlass)),
            (.einlass, .schliesst, getSteuerwinkelSchliesst(einlass: einlass)),
            (.auslass, .oeffnet, getSteuerwinkelOeffnet(auslass: auslass)),
            (.auslass, .schliesst, getSteuerwinkelSchliesst(auslass: auslass)),
            (.ueberstroemer, .oeffnet, getSteuerwinkelOeffnet(ueberstroemer: ueberstroemer)),
            (.ueberstroemer, .schliesst, getSteuerwinkelSchliesst(ueberstroemer: ueberstroemer)),
        ]
        let start = rotate(center, center)
        let lines = edges.map { port, edge, angle -> SteuerdiagrammLine in
            let p = getPointOnCircle(angle: angle, radius: radius)
            let end = rotate(p.x + diagramMargin, p.y + diagramMargin)
            return SteuerdiagrammLine(port: port, edge: edge, angle: angle,
                                      x1: start.x, y1: start.y, x2: end.x, y2: end.y)
        }
        return SteuerdiagrammGeometry(
            size: size, center: center, radius: radius, lines: lines,
            labels: [
                SteuerdiagrammLabel(text: "OT", x: center + 10, y: 30),
                SteuerdiagrammLabel(text: "UT", x: center - 40, y: 580),
            ]
        )
    }
}
