import Charts
import SimTuningAPI
import SimTuningLogic
import SwiftUI

/// v2 `DynoDiagnosisViewModel`: power curve from the stored RPM series with the v2 constants.
struct DiagnosisView: View {
    @Environment(AppModel.self) private var model
    let dyno: DynoDto
    let onSaved: () async -> Void

    @State private var cw: Double? = 0.6
    @State private var uebersetzung: Double?
    @State private var frontA: Double? = 0.6
    @State private var gewicht: Double?

    private var curve: [DynoPsPointDto] {
        guard let cw, let uebersetzung, uebersetzung != 0, let frontA, let gewicht else { return [] }
        return DynoLogic.diagnosisCurve(
            drehzahl: dyno.drehzahl.map { (zeit: $0.zeit, drehzahl: $0.drehzahl) },
            cw: cw, gesamtuebersetzung: uebersetzung, gewicht: gewicht, frontA: frontA
        ).map { DynoPsPointDto(drehzahl: $0.drehzahl, ps: DynoLogic.wattToPs($0.leistung)) }
    }

    var body: some View {
        Form {
            Section("Fahrzeug") {
                NumberField("Gewicht inkl. Fahrer", value: $gewicht, unit: "kg")
                NumberField("Luftwiderstandsbeiwert (cw)", value: $cw)
                NumberField("Stirnfläche", value: $frontA, unit: "m²")
                NumberField("Gesamtübersetzung", value: $uebersetzung)
            }
            if dyno.drehzahl.isEmpty {
                Section {
                    Text("Noch keine Drehzahlkurve. Nimm zuerst einen Audio-Dyno auf.").foregroundStyle(.secondary)
                }
            } else if !curve.isEmpty {
                let points = curve
                Section {
                    Chart {
                        ForEach(Array(DynoView.downsample(points, max: 400).enumerated()), id: \.offset) { _, point in
                            LineMark(x: .value("U/min", point.drehzahl), y: .value("PS", point.ps))
                        }
                    }
                    .frame(height: 260)
                    .accessibilityLabel(Text("Leistungskurve"))
                    if let peak = points.max(by: { $0.ps < $1.ps }) {
                        ResultRow(title: "Maximale Leistung", value: "\(model.format(peak.ps, unit: "PS")) @ \(model.format(peak.drehzahl, unit: "U/min"))")
                    }
                    Button("Leistungskurve speichern", systemImage: "square.and.arrow.down") {
                        Task {
                            let data = DynoDataInputDto(dynoPs: Array(points.prefix(maxDynoPoints)))
                            await model.perform { try await $0.saveDynoData(dynoId: dyno.id, data) }
                            await onSaved()
                        }
                    }
                } header: {
                    Text("Leistung")
                } footer: {
                    Text("Formel und Konstanten wie in SimTuning v2 (Luftdichte 1,2, Radradius 0,277 m, Rollwiderstand 0,005).")
                }
            }
        }
        .navigationTitle("Diagnose")
        .onAppear {
            if gewicht == nil { gewicht = model.selectedVehicle?.gewicht }
        }
    }
}
