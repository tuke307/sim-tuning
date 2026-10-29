import SimTuningAPI
import SimTuningLogic
import SwiftUI

struct MotorView: View {
    var body: some View {
        NavigationStack {
            SheetList(module: .motor) {
                NavigationLink {
                    HubraumCalculatorView()
                } label: {
                    Label("Hubraum", systemImage: "cylinder")
                }
                .accessibilityIdentifier("motor.hubraum")
                NavigationLink {
                    VerdichtungCalculatorView()
                } label: {
                    Label("Verdichtung", systemImage: "arrow.down.right.and.arrow.up.left")
                }
                .accessibilityIdentifier("motor.verdichtung")
                NavigationLink {
                    SteuerdiagrammView()
                } label: {
                    Label("Steuerdiagramm", systemImage: "circle.dashed")
                }
                .accessibilityIdentifier("motor.steuerdiagramm")
                NavigationLink {
                    UmrechnungView()
                } label: {
                    Label("Umrechnung", systemImage: "arrow.left.arrow.right")
                }
                .accessibilityIdentifier("motor.umrechnung")
            }
            .moduleChrome("Motor")
        }
    }
}

// MARK: - Hubraum

struct HubraumCalculatorView: View {
    @Environment(AppModel.self) private var model
    @State private var hubraumCm3: Double?
    @State private var hubMm: Double?
    @State private var einbauspiel: Double? = 3
    @State private var drehzahl: Double?

    /// v2 `MotorHubraumViewModel`: d = f(Vh in cm³, h in cm) → cm.
    private var bohrungMm: Double? {
        guard let hubraumCm3, let hubMm, hubMm > 0, hubraumCm3 > 0 else { return nil }
        return EngineLogic.getCylinderHoleDiameter(hubraum: hubraumCm3, hub: hubMm / 10) * 10
    }

    var body: some View {
        SheetForm(module: .motor, subsheet: 1, subtitle: "Hubraum") {
            Section("Eingaben") {
                NumberField("Hubraum", value: $hubraumCm3, unit: "cm³", identifier: "hubraum.hubraum")
                NumberField("Hub", value: $hubMm, unit: "mm", identifier: "hubraum.hub")
                NumberField("Einbauspiel", value: $einbauspiel, unit: "¹⁄₁₀₀ mm")
                NumberField("Drehzahl", value: $drehzahl, unit: "U/min")
            }
            Section("Ergebnis") {
                ResultRow(title: "Bohrung", value: model.format(bohrungMm, unit: "mm"), identifier: "hubraum.bohrung")
                ResultRow(title: "Kolbendurchmesser", value: model.format(bohrungMm.map {
                    EngineLogic.getKolbenDurchmesser(bohrungsdurchmesser: $0, einbauspiel: einbauspiel ?? 0)
                }, unit: "mm"))
                if let hubMm, let drehzahl {
                    ResultRow(title: "Mittlere Kolbengeschwindigkeit", value: model.format(
                        EngineLogic.getKolbenGeschwindigkeit(hub: hubMm / 1000, drehzahl: drehzahl), unit: "m/s"))
                }
            }
            if let bohrungMm {
                let grinding = EngineLogic.getGrindingDiameters(bohrungMm)
                Section("Schleifmaße") {
                    ResultRow(title: "1. Übermaß", value: model.format(grinding.diameter1, unit: "mm"))
                    ResultRow(title: "2. Übermaß", value: model.format(grinding.diameter2, unit: "mm"))
                    ResultRow(title: "3. Übermaß", value: model.format(grinding.diameter3, unit: "mm"))
                    ResultRow(title: "4. Übermaß", value: model.format(grinding.diameter4, unit: "mm"))
                }
            }
            VehicleActionsSection(apply: apply, save: save)
        }
        .moduleChrome("Hubraum")
        .onAppear { if hubraumCm3 == nil { apply() } }
    }

    private func apply() {
        guard let motor = model.selectedVehicle?.motor else { return }
        hubraumCm3 = motor.hubraumV.map { $0 / 1000 }
        hubMm = motor.hubL
        drehzahl = motor.resonanzU
    }

    private func save() {
        var motor = MotorInputDto()
        motor.hubraumV = .some(hubraumCm3.map { $0 * 1000 })
        motor.hubL = .some(hubMm)
        if let bohrungMm { motor.bohrungD = .some(bohrungMm) }
        if let hubMm, let drehzahl {
            motor.kolbenG = .some(EngineLogic.getKolbenGeschwindigkeit(hub: hubMm / 1000, drehzahl: drehzahl))
        }
        Task { await model.saveToSelectedVehicle(VehicleInputDto(motor: motor)) }
    }
}

// MARK: - Verdichtung

struct VerdichtungCalculatorView: View {
    @Environment(AppModel.self) private var model
    @State private var hubraumCm3: Double?
    @State private var brennraumCm3: Double?
    @State private var bohrungMm: Double?
    @State private var zielVerdichtung: Double?

    private var verdichtung: Double? {
        guard let hubraumCm3, let brennraumCm3, brennraumCm3 > 0 else { return nil }
        return EngineLogic.getCompression(hubraum: hubraumCm3 * 1000, brennraum: brennraumCm3 * 1000)
    }

    private var abdrehen: Double? {
        guard let hubraumCm3, let brennraumCm3, let bohrungMm, let zielVerdichtung, zielVerdichtung != 0 else {
            return nil
        }
        return EngineLogic.getToDecreasingLength(hubraum: hubraumCm3 * 1000, brennraum: brennraumCm3 * 1000,
                                                 durchmesser: bohrungMm, zielVerdichtung: zielVerdichtung)
    }

    var body: some View {
        SheetForm(module: .motor, subsheet: 2, subtitle: "Verdichtung") {
            Section("Eingaben") {
                NumberField("Hubraum", value: $hubraumCm3, unit: "cm³", identifier: "verdichtung.hubraum")
                NumberField("Brennraum", value: $brennraumCm3, unit: "cm³", identifier: "verdichtung.brennraum")
                NumberField("Bohrung", value: $bohrungMm, unit: "mm", identifier: "verdichtung.bohrung")
            }
            Section("Ergebnis") {
                ResultRow(title: "Verdichtung", value: verdichtung.map { "\(model.format($0)) : 1" } ?? "–",
                          identifier: "verdichtung.result")
            }
            Section {
                NumberField("Zielverdichtung", value: $zielVerdichtung, unit: ": 1", identifier: "verdichtung.ziel")
                ResultRow(title: "Zylinder abdrehen um", value: model.format(abdrehen, unit: "mm"),
                          identifier: "verdichtung.abdrehen")
            } header: {
                Text("Zielverdichtung")
            } footer: {
                Text("Negative Werte bedeuten, dass der Brennraum vergrößert werden muss.")
            }
            VehicleActionsSection(apply: apply, save: save)
        }
        .moduleChrome("Verdichtung")
    }

    private func apply() {
        guard let motor = model.selectedVehicle?.motor else { return }
        hubraumCm3 = motor.hubraumV.map { $0 / 1000 }
        brennraumCm3 = motor.brennraumV.map { $0 / 1000 }
        bohrungMm = motor.bohrungD
    }

    private func save() {
        var motor = MotorInputDto()
        motor.brennraumV = .some(brennraumCm3.map { $0 * 1000 })
        motor.verdichtungV = .some(verdichtung)
        Task { await model.saveToSelectedVehicle(VehicleInputDto(motor: motor)) }
    }
}

// MARK: - Umrechnung

struct UmrechnungView: View {
    @Environment(AppModel.self) private var model
    @State private var hubMm: Double?
    @State private var pleulMm: Double?
    @State private var deachsierungMm: Double? = 0
    @State private var vorher: Double?
    @State private var nachher: Double?
    @State private var edge: Edge = .unterkante
    @State private var kwGrad: Double?

    enum Edge: String, CaseIterable, Identifiable {
        case oberkante, unterkante
        var id: String { rawValue }
        var title: LocalizedStringKey {
            switch self {
            case .oberkante: "Kolbenoberkante (Einlass)"
            case .unterkante: "Kolbenunterkante (Auslass, Überströmer)"
            }
        }
    }

    private var hubradius: Double? {
        guard let hubMm, let pleulMm else { return nil }
        return EngineLogic.getHubRadius(hub: hubMm, pleullaenge: pleulMm, deachsierung: deachsierungMm ?? 0)
    }

    var body: some View {
        SheetForm(module: .motor, subsheet: 4, subtitle: "Umrechnung") {
            Section("Kurbeltrieb") {
                NumberField("Hub", value: $hubMm, unit: "mm")
                NumberField("Pleuellänge", value: $pleulMm, unit: "mm")
                NumberField("Deachsierung", value: $deachsierungMm, unit: "mm")
                ResultRow(title: "Hubradius", value: model.format(hubradius, unit: "mm"))
            }
            Section("Steuerzeit ändern") {
                NumberField("Steuerzeit vorher", value: $vorher, unit: "°KW")
                NumberField("Steuerzeit nachher", value: $nachher, unit: "°KW")
                Picker("Steuerkante", selection: $edge) {
                    ForEach(Edge.allCases) { Text($0.title).tag($0) }
                }
                if let vorher, let nachher {
                    let w = EngineLogic.getSteuerwinkel(vorherSteuerzeit: vorher, nachherSteuerzeit: nachher,
                                                        kolbenoberkante: edge == .oberkante,
                                                        kolbenunterkante: edge == .unterkante)
                    ResultRow(title: "Öffnet vorher / nachher",
                              value: "\(model.format(w.oeffnetVorher))° / \(model.format(w.oeffnetNachher))°")
                    ResultRow(title: "Schließt vorher / nachher",
                              value: "\(model.format(w.schliesstVorher))° / \(model.format(w.schliesstNachher))°")
                    ResultRow(title: "Differenz", value: model.format(
                        EngineLogic.getPortTimingDifference(inMm: false, vorher: vorher, nachher: nachher), unit: "°KW"))
                    if let hubradius, let pleulMm {
                        ResultRow(title: "Differenz am Kolbenweg", value: model.format(
                            EngineLogic.getPortTimingDifference(inMm: true, vorher: w.oeffnetVorher,
                                                                nachher: w.oeffnetNachher, pleullaenge: pleulMm,
                                                                hubradius: hubradius,
                                                                deachsierung: deachsierungMm ?? 0), unit: "mm"))
                    }
                }
            }
            Section("Kolbenweg") {
                NumberField("Kurbelwinkel nach OT", value: $kwGrad, unit: "°KW")
                if let kwGrad, let hubradius, let pleulMm {
                    ResultRow(title: "Abstand zum OT", value: model.format(
                        EngineLogic.getDistanceToOT(pleullaenge: pleulMm, hubradius: hubradius,
                                                    deachsierung: deachsierungMm ?? 0, kwgrad: kwGrad), unit: "mm"))
                }
            }
            VehicleActionsSection(apply: apply, save: nil)
        }
        .moduleChrome("Umrechnung")
        .onAppear { if hubMm == nil { apply() } }
    }

    private func apply() {
        guard let motor = model.selectedVehicle?.motor else { return }
        hubMm = motor.hubL
        pleulMm = motor.pleulL
        deachsierungMm = motor.deachsierungL ?? 0
    }
}
