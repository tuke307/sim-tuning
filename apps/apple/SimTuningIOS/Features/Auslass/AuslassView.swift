import SimTuningAPI
import SimTuningLogic
import SwiftUI

struct AuslassView: View {
    var body: some View {
        NavigationStack {
            SheetList(module: .auslass) {
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
                .accessibilityIdentifier("auslass.anwendung")
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
        SheetForm(module: .auslass, subsheet: 1, subtitle: "Theorie") {
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
    @State private var highlighted: Int?
    @State private var drawIn = 0.0

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

    private func items(_ r: AuslassLogic.AuspuffResult) -> [StuecklisteItem] {
        [
            .init(pos: 1, title: "Krümmerlänge", value: model.format(r.kruemmerL), unit: "mm", identifier: "auspuff.lk"),
            .init(pos: 2, title: "Diffusorlänge", value: model.format(r.diffusorL1), unit: "mm"),
            .init(pos: 3, title: "Mittelteillänge", value: model.format(r.mittelteilL), unit: "mm"),
            .init(pos: 4, title: "Gegenkonuslänge", value: model.format(r.gegenkonusL), unit: "mm"),
            .init(pos: 5, title: "Endrohrlänge", value: model.format(endrohrL), unit: "mm"),
            .init(pos: 6, title: "Krümmerdurchmesser", value: model.format(r.kruemmerD), unit: "mm"),
            .init(pos: 7, title: "Endrohrdurchmesser", value: model.format(endrohrD), unit: "mm"),
            .init(pos: 8, title: "Mittelteildurchmesser", value: model.format(r.mittelteilD), unit: "mm"),
            .init(pos: 9, title: "Diffusordurchmesser", value: model.format(r.diffusorD1), unit: "mm"),
            .init(pos: 10, title: "Resonanzlänge", value: model.format(r.resonanzL), unit: "mm",
                  identifier: "auspuff.resonanz"),
        ]
    }

    var body: some View {
        SheetForm(module: .auslass, subsheet: 2, subtitle: "Anwendung") {
            if isComplete {
                let result = AuslassLogic.calculateAuspuff(input)
                Section("Zeichnung") {
                    AuspuffDrawing(geometry: AuslassLogic.getAuspuffGeometry(result, input),
                                   highlighted: $highlighted, progress: drawIn)
                        .accessibilityElement()
                        .accessibilityLabel(Text("Auspuffzeichnung"))
                        .accessibilityValue(Text(verbatim: items(result).prefix(8)
                            .map { "\($0.pos): \($0.value) mm" }.joined(separator: ", ")))
                        .accessibilityHint(Text("Die Positionen sind in der Stückliste beschrieben."))
                        .accessibilityIdentifier("auspuff.zeichnung")
                        .drawingSheet()
                        .modifier(DrawInModifier(progress: $drawIn))
                        .drawingRow()
                }
            }
            Section("Motor & Auslass") {
                NumberField("Resonanzdrehzahl", value: $resonanzU, unit: "U/min")
                NumberField("Auslasssteuerzeit", value: $steuerzeit, unit: "°KW")
                NumberField("Auslasslänge", value: $laengeL, unit: "mm", identifier: "auspuff.laenge")
                NumberField("Auslassdurchmesser", value: $durchmesserD, unit: "mm", identifier: "auspuff.durchmesser")
                NumberField("Auslassfläche", value: $flaecheA, unit: "mm²", identifier: "auspuff.flaeche")
            }
            Section("Auspuff") {
                NumberField("Abgastemperatur", value: $abgasT, unit: "°C", identifier: "auspuff.abgasT")
                NumberField("Krümmerfaktor", value: $kruemmerF, identifier: "auspuff.kruemmerF")
                NumberField("Krümmerwinkel", value: $kruemmerW, unit: "°", identifier: "auspuff.kruemmerW")
                Picker("Diffusorstufen", selection: $diffusorStage) {
                    ForEach(0...3, id: \.self) { Text(verbatim: "\($0)").tag($0) }
                }
                NumberField("Diffusorwinkel", value: $diffusorW1, unit: "°", identifier: "auspuff.diffusorW")
                NumberField("Mittelteilfaktor", value: $mittelteilF, identifier: "auspuff.mittelteilF")
                NumberField("Gegenkonuswinkel", value: $gegenKonusW, unit: "°", identifier: "auspuff.gegenkonusW")
                NumberField("Endrohrlänge", value: $endrohrL, unit: "mm", identifier: "auspuff.endrohrL")
                NumberField("Endrohrdurchmesser", value: $endrohrD, unit: "mm", identifier: "auspuff.endrohrD")
            }
            if isComplete {
                Stueckliste(items: items(AuslassLogic.calculateAuspuff(input)), highlighted: $highlighted,
                            footer: diffusorStage > 1
                                ? "Wie in v2 ist nur der einstufige Diffusor berechnet; mehrstufige Diffusoren ergeben 0."
                                : nil)
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

/// Renders `AuslassLogic.getAuspuffGeometry` (px = mm) as a dimensioned drawing: chamber outline in
/// the Auslass colour, extension lines as hairlines, dimension lines with arrow heads and numbered
/// callouts in the signal colour, part names above the chamber. Scales to the row width.
struct AuspuffDrawing: View, Animatable {
    let geometry: AuslassLogic.AuspuffGeometry
    @Binding var highlighted: Int?
    nonisolated var progress: Double

    nonisolated var animatableData: Double {
        get { progress }
        set { progress = newValue }
    }

    @Environment(\.colorSchemeContrast) private var contrast
    @ScaledMetric(relativeTo: .caption2) private var calloutBase: CGFloat = 20
    @ScaledMetric(relativeTo: .caption2) private var labelBase: CGFloat = 11

    private var calloutDiameter: CGFloat { min(calloutBase, 30) }
    /// Part names scale with Dynamic Type, capped so they stay on the sheet.
    private var labelSize: CGFloat { min(labelBase, 15) }
    private var labelRow: CGFloat { labelSize + 5 }
    private var labelBand: CGFloat { labelRow * 2 + 6 }

    /// Chamber axis (all v2 primitives are mirrored around it).
    private var ym: Double {
        geometry.rects.first.map { $0.y + $0.height / 2 } ?? geometry.height * 0.75
    }

    private struct Anchor {
        let pos: Int
        let x: Double
        let y: Double
    }

    /// Callout anchors in drawing coordinates: on the dimension line they describe.
    private var anchors: [Anchor] {
        var result: [Anchor] = []
        for label in geometry.valueLabels {
            switch label.text {
            case "LK": result.append(Anchor(pos: 1, x: label.x + 15, y: ym + 175))
            case "LD", "LD1": result.append(Anchor(pos: 2, x: label.x + 15, y: ym + 175))
            case "LM": result.append(Anchor(pos: 3, x: label.x + 15, y: ym + 175))
            case "LG": result.append(Anchor(pos: 4, x: label.x + 15, y: ym + 175))
            case "LE": result.append(Anchor(pos: 5, x: label.x + 15, y: ym + 175))
            case "D1": result.append(Anchor(pos: 6, x: label.x + 55, y: ym))
            case "D2": result.append(Anchor(pos: 7, x: label.x - 30, y: ym))
            default: break
            }
        }
        if let mittelteil = middleDimensionX { result.append(Anchor(pos: 8, x: mittelteil, y: ym)) }
        return result
    }

    /// x of the added Mittelteil diameter dimension (inside the cylindrical middle section).
    private var middleDimensionX: Double? {
        guard let rect = geometry.rects.first, rect.width > 0, rect.height > 0 else { return nil }
        return rect.x + rect.width * 0.75
    }

    private var bounds: CGRect {
        var xs: [Double] = []
        var ys: [Double] = []
        for s in geometry.segments {
            xs += [s.x1, s.x2]
            ys += [s.y1, s.y2]
        }
        for r in geometry.rects where r.width > 0 {
            xs += [r.x, r.x + r.width]
            ys += [r.y, r.y + r.height]
        }
        for a in anchors {
            xs.append(a.x)
            ys.append(a.y)
        }
        guard let minX = xs.min(), let maxX = xs.max(), let minY = ys.min(), let maxY = ys.max() else {
            return CGRect(x: 0, y: 0, width: geometry.width, height: geometry.height)
        }
        return CGRect(x: minX, y: minY, width: max(maxX - minX, 1), height: max(maxY - minY, 1))
    }

    private struct Mapping {
        let scale: CGFloat
        let offset: CGPoint
        let bounds: CGRect

        func callAsFunction(_ x: Double, _ y: Double) -> CGPoint {
            CGPoint(x: offset.x + (x - bounds.minX) * scale, y: offset.y + (y - bounds.minY) * scale)
        }
    }

    private var padding: CGFloat { calloutDiameter / 2 + 4 }

    private func mapping(_ size: CGSize) -> Mapping {
        let b = bounds
        let availableWidth = max(size.width - 2 * padding, 1)
        let availableHeight = max(size.height - labelBand - 2 * padding, 1)
        let scale = min(availableWidth / b.width, availableHeight / b.height)
        let drawnWidth = b.width * scale
        return Mapping(scale: scale,
                       offset: CGPoint(x: (size.width - drawnWidth) / 2, y: labelBand + padding),
                       bounds: b)
    }

    var body: some View {
        let b = bounds
        AspectHeightLayout(aspect: b.width / b.height, extra: labelBand + 2 * padding) {
            GeometryReader { proxy in
                Canvas { context, size in
                    draw(context, mapping(size))
                }
                .contentShape(Rectangle())
                .onTapGesture { location in
                    let m = mapping(proxy.size)
                    let hit = anchors.first { a in
                        let p = m(a.x, a.y)
                        return hypot(p.x - location.x, p.y - location.y) <= max(calloutDiameter, 44) / 2
                    }
                    if let hit { highlighted = highlighted == hit.pos ? nil : hit.pos }
                }
            }
        }
    }

    private func draw(_ context: GraphicsContext, _ m: Mapping) {
        let hc = contrast == .increased
        let hairline: CGFloat = hc ? 1.5 : 1

        // Extension lines.
        var boundary = Path()
        for s in geometry.segments where s.kind == .boundary {
            boundary.move(to: m(s.x1, s.y1))
            boundary.addLine(to: m(s.x2, s.y2))
        }
        context.stroke(boundary, with: .color(Theme.mutedForeground), lineWidth: hairline * 0.75)

        // Chamber outline in the module colour.
        var frame = Path()
        for s in geometry.segments where s.kind == .frame {
            frame.move(to: m(s.x1, s.y1))
            frame.addLine(to: m(s.x2, s.y2))
        }
        for r in geometry.rects where r.width > 0 {
            let origin = m(r.x, r.y)
            frame.addRect(CGRect(x: origin.x, y: origin.y, width: r.width * m.scale, height: r.height * m.scale))
        }
        context.stroke(frame, with: .color(Theme.auslass), style: StrokeStyle(lineWidth: hc ? 2.5 : 2, lineJoin: .miter))

        // Dimension lines (single draw-in), arrow heads at both ends.
        var dimensions: [(CGPoint, CGPoint)] = geometry.segments
            .filter { $0.kind == .dimension && ($0.x1 != $0.x2 || $0.y1 != $0.y2) }
            .map { (m($0.x1, $0.y1), m($0.x2, $0.y2)) }
        if let x = middleDimensionX, let rect = geometry.rects.first {
            dimensions.append((m(x, rect.y), m(x, rect.y + rect.height)))
        }
        for (a, b) in dimensions {
            var path = Path()
            path.move(to: a)
            path.addLine(to: b)
            context.stroke(path.trimmedPath(from: 0, to: progress), with: .color(Theme.primary), lineWidth: hairline)
            if progress >= 1, hypot(b.x - a.x, b.y - a.y) > 8 {
                context.fill(Drafting.arrowHead(tip: a, from: b, size: 5), with: .color(Theme.primary))
                context.fill(Drafting.arrowHead(tip: b, from: a, size: 5), with: .color(Theme.primary))
            }
        }

        // Callouts on the dimension lines.
        for a in anchors {
            Drafting.callout(context, number: a.pos, at: m(a.x, a.y), diameter: calloutDiameter,
                             highlighted: highlighted == a.pos, lineWidth: hairline)
        }

        // Part names above the chamber in two rows: each name takes the lower row unless it would
        // run into the previous name there (short parts such as the header).
        var rowEnds: [CGFloat] = [-.infinity, -.infinity]
        for label in geometry.partLabels {
            let x = m(label.x - 20, ym).x
            let text = context.resolve(Text(label.title)
                .font(.system(size: labelSize, weight: .semibold).width(.condensed))
                .foregroundStyle(Theme.mutedForeground))
            let width = text.measure(in: CGSize(width: 400, height: 100)).width
            let row = rowEnds.firstIndex { x >= $0 + 4 } ?? (rowEnds[0] <= rowEnds[1] ? 0 : 1)
            rowEnds[row] = x + 3 + width
            let y = m.offset.y - padding - CGFloat(row) * labelRow
            var tick = Path()
            tick.move(to: CGPoint(x: x, y: y))
            tick.addLine(to: CGPoint(x: x, y: m.offset.y - 2))
            context.stroke(tick, with: .color(Theme.border), lineWidth: hairline * 0.75)
            context.draw(text, at: CGPoint(x: x + 3, y: y), anchor: .bottomLeading)
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
