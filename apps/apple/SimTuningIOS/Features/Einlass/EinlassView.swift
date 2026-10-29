import SimTuningAPI
import SimTuningLogic
import SwiftUI

struct EinlassView: View {
    var body: some View {
        NavigationStack {
            SheetList(module: .einlass) {
                NavigationLink {
                    EinlassKanalView()
                } label: {
                    Label("Kanal", systemImage: "point.topleft.down.to.point.bottomright.curvepath")
                }
                NavigationLink {
                    VergaserView()
                } label: {
                    Label("Vergaser", systemImage: "drop")
                }
            }
            .moduleChrome("Einlass")
        }
    }
}

/// v2 `EinlassKanalViewModel`: resonance length from port area (cm²), timing (°KW), crankcase
/// volume (cm³) and resonance rpm, `l = 3 062 500·φ²·F / (n²·V)` (Rieck, Rechensheet-2Takt).
struct EinlassKanalView: View {
    @Environment(AppModel.self) private var model
    @State private var flaecheMm2: Double?
    @State private var steuerwinkel: Double?
    @State private var kurbelgehaeuseCm3: Double?
    @State private var resonanzU: Double?

    private var resonanzlaenge: Double? {
        guard let flaecheMm2, let steuerwinkel, let kurbelgehaeuseCm3, let resonanzU,
              resonanzU != 0, kurbelgehaeuseCm3 != 0 else { return nil }
        return EinlassLogic.getResonanzLaenge(
            einlassflaeche: flaecheMm2 / 100, einlasssteuerwinkel: steuerwinkel,
            kurbelgehausevolumen: kurbelgehaeuseCm3, resonanzdrehzahl: resonanzU
        )
    }

    var body: some View {
        SheetForm(module: .einlass, subsheet: 1, subtitle: "Kanal") {
            Section("Eingaben") {
                NumberField("Einlassfläche", value: $flaecheMm2, unit: "mm²")
                NumberField("Einlasssteuerwinkel", value: $steuerwinkel, unit: "°KW")
                NumberField("Kurbelgehäusevolumen", value: $kurbelgehaeuseCm3, unit: "cm³")
                NumberField("Resonanzdrehzahl", value: $resonanzU, unit: "U/min")
            }
            Section {
                ResultRow(title: "Resonanzlänge", value: model.format(resonanzlaenge, unit: "mm"))
            } header: {
                Text("Ergebnis")
            } footer: {
                Text("Nach Rieck: n = 1750·φ / √(V·l / F).")
            }
            VehicleActionsSection(apply: apply, save: save)
        }
        .moduleChrome("Kanal")
        .onAppear { if resonanzU == nil { apply() } }
    }

    private func apply() {
        guard let motor = model.selectedVehicle?.motor else { return }
        flaecheMm2 = motor.einlass?.flaecheA
        steuerwinkel = motor.einlass?.steuerzeitSZ
        kurbelgehaeuseCm3 = motor.kurbelgehaeuseV.map { $0 / 1000 }
        resonanzU = motor.resonanzU
    }

    private func save() {
        var motor = MotorInputDto()
        motor.resonanzU = .some(resonanzU)
        motor.kurbelgehaeuseV = .some(kurbelgehaeuseCm3.map { $0 * 1000 })
        motor.einlass = EinlassInputDto(flaecheA: .some(flaecheMm2), steuerzeitSZ: .some(steuerwinkel))
        Task { await model.saveToSelectedVehicle(VehicleInputDto(motor: motor)) }
    }
}

/// v2 `EinlassVergaserViewModel`: carburettor bore from displacement (cm³) and rpm, main jet.
struct VergaserView: View {
    @Environment(AppModel.self) private var model
    @State private var hubraumCm3: Double?
    @State private var resonanzU: Double?
    @State private var widerstand: Double? = 0.9
    @State private var vergaserMm: Double?
    @State private var duesenFaktor: Double? = 0.95

    private var berechnet: Double? {
        guard let hubraumCm3, let resonanzU else { return nil }
        return EinlassLogic.getVergaserDurchmesser(hubvolumen: hubraumCm3, resonanzdrehzahl: resonanzU,
                                                   widerstandsFaktor: widerstand ?? 0.9)
    }

    var body: some View {
        SheetForm(module: .einlass, subsheet: 2, subtitle: "Vergaser") {
            Section("Vergasergröße") {
                NumberField("Hubraum", value: $hubraumCm3, unit: "cm³")
                NumberField("Resonanzdrehzahl", value: $resonanzU, unit: "U/min")
                NumberField("Widerstandsfaktor", value: $widerstand)
                ResultRow(title: "Vergaserdurchmesser", value: model.format(berechnet, unit: "mm"))
            }
            Section("Hauptdüse") {
                NumberField("Vergaserdurchmesser", value: $vergaserMm, unit: "mm")
                NumberField("Faktor", value: $duesenFaktor)
                ResultRow(title: "Hauptdüse", value: model.format((vergaserMm ?? berechnet).map {
                    EinlassLogic.getVergaserHauptduesenDurchmesser(vergasergroesse: $0,
                                                                   widerstandsFaktor: duesenFaktor ?? 0.95)
                }))
            }
            VehicleActionsSection(apply: apply, save: save)
        }
        .moduleChrome("Vergaser")
        .onAppear { if hubraumCm3 == nil { apply() } }
    }

    private func apply() {
        guard let motor = model.selectedVehicle?.motor else { return }
        hubraumCm3 = motor.hubraumV.map { $0 / 1000 }
        resonanzU = motor.resonanzU
        vergaserMm = motor.einlass?.vergaser?.durchmesserD
    }

    private func save() {
        var motor = MotorInputDto()
        motor.einlass = EinlassInputDto(vergaser: VergaserInputDto(durchmesserD: .some(vergaserMm ?? berechnet)))
        Task { await model.saveToSelectedVehicle(VehicleInputDto(motor: motor)) }
    }
}
