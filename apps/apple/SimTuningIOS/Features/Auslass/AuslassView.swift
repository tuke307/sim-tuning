import SimTuningAPI
import SimTuningLogic
import SwiftUI

struct AuslassView: View {
    var body: some View {
        NavigationStack {
            List {
                NavigationLink {
                    AuslassTheorieView()
                } label: {
                    Label("Theorie", systemImage: "function")
                }
                NavigationLink {
                    AuslassAnwendungView()
                } label: {
                    Label("Anwendung", systemImage: "ruler")
                }
            }
            .moduleChrome("Auslass")
        }
    }
}

/// v2 `AuslassTheorieViewModel`: gas velocity, header diameter range, header and tuned length.
struct AuslassTheorieView: View {
    @Environment(AppModel.self) private var model
    @State private var abgasT: Double?
    @State private var flaecheMm2: Double?
    @State private var kruemmerD: Double?
    @State private var kruemmerF: Double?
    @State private var steuerzeit: Double?
    @State private var resonanzU: Double?

    var body: some View {
        Form {
            Section("Abgas") {
                NumberField("Abgastemperatur", value: $abgasT, unit: "°C")
                ResultRow(title: "Gasgeschwindigkeit", value: model.format(abgasT.map {
                    AuslassLogic.getGasGeschwindigkeit(abgastemperatur: $0)
                }, unit: "m/s"))
            }
            Section("Krümmer") {
                NumberField("Auslassfläche", value: $flaecheMm2, unit: "mm²")
                if let flaecheMm2 {
                    let a = AuslassLogic.getKruemmerDurchmesser(auslassflaeche: flaecheMm2 / 100, percentage: 10)
                    let b = AuslassLogic.getKruemmerDurchmesser(auslassflaeche: flaecheMm2 / 100, percentage: 20)
                    ResultRow(title: "Krümmerdurchmesser", value: "\(model.format(a)) – \(model.format(b)) cm")
                }
                NumberField("Krümmerdurchmesser", value: $kruemmerD, unit: "mm")
                NumberField("Drehmomentfaktor", value: $kruemmerF)
                if let kruemmerD, let kruemmerF {
                    ResultRow(title: "Krümmerlänge", value: model.format(AuslassLogic.getKruemmerLaenge(
                        kruemmerdurchmesser: kruemmerD, drehmomentfaktor: kruemmerF, auslassLaenge: 0), unit: "mm"))
                }
            }
            Section("Resonanz") {
                NumberField("Auslasssteuerzeit", value: $steuerzeit, unit: "°KW")
                NumberField("Resonanzdrehzahl", value: $resonanzU, unit: "U/min")
                if let steuerzeit, let abgasT, let resonanzU, resonanzU != 0 {
                    ResultRow(title: "Resonanzlänge", value: model.format(AuslassLogic.getResonanzLaenge(
                        auslassSteuerwinkel: steuerzeit, abgasTemperatur: abgasT, resonanzDrehzahl: resonanzU),
                                                                             unit: "mm"))
                }
            }
            VehicleActionsSection(apply: apply, save: nil)
        }
        .moduleChrome("Theorie")
        .onAppear { if steuerzeit == nil { apply() } }
    }

    private func apply() {
        guard let motor = model.selectedVehicle?.motor else { return }
        abgasT = motor.auslass?.auspuff?.abgasT
        flaecheMm2 = motor.auslass?.flaecheA
        kruemmerD = motor.auslass?.auspuff?.kruemmerD ?? motor.auslass?.durchmesserD
        kruemmerF = motor.auslass?.auspuff?.kruemmerF
        steuerzeit = motor.auslass?.steuerzeitSZ
        resonanzU = motor.resonanzU
    }
}

/// v2 `AuslassAnwendungViewModel`: complete expansion chamber + drawing.
struct AuslassAnwendungView: View {
    @Environment(AppModel.self) private var model
    @State private var resonanzU: Double?
    @State private var laengeL: Double?
    @State private var steuerzeit: Double?
    @State private var durchmesserD: Double?
    @State private var flaecheA: Double?
    @State private var abgasT: Double?
    @State private var diffusorStage = 1
    @State private var diffusorW1: Double?
    @State private var kruemmerF: Double?
    @State private var kruemmerW: Double?
    @State private var mittelteilF: Double?
    @State private var gegenKonusW: Double?
    @State private var endrohrL: Double?
    @State private var endrohrD: Double?

    private var input: AuslassLogic.AuspuffInput {
        .init(
            resonanzU: resonanzU,
            auslass: .init(laengeL: laengeL, steuerzeitSZ: steuerzeit, durchmesserD: durchmesserD, flaecheA: flaecheA),
            auspuff: .init(abgasT: abgasT, diffusorStage: diffusorStage, diffusorW1: diffusorW1, kruemmerF: kruemmerF,
                           kruemmerW: kruemmerW, mittelteilF: mittelteilF, gegenKonusW: gegenKonusW,
                           endrohrL: endrohrL, endrohrD: endrohrD)
        )
    }

    private var isComplete: Bool {
        [resonanzU, steuerzeit, abgasT, durchmesserD, flaecheA, kruemmerF, mittelteilF, gegenKonusW, endrohrD]
            .allSatisfy { $0 != nil } && (resonanzU ?? 0) != 0
    }

    var body: some View {
        Form {
            Section("Motor & Auslass") {
                NumberField("Resonanzdrehzahl", value: $resonanzU, unit: "U/min")
                NumberField("Auslasssteuerzeit", value: $steuerzeit, unit: "°KW")
                NumberField("Auslasslänge", value: $laengeL, unit: "mm")
                NumberField("Auslassdurchmesser", value: $durchmesserD, unit: "mm")
                NumberField("Auslassfläche", value: $flaecheA, unit: "mm²")
            }
            Section("Auspuff") {
                NumberField("Abgastemperatur", value: $abgasT, unit: "°C")
                NumberField("Krümmerfaktor", value: $kruemmerF)
                NumberField("Krümmerwinkel", value: $kruemmerW, unit: "°")
                Picker("Diffusorstufen", selection: $diffusorStage) {
                    ForEach(0...3, id: \.self) { Text(verbatim: "\($0)").tag($0) }
                }
                NumberField("Diffusorwinkel", value: $diffusorW1, unit: "°")
                NumberField("Mittelteilfaktor", value: $mittelteilF)
                NumberField("Gegenkonuswinkel", value: $gegenKonusW, unit: "°")
                NumberField("Endrohrlänge", value: $endrohrL, unit: "mm")
                NumberField("Endrohrdurchmesser", value: $endrohrD, unit: "mm")
            }
            if isComplete {
                let result = AuslassLogic.calculateAuspuff(input)
                Section {
                    ResultRow(title: "Resonanzlänge", value: model.format(result.resonanzL, unit: "mm"))
                    ResultRow(title: "Krümmer D / L", value: "\(model.format(result.kruemmerD)) / \(model.format(result.kruemmerL)) mm")
                    ResultRow(title: "Diffusor D / L", value: "\(model.format(result.diffusorD1)) / \(model.format(result.diffusorL1)) mm")
                    ResultRow(title: "Mittelteil D / L", value: "\(model.format(result.mittelteilD)) / \(model.format(result.mittelteilL)) mm")
                    ResultRow(title: "Gegenkonus D / L", value: "\(model.format(result.gegenkonusD)) / \(model.format(result.gegenkonusL)) mm")
                } header: {
                    Text("Ergebnis")
                } footer: {
                    if diffusorStage > 1 {
                        Text("Wie in v2 ist nur der einstufige Diffusor berechnet; mehrstufige Diffusoren ergeben 0.")
                    }
                }
                Section("Zeichnung") {
                    ScrollView(.horizontal) {
                        AuspuffDrawing(geometry: AuslassLogic.getAuspuffGeometry(result, input))
                            .frame(width: 700, height: 350)
                    }
                    .accessibilityElement()
                    .accessibilityLabel(Text("Auspuffzeichnung"))
                }
            }
            VehicleActionsSection(apply: apply, save: save)
        }
        .moduleChrome("Anwendung")
        .onAppear { if resonanzU == nil { apply() } }
    }

    private func apply() {
        guard let motor = model.selectedVehicle?.motor else { return }
        resonanzU = motor.resonanzU
        laengeL = motor.auslass?.laengeL
        steuerzeit = motor.auslass?.steuerzeitSZ
        durchmesserD = motor.auslass?.durchmesserD
        flaecheA = motor.auslass?.flaecheA
        let p = motor.auslass?.auspuff
        abgasT = p?.abgasT
        diffusorStage = p?.diffusorStage ?? 1
        diffusorW1 = p?.diffusorW1
        kruemmerF = p?.kruemmerF
        kruemmerW = p?.kruemmerW
        mittelteilF = p?.mittelteilF
        gegenKonusW = p?.gegenKonusW
        endrohrL = p?.endrohrL
        endrohrD = p?.endrohrD
    }

    private func save() {
        var auspuff = AuspuffInputDto()
        auspuff.abgasT = .some(abgasT)
        auspuff.diffusorStage = diffusorStage
        auspuff.diffusorW1 = .some(diffusorW1)
        auspuff.kruemmerF = .some(kruemmerF)
        auspuff.kruemmerW = .some(kruemmerW)
        auspuff.mittelteilF = .some(mittelteilF)
        auspuff.gegenKonusW = .some(gegenKonusW)
        auspuff.endrohrL = .some(endrohrL)
        auspuff.endrohrD = .some(endrohrD)
        if isComplete {
            let r = AuslassLogic.calculateAuspuff(input)
            auspuff.resonanzL = .some(r.resonanzL)
            auspuff.kruemmerD = .some(r.kruemmerD)
            auspuff.kruemmerL = .some(r.kruemmerL)
            auspuff.mittelteilD = .some(r.mittelteilD)
            auspuff.mittelteilL = .some(r.mittelteilL)
            auspuff.diffusorD1 = .some(r.diffusorD1)
            auspuff.diffusorL1 = .some(r.diffusorL1)
            auspuff.gegenkonusD = .some(r.gegenkonusD)
            auspuff.gegenkonusL = .some(r.gegenkonusL)
        }
        var motor = MotorInputDto()
        motor.resonanzU = .some(resonanzU)
        motor.auslass = AuslassInputDto(durchmesserD: .some(durchmesserD), flaecheA: .some(flaecheA),
                                        laengeL: .some(laengeL), steuerzeitSZ: .some(steuerzeit), auspuff: auspuff)
        Task { await model.saveToSelectedVehicle(VehicleInputDto(motor: motor)) }
    }
}

/// Renders `AuslassLogic.getAuspuffGeometry` (2000×1000 design space, px = mm) with SwiftUI paths.
struct AuspuffDrawing: View {
    let geometry: AuslassLogic.AuspuffGeometry

    var body: some View {
        Canvas { context, size in
            let scale = min(size.width / geometry.width, size.height / geometry.height)
            func p(_ x: Double, _ y: Double) -> CGPoint { CGPoint(x: x * scale, y: y * scale) }
            for kind in [AuslassLogic.SegmentKind.frame, .boundary, .dimension] {
                var path = Path()
                for s in geometry.segments where s.kind == kind {
                    path.move(to: p(s.x1, s.y1))
                    path.addLine(to: p(s.x2, s.y2))
                }
                let color: Color = kind == .frame ? .red : .primary
                context.stroke(path, with: .color(color), lineWidth: kind == .frame ? 2 : 1)
            }
            for r in geometry.rects {
                let rect = CGRect(x: r.x * scale, y: r.y * scale, width: r.width * scale, height: r.height * scale)
                context.stroke(Path(rect.standardized), with: .color(.red), lineWidth: 2)
            }
            for label in geometry.valueLabels {
                context.draw(Text(verbatim: label.text).font(.caption2), at: p(label.x, label.y), anchor: .topLeading)
            }
            for label in geometry.partLabels {
                context.draw(Text(label.title).font(.caption2).foregroundStyle(.secondary),
                             at: p(label.x, label.y), anchor: .leading)
            }
        }
    }
}

extension AuslassLogic.PartLabel {
    var title: LocalizedStringKey {
        switch key {
        case .kruemmer: "Krümmer"
        case .konus: "Konus"
        case .mittelteil: "Mittelteil"
        case .gegenkonus: "Gegenkonus"
        case .endrohr: "Endrohr"
        }
    }
}
