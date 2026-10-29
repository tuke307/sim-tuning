import Foundation

/// Intake calculations, ported 1:1 from `packages/common/src/logic/einlass.ts` (v2 `EinlassLogic`).
public enum EinlassLogic {
    /// Resonance length of the intake tract in mm, from `n = 1750·φ / √(V·l / F)` (Rieck; Bitterli
    /// §5.2; Rechensheet-2Takt) solved for `l = 3 062 500·φ²·F / (n²·V)` [cm]. `F` in cm², `φ` in
    /// °KW, `V` (crankcase) in cm³. v2 multiplied by `V` and subtracted `1/d`; not kept.
    public static func getResonanzLaenge(
        einlassflaeche: Double,
        einlasssteuerwinkel: Double,
        kurbelgehausevolumen: Double,
        resonanzdrehzahl: Double
    ) -> Double {
        let laengeCm = (3_062_500 * einlasssteuerwinkel * einlasssteuerwinkel * einlassflaeche)
            / (resonanzdrehzahl * resonanzdrehzahl * kurbelgehausevolumen)
        return Rounding.roundTo(laengeCm * 10, 2)
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
