import Foundation

/// Intake calculations, ported 1:1 from `packages/common/src/logic/einlass.ts` (v2 `EinlassLogic`).
public enum EinlassLogic {
    /// Resonance length of the intake tract (v2 formula and scaling kept as-is).
    public static func getResonanzLaenge(
        einlassflaeche: Double,
        einlasssteuerwinkel: Double,
        kurbelgehausevolumen: Double,
        resonanzdrehzahl: Double,
        ansaugleitungsdurchmesser: Double
    ) -> Double {
        let winkel2 = einlasssteuerwinkel * einlasssteuerwinkel
        let drehzahl2 = resonanzdrehzahl * resonanzdrehzahl
        let laenge = einlassflaeche
            * (((3_062_500 * winkel2) / drehzahl2) * kurbelgehausevolumen - 1 / ansaugleitungsdurchmesser)
            * 10
        return Rounding.roundTo(laenge, 2)
    }

    /// Carburettor bore: `k · √(Vh/1000 · n)`, default resistance factor 0.9.
    public static func getVergaserDurchmesser(
        hubvolumen: Double, resonanzdrehzahl: Double, widerstandsFaktor: Double = 0.9
    ) -> Double {
        Rounding.roundTo(widerstandsFaktor * ((hubvolumen / 1000) * resonanzdrehzahl).squareRoot(), 2)
    }

    /// Main jet size: `carburettor bore · 5 · k`, rounded to an integer, default k = 0.95.
    public static func getVergaserHauptduesenDurchmesser(
        vergasergroesse: Double, widerstandsFaktor: Double = 0.95
    ) -> Double {
        Rounding.roundTo(vergasergroesse * 5 * widerstandsFaktor, 0)
    }
}
