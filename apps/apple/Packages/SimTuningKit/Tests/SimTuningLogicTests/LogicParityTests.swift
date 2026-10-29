import Foundation
import Testing
@testable import SimTuningLogic

/// Swift mirror of `packages/common/src/logic/v2-parity.spec.ts`. Exact equality where the TS
/// spec uses `equal`/`deepEqual`, the same tolerance where it uses `close`.
@Suite("v2 parity — EngineLogic")
struct EngineParityTests {
    let g = Golden.number

    @Test func compressionBoreDisplacementAndKinematics() {
        #expect(EngineLogic.getCompression(hubraum: 50000, brennraum: 17000) == g("compression"))
        #expect(EngineLogic.getCompression(hubraum: 49900, brennraum: 5880) == g("compression2"))
        #expect(EngineLogic.getCylinderHoleDiameter(hubraum: 50000, hub: 17000) == g("cylinderHoleDiameter"))
        #expect(EngineLogic.getCylinderHoleDiameter(hubraum: 49900, hub: 44) == g("cylinderHoleDiameter2"))
        #expect(EngineLogic.getDistanceToOT(pleullaenge: 44, hubradius: 22, deachsierung: 2, kwgrad: 120)
            == g("distanceToOT"))
        #expect(EngineLogic.getDistanceToOT(pleullaenge: 95, hubradius: 22, deachsierung: 2, kwgrad: 67.5)
            == g("distanceToOT2"))
        #expect(EngineLogic.getHubRadius(hub: 44, pleullaenge: 44, deachsierung: 2) == g("hubRadius"))
        #expect(EngineLogic.getHubRadius(hub: 44, pleullaenge: 95, deachsierung: 2) == g("hubRadius2"))
        expectClose(EngineLogic.getKolbenDurchmesser(bohrungsdurchmesser: 38, einbauspiel: 0.03),
                    g("kolbenDurchmesser"))
        #expect(EngineLogic.getKolbenGeschwindigkeit(hub: 44, drehzahl: 8000) == g("kolbenGeschwindigkeit"))
        let grinding = Golden.dict("grinding")
        let ours = EngineLogic.getGrindingDiameters(38)
        expectClose(ours.diameter1, grinding["Diameter1"]!)
        expectClose(ours.diameter4, grinding["Diameter4"]!)
    }

    @Test func portTimingHelpers() {
        #expect(EngineLogic.getPortTimingDifference(inMm: false, vorher: 120, nachher: 130) == g("portTimingDiffDeg"))
        #expect(EngineLogic.getPortTimingDifference(inMm: true, vorher: 120, nachher: 130, pleullaenge: 95,
                                                    hubradius: 22, deachsierung: 2) == g("portTimingDiffMm"))
        let oben = EngineLogic.getSteuerwinkel(vorherSteuerzeit: 120, nachherSteuerzeit: 130,
                                               kolbenoberkante: true, kolbenunterkante: false)
        #expect([oben.oeffnetVorher, oben.schliesstVorher, oben.oeffnetNachher, oben.schliesstNachher]
            == Golden.numbers("steuerwinkelOben"))
        let unten = EngineLogic.getSteuerwinkel(vorherSteuerzeit: 120, nachherSteuerzeit: 130,
                                                kolbenoberkante: false, kolbenunterkante: true)
        #expect([unten.oeffnetVorher, unten.schliesstVorher, unten.oeffnetNachher, unten.schliesstNachher]
            == Golden.numbers("steuerwinkelUnten"))
        #expect([
            EngineLogic.getSteuerwinkelOeffnet(einlass: 135),
            EngineLogic.getSteuerwinkelOeffnet(auslass: 145.5),
            EngineLogic.getSteuerwinkelOeffnet(ueberstroemer: 117),
            EngineLogic.getSteuerwinkelOeffnet(),
        ] == Golden.numbers("swOeffnet"))
        #expect([
            EngineLogic.getSteuerwinkelSchliesst(einlass: 135),
            EngineLogic.getSteuerwinkelSchliesst(auslass: 145.5),
            EngineLogic.getSteuerwinkelSchliesst(ueberstroemer: 117),
            EngineLogic.getSteuerwinkelSchliesst(),
        ] == Golden.numbers("swSchliesst"))
        #expect(EngineLogic.getToDecreasingLength(hubraum: 50000, brennraum: 17000, durchmesser: 38,
                                                  zielVerdichtung: 12) == g("toDecreasingLength"))
        #expect(EngineLogic.getToDecreasingLength(hubraum: 49900, brennraum: 5880, durchmesser: 38,
                                                  zielVerdichtung: 10) == g("toDecreasingLength2"))
        #expect(EngineLogic.getVorauslass(auslassSW: 190, ueberstroemerSW: 125) == g("vorauslass"))
        #expect(EngineLogic.getVorauslass(auslassSW: 145, ueberstroemerSW: 117.25) == g("vorauslass2"))
    }

    @Test func timingDiagramHasTdcOnTop() {
        let diagram = EngineLogic.getSteuerdiagramm(einlass: 135, auslass: 145, ueberstroemer: 117)
        #expect(diagram.size == 600)
        #expect(diagram.lines.count == 6)
        let open = diagram.lines[0]
        let close = diagram.lines[1]
        #expect(open.y2 == close.y2)
        #expect(open.x2 + close.x2 == 2 * diagram.center)
        #expect(open.y2 < diagram.center)
    }
}

@Suite("v2 parity — EinlassLogic")
struct EinlassParityTests {
    let g = Golden.number

    @Test func carburettorSizing() {
        #expect(EinlassLogic.getVergaserDurchmesser(hubvolumen: 50000, resonanzdrehzahl: 8000, widerstandsFaktor: 0.9)
            == g("vergaserD"))
        #expect(EinlassLogic.getVergaserDurchmesser(hubvolumen: 49900, resonanzdrehzahl: 5500, widerstandsFaktor: 0.85)
            == g("vergaserD2"))
        #expect(EinlassLogic.getVergaserHauptduesenDurchmesser(vergasergroesse: 18, widerstandsFaktor: 0.95)
            == g("hauptduese"))
        #expect(EinlassLogic.getVergaserHauptduesenDurchmesser(vergasergroesse: 16.3, widerstandsFaktor: 0.9)
            == g("hauptduese2"))
    }
}

@Suite("v2 parity — AuslassLogic")
struct AuslassParityTests {
    let g = Golden.number

    @Test func singleFormulas() {
        #expect(AuslassLogic.getKruemmerLaenge(kruemmerdurchmesser: 20, drehmomentfaktor: 20, auslassLaenge: 20)
            == g("kruemmerL"))
        #expect(AuslassLogic.getVehiclePortDuration(auslassSteuerzeit: 180, drehzahl: 8000) == g("portDuration"))
    }

    @Test(arguments: [1, 2, 3])
    func expansionChamber(stage: Int) {
        let input = AuslassLogic.AuspuffInput(
            resonanzU: 8000,
            auslass: .init(laengeL: 100, steuerzeitSZ: 190, durchmesserD: 45.83, flaecheA: 20),
            // v2 users typed the speed of sound (548.69 m/s) here; 500 °C gives exactly that.
            auspuff: .init(abgasT: 500, diffusorStage: stage, diffusorW1: 7.5, kruemmerF: 10, kruemmerW: 2,
                           mittelteilF: 2.9, gegenKonusW: 8, endrohrL: 295, endrohrD: 27)
        )
        let result = AuslassLogic.calculateAuspuff(input)
        let expected = Golden.dict("auspuffStage\(stage)")
        let ours = result.byKey
        #expect(Set(expected.keys) == Set(ours.keys))
        for (key, value) in expected {
            #expect(ours[key] == value, "\(key)")
        }
        let geometry = AuslassLogic.getAuspuffGeometry(result, input)
        #expect(geometry.rects.count == 2)
        #expect(geometry.partLabels.count == 5)
    }
}

@Suite("v2 parity — DynoLogic")
struct DynoParityTests {
    @Test func powerFormula() {
        expectClose(DynoLogic.getLeistung(n: 6000, t: 0.5, p: 1.2, cw: 0.6, i: 7.5, r: 0.28, m: 120, c: 0.02,
                                          g: 9.81, aw: 0.6, s: 0), Golden.number("leistung"))
        expectClose(DynoLogic.getLeistung(n: 4500, t: 2, p: 1.18, cw: 0.8, i: 9, r: 0.3, m: 160, c: 0.015,
                                          g: 9.81, aw: 0.7, s: 0.05), Golden.number("leistung2"))
    }
}

/// Mirror of the TS "literature" suite: formulas v2 got wrong, pinned to the sources
/// (Werkstatthandbuch S 51, Bitterli, Rechensheet-2Takt).
@Suite("literature — formulas v2 got wrong")
struct LiteratureTests {
    @Test func displacement() {
        #expect(EngineLogic.getDisplacement(bohrungsdurchmesser: 38, hub: 44) == 49901.1)
        #expect(EngineLogic.getDisplacement(bohrungsdurchmesser: 45, hub: 44) == 69979)
    }

    @Test func intakeResonanceLength() {
        #expect(EinlassLogic.getResonanzLaenge(einlassflaeche: 10, einlasssteuerwinkel: 360,
                                               kurbelgehausevolumen: 290, resonanzdrehzahl: 9500) == 1516.48)
        #expect(EinlassLogic.getResonanzLaenge(einlassflaeche: 3, einlasssteuerwinkel: 135,
                                               kurbelgehausevolumen: 149.7, resonanzdrehzahl: 5500) == 369.76)
    }

    @Test func speedOfSoundAndExhaustResonanceLength() {
        #expect(AuslassLogic.getGasGeschwindigkeit(abgastemperatur: 500) == 548.69)
        #expect(AuslassLogic.getGasGeschwindigkeit(abgastemperatur: 550) == 566.16)
        #expect(AuslassLogic.getResonanzLaenge(auslassSteuerwinkel: 190, schallgeschwindigkeit: 548.69,
                                               resonanzDrehzahl: 8000) == 1085.95)
        #expect(AuslassLogic.getResonanzLaenge(auslassSteuerwinkel: 190, abgasTemperatur: 500,
                                               resonanzDrehzahl: 8000) == 1085.95)
    }

    @Test func headerDiameterFromPortArea() {
        #expect(AuslassLogic.getKruemmerDurchmesser(auslassflaeche: 1100, percentage: 10) == 39.25)
        #expect(AuslassLogic.getKruemmerDurchmesser(auslassflaeche: 1100, percentage: 50) == 45.83)
    }
}
