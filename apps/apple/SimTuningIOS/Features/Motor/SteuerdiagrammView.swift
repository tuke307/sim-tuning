import SimTuningAPI
import SimTuningLogic
import SwiftUI

// MARK: - Steuerdiagramm

/// Port timing diagram as a drawing sheet: port lines in module colours with dash patterns,
/// opening angles as signal-coloured dimension arcs with numbered callouts, linked to the
/// Stückliste below.
struct SteuerdiagrammView: View {
    @Environment(AppModel.self) private var model
    @Draft(.steuerdiagramm, "einlass") private var einlass
    @Draft(.steuerdiagramm, "auslass") private var auslass
    @Draft(.steuerdiagramm, "ueberstroemer") private var ueberstroemer
    @State private var highlighted: Int?
    @State private var drawIn = 0.0

    private var vorauslass: Double? {
        guard let auslass, let ueberstroemer else { return nil }
        return EngineLogic.getVorauslass(auslassSW: auslass, ueberstroemerSW: ueberstroemer)
    }

    /// Dimension arcs shown in the drawing; positions match the Stückliste.
    private var spans: [TimingSpan] {
        var result: [TimingSpan] = []
        if (einlass ?? 0) > 0 {
            result.append(TimingSpan(pos: 1, from: (.einlass, .oeffnet), to: (.einlass, .schliesst),
                                     target: .top, radius: 0.86))
        }
        if (auslass ?? 0) > 0 {
            result.append(TimingSpan(pos: 2, from: (.auslass, .oeffnet), to: (.auslass, .schliesst),
                                     target: .bottom, radius: 0.72))
        }
        if (ueberstroemer ?? 0) > 0 {
            result.append(TimingSpan(pos: 3, from: (.ueberstroemer, .oeffnet), to: (.ueberstroemer, .schliesst),
                                     target: .bottom, radius: 0.58))
        }
        if let vorauslass, vorauslass > 0 {
            result.append(TimingSpan(pos: 4, from: (.auslass, .oeffnet), to: (.ueberstroemer, .oeffnet),
                                     target: .shortest, radius: 0.44))
        }
        return result
    }

    private var items: [StuecklisteItem] {
        var rows: [StuecklisteItem] = []
        if let einlass { rows.append(.init(pos: 1, title: "Einlasssteuerzeit", value: model.format(einlass), unit: "°KW")) }
        if let auslass { rows.append(.init(pos: 2, title: "Auslasssteuerzeit", value: model.format(auslass), unit: "°KW")) }
        if let ueberstroemer {
            rows.append(.init(pos: 3, title: "Überströmersteuerzeit", value: model.format(ueberstroemer), unit: "°KW"))
        }
        if let vorauslass { rows.append(.init(pos: 4, title: "Vorauslass", value: model.format(vorauslass), unit: "°KW")) }
        return rows
    }

    var body: some View {
        SheetForm(module: .motor, subsheet: 3, subtitle: "Steuerdiagramm") {
            Section("Zeichnung") {
                VStack(alignment: .leading, spacing: Theme.Spacing.s) {
                    SteuerdiagrammDrawing(
                        geometry: EngineLogic.getSteuerdiagramm(
                            einlass: einlass ?? 0, auslass: auslass ?? 0, ueberstroemer: ueberstroemer ?? 0),
                        spans: spans, highlighted: $highlighted, progress: drawIn
                    )
                    .aspectRatio(1, contentMode: .fit)
                    .frame(maxWidth: .infinity)
                    .accessibilityElement()
                    .accessibilityLabel(Text("Steuerdiagramm"))
                    .accessibilityValue(Text(accessibilitySummary))
                    .accessibilityIdentifier("steuer.zeichnung")
                    ViewThatFits(in: .horizontal) {
                        HStack(spacing: Theme.Spacing.l) { legend }
                        VStack(alignment: .leading, spacing: Theme.Spacing.xs) { legend }
                    }
                    .font(.footnote)
                }
                .drawingSheet()
                .modifier(DrawInModifier(progress: $drawIn))
                .drawingRow()
            }
            Section("Steuerzeiten") {
                NumberField("Einlass", value: $einlass, unit: "°KW", identifier: "steuer.einlass")
                NumberField("Auslass", value: $auslass, unit: "°KW", identifier: "steuer.auslass")
                NumberField("Überströmer", value: $ueberstroemer, unit: "°KW", identifier: "steuer.ueberstroemer")
            }
            if !items.isEmpty {
                Stueckliste(items: items, highlighted: $highlighted)
            }
            Section("Steuerwinkel") {
                angleRows("Einlass", einlass.map { EngineLogic.getSteuerwinkelOeffnet(einlass: $0) },
                          einlass.map { EngineLogic.getSteuerwinkelSchliesst(einlass: $0) })
                angleRows("Auslass", auslass.map { EngineLogic.getSteuerwinkelOeffnet(auslass: $0) },
                          auslass.map { EngineLogic.getSteuerwinkelSchliesst(auslass: $0) })
                angleRows("Überströmer", ueberstroemer.map { EngineLogic.getSteuerwinkelOeffnet(ueberstroemer: $0) },
                          ueberstroemer.map { EngineLogic.getSteuerwinkelSchliesst(ueberstroemer: $0) })
            }
            VehicleActionsSection(calculator: .steuerdiagramm, apply: apply, save: save)
        }
        .calculatorChrome("Steuerdiagramm")
        .calculatorDraft(.steuerdiagramm, seed: apply)
    }

    @ViewBuilder
    private var legend: some View {
        PortLegendItem(style: .einlass, title: "Einlass")
        PortLegendItem(style: .auslass, title: "Auslass")
        PortLegendItem(style: .ueberstroemer, title: "Überströmer")
    }

    private var accessibilitySummary: String {
        [("Einlass", einlass), ("Auslass", auslass), ("Überströmer", ueberstroemer)]
            .compactMap { name, value in value.map { "\(name) \(model.format($0)) °KW" } }
            .joined(separator: ", ")
    }

    @ViewBuilder
    private func angleRows(_ title: LocalizedStringKey, _ open: Double?, _ close: Double?) -> some View {
        if let open, let close {
            LabeledContent {
                Text(verbatim: "\(model.format(open))° → \(model.format(close))°")
                    .font(.stData)
                    .foregroundStyle(Theme.foreground)
            } label: {
                Text(title)
            }
        }
    }

    private func apply() {
        guard let motor = model.selectedVehicle?.motor else { return }
        einlass = motor.einlass?.steuerzeitSZ
        auslass = motor.auslass?.steuerzeitSZ
        ueberstroemer = motor.ueberstroemer?.steuerzeitSZ
    }

    private func save() {
        var motor = MotorInputDto()
        motor.einlass = EinlassInputDto(steuerzeitSZ: .some(einlass))
        motor.auslass = AuslassInputDto(steuerzeitSZ: .some(auslass))
        motor.ueberstroemer = UeberstroemerInputDto(steuerzeitSZ: .some(ueberstroemer))
        Task { await model.saveToSelectedVehicle(VehicleInputDto(motor: motor), from: .steuerdiagramm) }
    }
}

/// A dimension arc between two port edges.
struct TimingSpan {
    enum Target { case top, bottom, shortest }

    let pos: Int
    let from: (EngineLogic.SteuerdiagrammPort, EngineLogic.SteuerdiagrammEdge)
    let to: (EngineLogic.SteuerdiagrammPort, EngineLogic.SteuerdiagrammEdge)
    /// Which of the two possible arcs to draw: the one around OT, around UT, or the shorter one.
    let target: Target
    /// Arc radius as a fraction of the diagram radius.
    let radius: Double
}

extension EngineLogic.SteuerdiagrammPort {
    var style: PortStyle {
        switch self {
        case .einlass: .einlass
        case .auslass: .auslass
        case .ueberstroemer: .ueberstroemer
        }
    }

    /// Short label at the line end (EÖ = Einlass öffnet, AS = Auslass schließt, …).
    func label(_ edge: EngineLogic.SteuerdiagrammEdge) -> String {
        let prefix = switch self {
        case .einlass: "E"
        case .auslass: "A"
        case .ueberstroemer: "Ü"
        }
        return prefix + (edge == .oeffnet ? "Ö" : "S")
    }
}

/// Draws the v2 port timing diagram from `EngineLogic.getSteuerdiagramm` (600×600 design space)
/// on the drawing grid. `progress` animates the dimension arcs (single draw-in).
struct SteuerdiagrammDrawing: View, Animatable {
    let geometry: EngineLogic.SteuerdiagrammGeometry
    let spans: [TimingSpan]
    @Binding var highlighted: Int?
    nonisolated var progress: Double

    nonisolated var animatableData: Double {
        get { progress }
        set { progress = newValue }
    }

    @Environment(\.colorSchemeContrast) private var contrast
    @ScaledMetric(relativeTo: .caption2) private var calloutBase: CGFloat = 20
    @ScaledMetric(relativeTo: .caption2) private var labelBase: CGFloat = 11

    /// Drawing labels scale with Dynamic Type but are capped so they stay on the sheet; the
    /// Stückliste below carries the full-size values.
    private var labelSize: CGFloat { min(labelBase, 16) }

    private struct ArcLayout {
        let pos: Int
        let points: [CGPoint]
        let mid: CGPoint
    }

    private struct DrawingLayout {
        let scale: CGFloat
        let origin: CGPoint
        let center: CGPoint
        let radius: CGFloat
        let arcs: [ArcLayout]

        func point(_ x: Double, _ y: Double) -> CGPoint {
            CGPoint(x: origin.x + x * scale, y: origin.y + y * scale)
        }
    }

    private var calloutDiameter: CGFloat { min(calloutBase, 30) }

    private func layout(_ size: CGSize) -> DrawingLayout {
        let side = min(size.width, size.height)
        let scale = side / geometry.size
        let origin = CGPoint(x: (size.width - side) / 2, y: (size.height - side) / 2)
        let center = CGPoint(x: origin.x + geometry.center * scale, y: origin.y + geometry.center * scale)
        let radius = geometry.radius * scale

        func angle(_ port: EngineLogic.SteuerdiagrammPort, _ edge: EngineLogic.SteuerdiagrammEdge) -> Double? {
            guard let line = geometry.lines.first(where: { $0.port == port && $0.edge == edge }) else { return nil }
            return atan2(line.y2 - line.y1, line.x2 - line.x1)
        }
        func distance(_ a: Double, _ b: Double) -> Double { abs(atan2(sin(a - b), cos(a - b))) }

        let arcs: [ArcLayout] = spans.compactMap { span in
            guard let a1 = angle(span.from.0, span.from.1), let a2 = angle(span.to.0, span.to.1) else { return nil }
            var ccw = (a2 - a1).truncatingRemainder(dividingBy: 2 * .pi)
            if ccw < 0 { ccw += 2 * .pi }
            let cw = ccw - 2 * .pi
            let delta: Double = switch span.target {
            case .shortest:
                abs(ccw) <= abs(cw) ? ccw : cw
            case .top, .bottom:
                distance(a1 + ccw / 2, span.target == .top ? -.pi / 2 : .pi / 2)
                    <= distance(a1 + cw / 2, span.target == .top ? -.pi / 2 : .pi / 2) ? ccw : cw
            }
            let r = radius * span.radius
            let steps = max(8, Int(abs(delta) / (.pi / 90)))
            let points = (0...steps).map { i -> CGPoint in
                let a = a1 + delta * Double(i) / Double(steps)
                return CGPoint(x: center.x + r * cos(a), y: center.y + r * sin(a))
            }
            let mid = CGPoint(x: center.x + r * cos(a1 + delta / 2), y: center.y + r * sin(a1 + delta / 2))
            return ArcLayout(pos: span.pos, points: points, mid: mid)
        }
        return DrawingLayout(scale: scale, origin: origin, center: center, radius: radius, arcs: arcs)
    }

    var body: some View {
        GeometryReader { proxy in
            Canvas { context, size in
                draw(context, layout(size))
            }
            .contentShape(Rectangle())
            .onTapGesture { location in
                let hit = layout(proxy.size).arcs.first { arc in
                    hypot(arc.mid.x - location.x, arc.mid.y - location.y) <= max(calloutDiameter, 44) / 2
                }
                if let hit { highlighted = highlighted == hit.pos ? nil : hit.pos }
            }
        }
    }

    private func draw(_ context: GraphicsContext, _ l: DrawingLayout) {
        let hc = contrast == .increased
        let c = l.center
        let r = l.radius

        // Crank circle and centre lines.
        let circle = Path(ellipseIn: CGRect(x: c.x - r, y: c.y - r, width: 2 * r, height: 2 * r))
        context.stroke(circle, with: .color(Theme.foreground), lineWidth: hc ? 2 : 1.5)
        var axes = Path()
        axes.move(to: CGPoint(x: c.x, y: c.y - r - 6))
        axes.addLine(to: CGPoint(x: c.x, y: c.y + r + 6))
        axes.move(to: CGPoint(x: c.x - r - 6, y: c.y))
        axes.addLine(to: CGPoint(x: c.x + r + 6, y: c.y))
        context.stroke(axes, with: .color(Theme.mutedForeground), style: StrokeStyle(lineWidth: hc ? 1.5 : 1, dash: [3, 3]))

        // Port lines: colour + dash pattern + end label.
        for line in geometry.lines where line.angle != 0 {
            let start = l.point(line.x1, line.y1)
            let end = l.point(line.x2, line.y2)
            var path = Path()
            path.move(to: start)
            path.addLine(to: end)
            let style = line.port.style
            context.stroke(path, with: .color(style.color),
                           style: StrokeStyle(lineWidth: hc ? 3 : 2.5, lineCap: .butt, dash: style.dash))
            let length = max(hypot(end.x - start.x, end.y - start.y), 1)
            let labelAt = CGPoint(x: end.x + (end.x - start.x) / length * 13, y: end.y + (end.y - start.y) / length * 13)
            context.draw(Text(verbatim: line.port.label(line.edge))
                            .font(.system(size: labelSize, weight: .semibold, design: .monospaced))
                            .foregroundStyle(style.color),
                         at: labelAt, anchor: .center)
        }

        // OT / UT.
        let heading = Font.system(size: labelSize + 1, weight: .bold).width(.condensed)
        context.draw(Text(verbatim: "OT").font(heading).foregroundStyle(Theme.foreground),
                     at: CGPoint(x: c.x + 6, y: c.y - r - 6), anchor: .bottomLeading)
        context.draw(Text(verbatim: "UT").font(heading).foregroundStyle(Theme.foreground),
                     at: CGPoint(x: c.x + 6, y: c.y + r + 6), anchor: .topLeading)

        // Dimension arcs (signal colour, arrow heads) with callouts.
        let dimWidth: CGFloat = hc ? 1.5 : 1
        for arc in l.arcs {
            var path = Path()
            path.addLines(arc.points)
            let drawn = path.trimmedPath(from: 0, to: progress)
            context.stroke(drawn, with: .color(Theme.primary), lineWidth: highlighted == arc.pos ? dimWidth + 1 : dimWidth)
            if progress >= 1, arc.points.count > 2 {
                let n = arc.points.count
                context.fill(Drafting.arrowHead(tip: arc.points[0], from: arc.points[1]), with: .color(Theme.primary))
                context.fill(Drafting.arrowHead(tip: arc.points[n - 1], from: arc.points[n - 2]), with: .color(Theme.primary))
            }
            Drafting.callout(context, number: arc.pos, at: arc.mid, diameter: calloutDiameter,
                             highlighted: highlighted == arc.pos, lineWidth: dimWidth)
        }
    }
}
