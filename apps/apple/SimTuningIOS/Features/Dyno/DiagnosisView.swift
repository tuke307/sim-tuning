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
    @State private var highlighted: Int?

    private var curve: [DynoPsPointDto] {
        guard let cw, let uebersetzung, uebersetzung != 0, let frontA, let gewicht else { return [] }
        return DynoLogic.diagnosisCurve(
            drehzahl: dyno.drehzahl.map { (zeit: $0.zeit, drehzahl: $0.drehzahl) },
            cw: cw, gesamtuebersetzung: uebersetzung, gewicht: gewicht, frontA: frontA
        ).map { DynoPsPointDto(drehzahl: $0.drehzahl, ps: DynoLogic.wattToPs($0.leistung)) }
    }

    var body: some View {
        SheetForm(module: .dyno, subsheet: 3, subtitle: "Diagnose") {
            Section("Fahrzeug") {
                NumberField("Gewicht inkl. Fahrer", value: $gewicht, unit: "kg")
                NumberField("Luftwiderstandsbeiwert (cw)", value: $cw)
                NumberField("Stirnfläche", value: $frontA, unit: "m²")
                NumberField("Gesamtübersetzung", value: $uebersetzung)
            }
            if dyno.drehzahl.isEmpty {
                Section {
                    Text("Noch keine Drehzahlkurve. Nimm zuerst einen Audio-Dyno auf.").foregroundStyle(Theme.mutedForeground)
                }
            } else if !curve.isEmpty {
                let points = curve
                let peak = points.max(by: { $0.ps < $1.ps })
                Section("Zeichnung") {
                    Chart {
                        ForEach(Array(DynoView.downsample(points, max: 400).enumerated()), id: \.offset) { _, point in
                            LineMark(x: .value("U/min", point.drehzahl), y: .value("PS", point.ps))
                                .foregroundStyle(Theme.dyno)
                                .lineStyle(StrokeStyle(lineWidth: 2))
                        }
                        if let peak {
                            // Dimension line from the axis to the peak, callout ① at the peak.
                            RuleMark(x: .value("U/min", peak.drehzahl), yStart: .value("PS", 0), yEnd: .value("PS", peak.ps))
                                .foregroundStyle(Theme.primary)
                                .lineStyle(StrokeStyle(lineWidth: 1, dash: [4, 3]))
                            PointMark(x: .value("U/min", peak.drehzahl), y: .value("PS", peak.ps))
                                .foregroundStyle(Theme.primary)
                                .symbolSize(30)
                                .annotation(position: .top, alignment: .center, spacing: 4) {
                                    Callout(number: 1, highlighted: highlighted == 1)
                                        .background(Circle().fill(Theme.card))
                                }
                        }
                    }
                    .chartXAxisLabel(position: .bottom, alignment: .trailing) { Text(verbatim: "U/min").stUnitStyle() }
                    .chartYAxisLabel(position: .leading, alignment: .top) { Text(verbatim: "PS").stUnitStyle() }
                    .stChartStyle()
                    .frame(height: 260)
                    .accessibilityLabel(Text("Leistungskurve"))
                    .padding(Theme.Spacing.s)
                    .background(Theme.card)
                    .drawingRow()
                }
                if let peak {
                    Stueckliste(items: [
                        .init(pos: 1, title: "Maximale Leistung", value: model.format(peak.ps), unit: "PS"),
                        .init(pos: 2, title: "bei Drehzahl", value: model.format(peak.drehzahl), unit: "U/min"),
                    ], highlighted: $highlighted)
                }
                Section {
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
