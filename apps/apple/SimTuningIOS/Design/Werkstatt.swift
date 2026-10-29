import SwiftUI

// MARK: - Schriftfeld (title block)

/// Ruled title block that opens every module screen: `BLATT n · MODUL`, the selected vehicle,
/// engine, unit system and date. Its top rule carries the module colour.
struct Schriftfeld: View {
    @Environment(AppModel.self) private var model
    @Environment(\.colorSchemeContrast) private var contrast
    @Environment(\.locale) private var locale

    let module: Module
    /// Sub-sheet number for calculator screens (`Blatt 2.3`).
    var subsheet: Int?
    var subtitle: LocalizedStringKey?

    private var sheetNumber: String {
        subsheet.map { "\(module.sheet).\($0)" } ?? "\(module.sheet)"
    }

    var body: some View {
        VStack(alignment: .leading, spacing: 0) {
            Rectangle()
                .fill(module.color)
                .frame(height: 3)
                .accessibilityHidden(true)
            VStack(alignment: .leading, spacing: Theme.Spacing.xs) {
                ViewThatFits(in: .horizontal) {
                    HStack(alignment: .firstTextBaseline, spacing: Theme.Spacing.s) {
                        sheetLabel
                        Spacer(minLength: Theme.Spacing.s)
                        dateLabel
                    }
                    VStack(alignment: .leading, spacing: 2) {
                        sheetLabel
                        dateLabel
                    }
                }
                Text(verbatim: model.selectedVehicle?.name ?? String(localized: "Kein Fahrzeug ausgewählt"))
                    .font(.stHeading(.title3, weight: .bold))
                    .foregroundStyle(Theme.foreground)
                Text(verbatim: engineLine)
                    .stUnitStyle()
            }
            .padding(.horizontal, Theme.Spacing.m)
            .padding(.vertical, Theme.Spacing.s + 2)
        }
        .frame(maxWidth: .infinity, alignment: .leading)
        .background(Theme.card)
        .clipShape(RoundedRectangle(cornerRadius: Theme.Radius.standard))
        .overlay(
            RoundedRectangle(cornerRadius: Theme.Radius.standard)
                .strokeBorder(Theme.border, lineWidth: Theme.hairline(contrast))
        )
        .accessibilityElement(children: .combine)
        .accessibilityAddTraits(.isHeader)
        .accessibilityIdentifier("schriftfeld.\(module.sheet)")
    }

    private var sheetLabel: some View {
        Group {
            if let subtitle {
                Text("Blatt \(sheetNumber) · \(Text(module.title)) · \(Text(subtitle))")
            } else {
                Text("Blatt \(sheetNumber) · \(Text(module.title))")
            }
        }
        .stHeadingStyle(.subheadline)
        .foregroundStyle(Theme.foreground)
    }

    private var dateLabel: some View {
        Text(verbatim: Date.now.formatted(.dateTime.day(.twoDigits).month(.twoDigits).year().locale(locale)))
            .font(.stData(.caption, weight: .regular))
            .foregroundStyle(Theme.mutedForeground)
    }

    private var engineLine: String {
        let engine = model.selectedVehicle?.motor?.name
        return [engine, String(localized: "Maße in mm")].compactMap { $0 }.joined(separator: " · ")
    }
}

extension View {
    /// Places the title block as the first, edge-to-edge item of a grouped list or form.
    func schriftfeldRow() -> some View {
        listRowInsets(EdgeInsets(top: 0, leading: Theme.Spacing.l, bottom: 0, trailing: Theme.Spacing.l))
            .listRowBackground(Color.clear)
            .listRowSeparator(.hidden)
    }
}

// MARK: - Callout (Positionsnummer)

/// Circled monospaced position number, repeated in the drawing and the Stückliste.
struct Callout: View {
    let number: Int
    var highlighted = false
    @ScaledMetric(relativeTo: .caption) private var size: CGFloat = 22

    var body: some View {
        Text(verbatim: "\(number)")
            .font(.stData(.caption, weight: .semibold))
            .foregroundStyle(highlighted ? Theme.primaryForeground : Theme.primary)
            .frame(minWidth: size, minHeight: size)
            .background(Circle().fill(highlighted ? Theme.primary : Color.clear))
            .overlay(Circle().strokeBorder(Theme.primary, lineWidth: 1))
            .accessibilityLabel(Text("Position \(number)"))
    }
}

// MARK: - Stückliste (parts list)

struct StuecklisteItem: Identifiable {
    let pos: Int
    let title: LocalizedStringKey
    let value: String
    let unit: String?
    var identifier: String?

    var id: Int { pos }
}

/// Parts list: Pos · Benennung · Wert · Einheit. Tapping a row highlights the matching callout in
/// the drawing (and vice versa through the shared `highlighted` binding).
struct Stueckliste: View {
    let items: [StuecklisteItem]
    @Binding var highlighted: Int?
    var footer: LocalizedStringKey?

    @Environment(\.dynamicTypeSize) private var dynamicTypeSize
    @ScaledMetric(relativeTo: .footnote) private var posWidth: CGFloat = 34
    @ScaledMetric(relativeTo: .footnote) private var unitWidth: CGFloat = 72

    var body: some View {
        Section {
            if !dynamicTypeSize.isAccessibilitySize {
                HStack(spacing: Theme.Spacing.s) {
                    Text("Pos").frame(width: posWidth, alignment: .leading)
                    Text("Benennung")
                    Spacer()
                    Text("Wert")
                    Text("Einheit").frame(width: unitWidth, alignment: .leading)
                }
                .stUnitStyle()
                .accessibilityHidden(true)
                .listRowBackground(Theme.card)
                .listRowSeparatorTint(Theme.border)
            }
            ForEach(items) { item in
                row(item)
                    .listRowBackground(highlighted == item.pos ? Theme.raised : Theme.card)
                    .listRowSeparatorTint(Theme.border)
            }
        } header: {
            Text("Stückliste")
        } footer: {
            if let footer { Text(footer) }
        }
    }

    private func row(_ item: StuecklisteItem) -> some View {
        let isOn = highlighted == item.pos
        return Button {
            highlighted = isOn ? nil : item.pos
        } label: {
            Group {
                if dynamicTypeSize.isAccessibilitySize {
                    VStack(alignment: .leading, spacing: Theme.Spacing.xs) {
                        HStack(spacing: Theme.Spacing.s) {
                            Callout(number: item.pos, highlighted: isOn)
                            Text(item.title).foregroundStyle(Theme.foreground)
                        }
                        HStack(alignment: .firstTextBaseline, spacing: Theme.Spacing.xs) {
                            Text(verbatim: item.value).font(.stData).foregroundStyle(Theme.foreground)
                            if let unit = item.unit { Text(verbatim: unit).stUnitStyle() }
                        }
                    }
                } else {
                    HStack(alignment: .center, spacing: Theme.Spacing.s) {
                        Callout(number: item.pos, highlighted: isOn)
                            .frame(width: posWidth, alignment: .leading)
                        Text(item.title)
                            .foregroundStyle(Theme.foreground)
                            .frame(maxWidth: .infinity, alignment: .leading)
                        Text(verbatim: item.value)
                            .font(.stData)
                            .foregroundStyle(Theme.foreground)
                            .multilineTextAlignment(.trailing)
                        Text(verbatim: item.unit ?? "")
                            .stUnitStyle()
                            .frame(width: unitWidth, alignment: .leading)
                    }
                }
            }
            .contentShape(Rectangle())
        }
        .buttonStyle(.plain)
        .accessibilityElement(children: .ignore)
        .accessibilityLabel(Text("Position \(item.pos), \(Text(item.title))"))
        .accessibilityValue(Text(verbatim: [item.value, item.unit].compactMap { $0 }.joined(separator: " ")))
        .accessibilityHint(Text("Hebt die Position in der Zeichnung hervor"))
        .accessibilityAddTraits(isOn ? [.isSelected, .isButton] : .isButton)
        .accessibilityIdentifier(item.identifier ?? "stueckliste.\(item.pos)")
    }
}

// MARK: - Drawing sheet

/// 5 mm drawing grid. Only behind diagrams, never behind forms.
struct DrawingGrid: View {
    var body: some View {
        Canvas { context, size in
            var path = Path()
            var x: CGFloat = 0
            while x <= size.width {
                path.move(to: CGPoint(x: x, y: 0))
                path.addLine(to: CGPoint(x: x, y: size.height))
                x += Theme.gridPitch
            }
            var y: CGFloat = 0
            while y <= size.height {
                path.move(to: CGPoint(x: 0, y: y))
                path.addLine(to: CGPoint(x: size.width, y: y))
                y += Theme.gridPitch
            }
            context.stroke(path, with: .color(Theme.grid), lineWidth: 1)
        }
        .accessibilityHidden(true)
    }
}

extension View {
    /// Sheet surface with the drawing grid and a hairline frame, radius 4.
    func drawingSheet() -> some View {
        modifier(DrawingSheetModifier())
    }
}

private struct DrawingSheetModifier: ViewModifier {
    @Environment(\.colorSchemeContrast) private var contrast

    func body(content: Content) -> some View {
        content
            .padding(Theme.Spacing.s)
            .background {
                ZStack {
                    Theme.card
                    DrawingGrid()
                }
            }
            .clipShape(RoundedRectangle(cornerRadius: Theme.Radius.standard))
            .overlay(
                RoundedRectangle(cornerRadius: Theme.Radius.standard)
                    .strokeBorder(Theme.border, lineWidth: Theme.hairline(contrast))
            )
    }
}

/// Drawing row inside a form: edge-to-edge sheet without the system row background.
extension View {
    func drawingRow() -> some View {
        listRowInsets(EdgeInsets(top: 0, leading: Theme.Spacing.l, bottom: 0, trailing: Theme.Spacing.l))
            .listRowBackground(Color.clear)
            .listRowSeparator(.hidden)
    }
}

/// Height follows the width (`width / aspect + extra`), so drawings scale to the row width.
struct AspectHeightLayout: Layout {
    let aspect: CGFloat
    var extra: CGFloat = 0

    func sizeThatFits(proposal: ProposedViewSize, subviews: Subviews, cache: inout ()) -> CGSize {
        let width = proposal.width ?? 320
        return CGSize(width: width, height: width / max(aspect, 0.1) + extra)
    }

    func placeSubviews(in bounds: CGRect, proposal: ProposedViewSize, subviews: Subviews, cache: inout ()) {
        for subview in subviews {
            subview.place(at: bounds.origin, proposal: ProposedViewSize(bounds.size))
        }
    }
}

// MARK: - Port line styles

/// Port lines differ by colour, label and dash pattern (never colour alone).
enum PortStyle {
    case einlass, auslass, ueberstroemer

    var color: Color {
        switch self {
        case .einlass: Theme.einlass
        case .auslass: Theme.auslass
        case .ueberstroemer: Theme.ueberstroemer
        }
    }

    /// Einlass solid, Auslass long dash, Überströmer dot-dash.
    var dash: [CGFloat] {
        switch self {
        case .einlass: []
        case .auslass: [10, 5]
        case .ueberstroemer: [9, 3, 2, 3]
        }
    }
}

/// Legend entry: line sample with the port's dash pattern + label.
struct PortLegendItem: View {
    let style: PortStyle
    let title: LocalizedStringKey

    var body: some View {
        HStack(spacing: Theme.Spacing.xs) {
            Canvas { context, size in
                var path = Path()
                path.move(to: CGPoint(x: 0, y: size.height / 2))
                path.addLine(to: CGPoint(x: size.width, y: size.height / 2))
                context.stroke(path, with: .color(style.color), style: StrokeStyle(lineWidth: 2.5, dash: style.dash))
            }
            .frame(width: 28, height: 8)
            .accessibilityHidden(true)
            Text(title).foregroundStyle(Theme.foreground)
        }
    }
}

// MARK: - Canvas helpers

enum Drafting {
    /// Filled arrow head at `tip`, pointing away from `from`.
    static func arrowHead(tip: CGPoint, from: CGPoint, size: CGFloat = 6) -> Path {
        let angle = atan2(tip.y - from.y, tip.x - from.x)
        let spread: CGFloat = .pi / 9
        var path = Path()
        path.move(to: tip)
        path.addLine(to: CGPoint(x: tip.x - size * cos(angle - spread), y: tip.y - size * sin(angle - spread)))
        path.addLine(to: CGPoint(x: tip.x - size * cos(angle + spread), y: tip.y - size * sin(angle + spread)))
        path.closeSubpath()
        return path
    }

    /// Callout drawn into a canvas: card-filled circle (breaks the line), signal ring and numeral.
    static func callout(_ context: GraphicsContext, number: Int, at center: CGPoint, diameter: CGFloat,
                        highlighted: Bool, lineWidth: CGFloat) {
        let rect = CGRect(x: center.x - diameter / 2, y: center.y - diameter / 2, width: diameter, height: diameter)
        let circle = Path(ellipseIn: rect)
        context.fill(circle, with: .color(highlighted ? Theme.primary : Theme.card))
        context.stroke(circle, with: .color(Theme.primary), lineWidth: lineWidth)
        let text = Text(verbatim: "\(number)")
            .font(.system(size: diameter * 0.58, weight: .semibold, design: .monospaced))
            .foregroundStyle(highlighted ? Theme.primaryForeground : Theme.primary)
        context.draw(text, at: center, anchor: .center)
    }
}

/// Runs the single draw-in of dimension lines (200 ms ease-out), instant with Reduce Motion.
struct DrawInModifier: ViewModifier {
    @Binding var progress: Double
    @Environment(\.accessibilityReduceMotion) private var reduceMotion

    func body(content: Content) -> some View {
        content.onAppear {
            guard progress < 1 else { return }
            if reduceMotion {
                progress = 1
            } else {
                withAnimation(.easeOut(duration: 0.2)) { progress = 1 }
            }
        }
    }
}
