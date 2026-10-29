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

/// Section footer that tells the user values come from / go to the selected vehicle.
struct VehicleActionsSection: View {
    @Environment(AppModel.self) private var model
    let apply: () -> Void
    let save: (() -> Void)?

    var body: some View {
        Section {
            Button("Werte aus Fahrzeug übernehmen", systemImage: "arrow.down.doc", action: apply)
                .disabled(model.selectedVehicle == nil)
            if let save {
                Button("Im Fahrzeug speichern", systemImage: "square.and.arrow.down", action: save)
                    .disabled(model.selectedVehicle == nil)
            }
        } footer: {
            if model.selectedVehicle?.isSystem == true, save != nil {
                Text("Startfahrzeuge sind schreibgeschützt. Beim Speichern wird eine Kopie angelegt.")
            }
        }
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
    }
}

extension View {
    /// Standard module navigation chrome (title + shared vehicle picker).
    func moduleChrome(_ title: LocalizedStringKey) -> some View {
        navigationTitle(title)
            .toolbar {
                ToolbarItem(placement: .topBarTrailing) { VehicleSelectionMenu() }
            }
    }
}
