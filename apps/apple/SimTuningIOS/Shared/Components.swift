import SimTuningAPI
import SwiftUI

/// Decimal input bound to an optional `Double`. Parses with the current locale (comma or dot),
/// updates on every keystroke so results stay live, and supports Dynamic Type and VoiceOver.
struct NumberField: View {
    let title: LocalizedStringKey
    let unit: String?
    @Binding var value: Double?
    var identifier: String?

    @Environment(\.locale) private var locale
    @State private var text = ""
    @FocusState private var focused: Bool

    init(_ title: LocalizedStringKey, value: Binding<Double?>, unit: String? = nil, identifier: String? = nil) {
        self.title = title
        _value = value
        self.unit = unit
        self.identifier = identifier
    }

    var body: some View {
        LabeledContent {
            HStack(spacing: 4) {
                // Short prompt: a long monospaced placeholder would push the value onto a second line.
                TextField(title, text: $text, prompt: Text(verbatim: "–"))
                    .font(.stData)
                    .keyboardType(.decimalPad)
                    .multilineTextAlignment(.trailing)
                    .focused($focused)
                    .accessibilityIdentifier(identifier ?? "")
                    .onChange(of: text) { _, newValue in
                        let parsed = Self.parse(newValue, locale: locale)
                        if parsed != value { value = parsed }
                    }
                if let unit {
                    Text(verbatim: unit)
                        .stUnitStyle()
                        .accessibilityHidden(true)
                }
            }
        } label: {
            Text(title)
        }
        .onAppear { text = Self.format(value, locale: locale) }
        .onChange(of: value) { _, newValue in
            if !focused || Self.parse(text, locale: locale) != newValue {
                text = Self.format(newValue, locale: locale)
            }
        }
    }

    static func parse(_ text: String, locale: Locale) -> Double? {
        var s = text.filter { !$0.isWhitespace }
        guard !s.isEmpty else { return nil }
        if let comma = s.lastIndex(of: ","), let dot = s.lastIndex(of: ".") {
            // Both present: the last one is the decimal separator, the other one groups digits.
            s = comma > dot
                ? s.replacingOccurrences(of: ".", with: "").replacingOccurrences(of: ",", with: ".")
                : s.replacingOccurrences(of: ",", with: "")
        } else if locale.decimalSeparator == "," || s.contains(",") {
            s = s.replacingOccurrences(of: ",", with: ".")
        }
        guard let number = Double(s), number.isFinite, abs(number) <= SimValidation.maxAbs else { return nil }
        return number
    }

    static func format(_ value: Double?, locale: Locale) -> String {
        guard let value else { return "" }
        return value.formatted(.number.precision(.fractionLength(0...6)).grouping(.never).locale(locale))
    }
}

/// Read-only result row.
struct ResultRow: View {
    let title: LocalizedStringKey
    let value: String
    var identifier: String?

    var body: some View {
        LabeledContent {
            Text(verbatim: value)
                .font(.stData(.body, weight: .semibold))
                .foregroundStyle(Theme.foreground)
                .textSelection(.enabled)
        } label: {
            Text(title)
        }
        .accessibilityElement(children: .combine)
        .accessibilityIdentifier(identifier ?? "")
    }
}

/// Toolbar menu to pick the vehicle shared by all module tabs.
struct VehicleSelectionMenu: View {
    @Environment(AppModel.self) private var model

    var body: some View {
        Menu {
            ForEach(model.vehicles) { vehicle in
                Button {
                    model.selectedVehicleId = vehicle.id
                } label: {
                    if vehicle.id == model.selectedVehicleId {
                        Label(vehicle.name, systemImage: "checkmark")
                    } else {
                        Text(verbatim: vehicle.name)
                    }
                }
            }
        } label: {
            Label {
                Text(verbatim: model.selectedVehicle?.name ?? String(localized: "Fahrzeug wählen"))
            } icon: {
                Image(systemName: "car.side")
            }
            .labelStyle(.titleAndIcon)
        }
        .accessibilityLabel(Text("Fahrzeug wählen"))
        .accessibilityValue(Text(verbatim: model.selectedVehicle?.name ?? ""))
        .accessibilityIdentifier("vehicle.selection")
    }
}

/// Reset / save actions of a calculator. "Werte aus Fahrzeug übernehmen" resets the work draft to
/// the selected vehicle's values. Saving into a starter vehicle creates (and selects) an own copy;
/// the result is confirmed inline with an undo for the copy.
struct VehicleActionsSection: View {
    @Environment(AppModel.self) private var model
    let calculator: Calculator
    let apply: () -> Void
    let save: (() -> Void)?

    private var isStarter: Bool { model.selectedVehicle?.isSystem == true }

    private var feedback: SaveFeedback? {
        model.saveFeedback.flatMap { $0.calculator == calculator ? $0 : nil }
    }

    var body: some View {
        Section {
            Button("Werte aus Fahrzeug übernehmen", systemImage: "arrow.down.doc") {
                model.resetDraft(calculator)
                apply()
                model.markDraft(calculator)
            }
            .disabled(model.selectedVehicle == nil)
            .accessibilityIdentifier("vehicle.apply")
            if let save {
                Button(isStarter ? LocalizedStringKey("Als eigenes Fahrzeug speichern")
                                 : LocalizedStringKey("Im Fahrzeug speichern"),
                       systemImage: isStarter ? "plus.square.on.square" : "square.and.arrow.down", action: save)
                    .disabled(model.selectedVehicle == nil)
                    .accessibilityIdentifier("vehicle.save")
            }
            if let feedback {
                SaveFeedbackRow(feedback: feedback)
                    .transition(.opacity)
            }
        } footer: {
            if isStarter, save != nil, feedback == nil {
                Text("Startfahrzeuge sind schreibgeschützt. Beim Speichern wird eine Kopie angelegt.")
            }
        }
        .animation(.default, value: feedback?.id)
        .sensoryFeedback(.success, trigger: feedback?.id) { _, new in new != nil }
    }
}

/// Inline confirmation after saving (success colour, hairline rule, optional undo).
private struct SaveFeedbackRow: View {
    @Environment(AppModel.self) private var model
    let feedback: SaveFeedback

    var body: some View {
        HStack(alignment: .firstTextBaseline, spacing: Theme.Spacing.s) {
            Image(systemName: "checkmark.circle.fill")
                .foregroundStyle(Theme.success)
                .accessibilityHidden(true)
            Group {
                switch feedback.kind {
                case .updated:
                    Text("Im Fahrzeug gespeichert")
                case let .copied(name, _, _):
                    Text("Kopie ‚\(name)‘ angelegt und ausgewählt")
                }
            }
            .font(.subheadline)
            .foregroundStyle(Theme.foreground)
            .frame(maxWidth: .infinity, alignment: .leading)
            .accessibilityIdentifier("vehicle.save.feedback")
            if case .copied = feedback.kind {
                Button("Rückgängig") {
                    Task { await model.undoStarterCopy(feedback) }
                }
                .buttonStyle(.borderless)
                .font(.subheadline.weight(.semibold))
                .accessibilityIdentifier("vehicle.save.undo")
            }
        }
        .accessibilityElement(children: .contain)
    }
}

/// Module sheet: grouped form on the drafting-table background, opened by the Schriftfeld.
/// Rows sit on the `card` surface with hairline separators (no shadows, no system grey).
struct SheetForm<Content: View>: View {
    let module: Module
    var subsheet: Int?
    var subtitle: LocalizedStringKey?
    @ViewBuilder let content: Content

    var body: some View {
        // A grouped `List` rather than `Form`: on iOS a Form always renders inset, rounded cards;
        // the list keeps the same native rows and controls with edge-to-edge sheets.
        List {
            Section {
                Schriftfeld(module: module, subsheet: subsheet, subtitle: subtitle)
                    .schriftfeldRow()
            }
            Group { content }
                .stRows()
        }
        // The decimal pad has no return key: scrolling puts the keyboard away.
        .scrollDismissesKeyboard(.immediately)
        .stScreen()
        .errorBanner()
    }
}

/// Module index list (hub screens) with the same chrome as `SheetForm`.
struct SheetList<Content: View>: View {
    let module: Module
    @ViewBuilder let content: Content

    var body: some View {
        List {
            Section {
                Schriftfeld(module: module)
                    .schriftfeldRow()
            }
            Group { content }
                .stRows()
        }
        .stScreen()
        .errorBanner()
    }
}

extension View {
    /// Navigation chrome of a module's root screen: title, shared vehicle picker and Profil.
    func moduleChrome(_ title: LocalizedStringKey) -> some View {
        navigationTitle(title)
            .toolbar {
                ToolbarItemGroup(placement: .topBarTrailing) {
                    VehicleSelectionMenu()
                    ProfileToolbarButton()
                }
            }
    }

    /// Navigation chrome of a calculator pushed from a module (title + shared vehicle picker).
    func calculatorChrome(_ title: LocalizedStringKey) -> some View {
        navigationTitle(title)
            .toolbar {
                ToolbarItem(placement: .topBarTrailing) { VehicleSelectionMenu() }
            }
    }

    /// Seeds the calculator's work draft from the selected vehicle the first time it is opened
    /// for that vehicle (and when the selection changes to a vehicle without a draft).
    func calculatorDraft(_ calculator: Calculator, seed: @escaping () -> Void) -> some View {
        modifier(CalculatorDraftModifier(calculator: calculator, seed: seed))
    }

    /// Non-modal error banner for `AppModel.errorMessage` below the navigation bar. Applied inside
    /// each navigation stack (`SheetForm`, `SheetList`, sheets): UIKit navigation bars ignore a
    /// SwiftUI safe-area inset set on the enclosing `TabView`.
    func errorBanner() -> some View {
        modifier(ErrorBannerModifier())
    }
}

/// Opens Profil (account, appearance, units) as a sheet.
struct ProfileToolbarButton: View {
    @Environment(AppModel.self) private var model

    var body: some View {
        Button {
            model.isProfilePresented = true
        } label: {
            Image(systemName: model.isSignedIn ? "person.crop.circle.fill" : "person.crop.circle")
        }
        .accessibilityLabel(Text("Profil"))
        .accessibilityIdentifier("profile.open")
    }
}

/// Calculator input kept in the per-vehicle work draft of `AppModel` (survives navigation and
/// relaunches). `default` applies while the field is not in the draft yet.
@MainActor
@propertyWrapper
struct Draft: DynamicProperty {
    @Environment(AppModel.self) private var model
    private let calculator: Calculator
    private let field: String
    private let defaultValue: Double?

    init(_ calculator: Calculator, _ field: String, default defaultValue: Double? = nil) {
        self.calculator = calculator
        self.field = field
        self.defaultValue = defaultValue
    }

    var wrappedValue: Double? {
        get { model.draftValue(calculator, field) ?? defaultValue }
        nonmutating set { model.setDraftValue(calculator, field, newValue) }
    }

    var projectedValue: Binding<Double?> {
        Binding(get: { wrappedValue }, set: { wrappedValue = $0 })
    }
}

private struct CalculatorDraftModifier: ViewModifier {
    @Environment(AppModel.self) private var model
    let calculator: Calculator
    let seed: () -> Void

    func body(content: Content) -> some View {
        content
            .onAppear(perform: seedIfNeeded)
            .onChange(of: model.selectedVehicleId) { _, _ in seedIfNeeded() }
    }

    private func seedIfNeeded() {
        guard model.selectedVehicle != nil, !model.hasDraft(calculator) else { return }
        seed()
        model.markDraft(calculator)
    }
}

private struct ErrorBannerModifier: ViewModifier {
    @Environment(AppModel.self) private var model

    func body(content: Content) -> some View {
        content
            .safeAreaInset(edge: .top, spacing: 0) {
                if let message = model.errorMessage {
                    ErrorBanner(message: message) { model.errorMessage = nil }
                        .transition(.move(edge: .top).combined(with: .opacity))
                }
            }
            .animation(.snappy, value: model.errorMessage)
    }
}

/// Drawing-sheet style notice: card surface, hairline rule in the destructive colour, icon + text.
struct ErrorBanner: View {
    let message: String
    let dismiss: () -> Void
    @Environment(\.colorSchemeContrast) private var contrast

    var body: some View {
        HStack(alignment: .center, spacing: Theme.Spacing.s) {
            Image(systemName: "exclamationmark.triangle.fill")
                .foregroundStyle(Theme.destructive)
                .accessibilityHidden(true)
            Text(verbatim: message)
                .font(.subheadline)
                .foregroundStyle(Theme.foreground)
                .frame(maxWidth: .infinity, alignment: .leading)
                .accessibilityLabel(Text("Fehler") + Text(verbatim: ": \(message)"))
            Button(action: dismiss) {
                Image(systemName: "xmark")
                    .font(.subheadline.weight(.semibold))
                    .foregroundStyle(Theme.mutedForeground)
                    .frame(width: 44, height: 44)
                    .contentShape(Rectangle())
            }
            .accessibilityLabel(Text("Fehlermeldung schließen"))
            .accessibilityIdentifier("error.dismiss")
        }
        .padding(.leading, Theme.Spacing.m)
        .background(Theme.card, in: RoundedRectangle(cornerRadius: Theme.Radius.standard))
        .overlay {
            RoundedRectangle(cornerRadius: Theme.Radius.standard)
                .strokeBorder(Theme.destructive, lineWidth: Theme.hairline(contrast))
        }
        .padding(.horizontal, Theme.Spacing.l)
        .padding(.vertical, Theme.Spacing.xs)
        .accessibilityElement(children: .contain)
        .accessibilityIdentifier("error.banner")
    }
}

/// SimTuning logo from `assets/brand` (vector asset). `.mark` drops the TUNING wordmark for
/// small sizes where it would not be legible.
struct BrandLogo: View {
    enum Variant { case full, mark }
    var variant: Variant = .full
    var size: CGFloat

    var body: some View {
        Image(variant == .full ? "Logo" : "LogoMark")
            .resizable()
            .interpolation(.high)
            .scaledToFit()
            .frame(width: size, height: size)
            .accessibilityLabel(Text(verbatim: "SimTuning"))
    }
}
