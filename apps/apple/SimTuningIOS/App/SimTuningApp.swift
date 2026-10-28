import SwiftUI

@main
struct SimTuningApp: App {
    @State private var model = AppModel.makeDefault()

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

enum AppTab: String, CaseIterable, Hashable {
    case vehicles, motor, einlass, auslass, dyno, profile
}

struct RootView: View {
    @Environment(AppModel.self) private var model

    var body: some View {
        @Bindable var model = model
        TabView(selection: $model.selectedTab) {
            Tab("Fahrzeuge", systemImage: "car.2", value: AppTab.vehicles) {
                VehiclesView()
            }
            Tab("Motor", systemImage: "engine.combustion", value: AppTab.motor) {
                MotorView()
            }
            Tab("Einlass", systemImage: "wind", value: AppTab.einlass) {
                EinlassView()
            }
            Tab("Auslass", systemImage: "flame", value: AppTab.auslass) {
                AuslassView()
            }
            Tab("Dyno", systemImage: "gauge.with.dots.needle.67percent", value: AppTab.dyno) {
                DynoView()
            }
            Tab("Profil", systemImage: "person.crop.circle", value: AppTab.profile) {
                ProfileView()
            }
        }
        .alert("Fehler", isPresented: Binding(get: { model.errorMessage != nil },
                                               set: { if !$0 { model.errorMessage = nil } })) {
            Button("OK", role: .cancel) { model.errorMessage = nil }
        } message: {
            Text(verbatim: model.errorMessage ?? "")
        }
    }
}
