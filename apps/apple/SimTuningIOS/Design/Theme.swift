import SwiftUI
import UIKit

/// "Werkstattzeichnung" design tokens (see `DESIGN.md` at the repository root). Every colour is
/// adaptive: dark (drafting table at night, the primary look) and light ("Lichtpause", diazo copy
/// on cream paper), each with an Increase Contrast variant where the base value is a hairline or
/// secondary ink. Keep these hex values in sync with the web tokens.
enum Theme {
    // MARK: Surfaces

    /// Drafting table / paper.
    static let background = adaptive(dark: 0x111614, light: 0xECE7DA)
    /// Sheet (grouped rows, title block).
    static let card = adaptive(dark: 0x19201D, light: 0xF7F4EC)
    /// Raised sheet, inputs, secondary / muted surfaces.
    static let raised = adaptive(dark: 0x222B27, light: 0xE3DDCD)
    /// 1 pt hairline. No drop shadows anywhere.
    static let border = adaptive(dark: 0x33403A, light: 0xC9C2B0, darkHC: 0x6A7A72, lightHC: 0x8C8573)

    // MARK: Ink

    static let foreground = adaptive(dark: 0xE9E5D8, light: 0x1B2333)
    static let mutedForeground = adaptive(dark: 0xA3AB9F, light: 0x545C6B, darkHC: 0xCBD0C6, lightHC: 0x363D4A)

    // MARK: Signal

    /// Selection, dimension lines, primary action: Signalgelb (dark) / Pausblau (light).
    static let primary = adaptive(dark: 0xF2B92C, light: 0x1E4F9C, lightHC: 0x173E7C)
    /// Text on `primary` fills.
    static let primaryForeground = adaptive(dark: 0x1A1406, light: 0xFFFFFF)
    static let destructive = adaptive(dark: 0xF07A6A, light: 0xA8361F)
    static let success = adaptive(dark: 0x6DBB8A, light: 0x2A6E46)
    static let warning = adaptive(dark: 0xF2B92C, light: 0x7A5A2C)

    // MARK: Module colours (classic Simson enamel)

    static let motor = adaptive(dark: 0xCDA977, light: 0x7A5A2C)        // Saharabraun
    static let einlass = adaptive(dark: 0x5AA7D6, light: 0x1B6797)      // Lagunenblau
    static let auslass = adaptive(dark: 0xE66A52, light: 0xA8361F)      // Ibisrot
    static let ueberstroemer = adaptive(dark: 0x6DBB8A, light: 0x2A6E46) // Billardgrün
    static let dyno = primary
    static let vehicles = foreground

    /// Chart series order: 1 signal, 2 Lagunenblau, 3 Ibisrot, 4 Billardgrün, 5 Saharabraun.
    static let series: [Color] = [primary, einlass, auslass, ueberstroemer, motor]

    /// 5 mm drawing grid (diagrams only, never behind forms): ink at 6 % (dark) / 7 % (light).
    static let grid = Color(uiColor: UIColor { trait in
        let hc = trait.accessibilityContrast == .high
        return trait.userInterfaceStyle == .dark
            ? UIColor(hex: 0xE9E5D8, alpha: hc ? 0.13 : 0.06)
            : UIColor(hex: 0x1B2333, alpha: hc ? 0.15 : 0.07)
    })

    // MARK: Shape & spacing

    enum Radius {
        /// Inputs, cards, buttons, drawings.
        static let standard: CGFloat = 4
        /// Tags.
        static let tag: CGFloat = 2
    }

    enum Spacing {
        static let xs: CGFloat = 4
        static let s: CGFloat = 8
        static let m: CGFloat = 12
        static let l: CGFloat = 16
        static let xl: CGFloat = 24
    }

    /// Hairline width (1 pt, 1.5 pt with Increase Contrast).
    static func hairline(_ contrast: ColorSchemeContrast) -> CGFloat { contrast == .increased ? 1.5 : 1 }

    /// Drawing grid pitch: 5 mm at the web's 96 dpi reference (≈ 19 px), rounded to the pt grid.
    static let gridPitch: CGFloat = 18

    // MARK: - Helpers

    private static func adaptive(dark: UInt32, light: UInt32, darkHC: UInt32? = nil, lightHC: UInt32? = nil) -> Color {
        Color(uiColor: UIColor { trait in
            let hc = trait.accessibilityContrast == .high
            if trait.userInterfaceStyle == .dark {
                return UIColor(hex: hc ? darkHC ?? dark : dark)
            }
            return UIColor(hex: hc ? lightHC ?? light : light)
        })
    }

    /// Navigation bar titles in condensed SF Pro (DIN-like lettering); bars stay system bars.
    @MainActor
    static func applyNavigationAppearance() {
        let large = UIFontMetrics(forTextStyle: .largeTitle)
            .scaledFont(for: .systemFont(ofSize: 34, weight: .bold, width: .condensed), maximumPointSize: 44)
        let inline = UIFontMetrics(forTextStyle: .headline)
            .scaledFont(for: .systemFont(ofSize: 17, weight: .semibold, width: .condensed), maximumPointSize: 24)
        let proxy = UINavigationBar.appearance()
        proxy.largeTitleTextAttributes = [.font: large, .kern: 0.6]
        proxy.titleTextAttributes = [.font: inline, .kern: 0.6]
    }
}

extension UIColor {
    convenience init(hex: UInt32, alpha: CGFloat = 1) {
        self.init(red: CGFloat((hex >> 16) & 0xFF) / 255,
                  green: CGFloat((hex >> 8) & 0xFF) / 255,
                  blue: CGFloat(hex & 0xFF) / 255,
                  alpha: alpha)
    }
}

// MARK: - Modules

/// The five drawing sheets of the manual. The module colour is the section colour everywhere
/// (tab tint, title-block rule, port lines in drawings).
enum Module: Int, CaseIterable, Identifiable {
    case vehicles = 1, motor, einlass, auslass, dyno

    var id: Int { rawValue }
    /// Sheet number in the title block (`Blatt n`).
    var sheet: Int { rawValue }

    var color: Color {
        switch self {
        case .vehicles: Theme.vehicles
        case .motor: Theme.motor
        case .einlass: Theme.einlass
        case .auslass: Theme.auslass
        case .dyno: Theme.dyno
        }
    }

    var title: LocalizedStringKey {
        switch self {
        case .vehicles: "Fahrzeuge"
        case .motor: "Motor"
        case .einlass: "Einlass"
        case .auslass: "Auslass"
        case .dyno: "Dyno"
        }
    }
}

// MARK: - Type

extension Font {
    /// Headings: condensed SF Pro (DIN 1451 feel), Dynamic Type text style.
    static func stHeading(_ style: Font.TextStyle = .headline, weight: Font.Weight = .semibold) -> Font {
        .system(style, weight: weight).width(.condensed)
    }

    static var stHeading: Font { stHeading() }

    /// Values: SF Mono (tabular, slashed zero), so numbers never jump when they change.
    static func stData(_ style: Font.TextStyle = .body, weight: Font.Weight = .medium) -> Font {
        .system(style, design: .monospaced, weight: weight)
    }

    static var stData: Font { stData() }

    /// Units and result labels: one step smaller, small caps.
    static var stUnit: Font { .system(.footnote).lowercaseSmallCaps() }
}

extension View {
    /// Heading lettering: condensed, uppercase, tracking 0.04 em.
    func stHeadingStyle(_ style: Font.TextStyle = .headline) -> some View {
        font(.stHeading(style)).textCase(.uppercase).tracking(0.6)
    }

    /// Unit lettering: small caps, tracking 0.06 em, secondary ink.
    func stUnitStyle() -> some View {
        font(.stUnit).tracking(0.8).foregroundStyle(Theme.mutedForeground)
    }

    /// Screen chrome: drafting-table background behind a list or form (no system grouped grey).
    /// Plain grouped sections (edge-to-edge sheets with hairline seams) instead of rounded inset
    /// cards, so surfaces keep the 4 pt drafting look.
    func stScreen() -> some View {
        listStyle(.grouped)
            .scrollContentBackground(.hidden)
            .background(Theme.background, ignoresSafeAreaEdges: .all)
    }

    /// Row chrome inside a list/form: sheet surface and hairline separators.
    func stRows() -> some View {
        listRowBackground(Theme.card)
            .listRowSeparatorTint(Theme.border)
    }
}
