import SwiftUI

@main
struct SimTuningApp: App {
    @State private var model = AppModel.makeDefault()

    init() {
        Theme.applyNavigationAppearance()
    }

    var body: some Scene {
        WindowGroup {
            RootView()
                .environment(model)
                .environment(\.locale, model.preferences.locale)
                .preferredColorScheme(model.preferences.colorScheme)
                .task { await model.bootstrap() }
        }
    }
}

/// The five module tabs (iPhone shows at most five without a "More" tab). Profil opens as a sheet
/// from the toolbar of every module (`ProfileToolbarButton`).
enum AppTab: String, CaseIterable, Hashable {
    case vehicles, motor, einlass, auslass, dyno

    var module: Module {
        switch self {
        case .vehicles: .vehicles
        case .motor: .motor
        case .einlass: .einlass
        case .auslass: .auslass
        case .dyno: .dyno
        }
    }

    /// Selected tab item tint: the module's section colour.
    var tint: Color { module.color }
}

struct RootView: View {
    @Environment(AppModel.self) private var model

    var body: some View {
        @Bindable var model = model
        TabView(selection: $model.selectedTab) {
            Tab("Fahrzeuge", systemImage: "car.2", value: AppTab.vehicles) {
                VehiclesView().tint(Theme.primary)
            }
            Tab("Motor", systemImage: "engine.combustion", value: AppTab.motor) {
                MotorView().tint(Theme.primary)
            }
            Tab("Einlass", systemImage: "wind", value: AppTab.einlass) {
                EinlassView().tint(Theme.primary)
            }
            Tab("Auslass", systemImage: "flame", value: AppTab.auslass) {
                AuslassView().tint(Theme.primary)
            }
            Tab("Dyno", systemImage: "gauge.with.dots.needle.67percent", value: AppTab.dyno) {
                DynoView().tint(Theme.primary)
            }
        }
        // The tab bar tints the selected item in the module colour; content keeps the signal
        // colour for controls (set per tab above).
        .tint(model.selectedTab.tint)
        .sheet(isPresented: $model.isProfilePresented) {
            ProfileView()
                .tint(Theme.primary)
                .environment(\.locale, model.preferences.locale)
                .preferredColorScheme(model.preferences.colorScheme)
        }
    }
}
