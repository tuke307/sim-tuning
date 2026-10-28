import Foundation
import Observation
import SimTuningAPI
import SimTuningLogic
import SimTuningStore
import SwiftUI

/// User preferences (mirrors `UpdatePreferencesSchema`): language, theme and unit rounding.
struct Preferences: Codable, Equatable {
    var language: LanguagePreference = .system
    var theme: ThemePreference = .system
    var roundingAccuracy: Int = RoundingAccuracy.default
    var roundOnUnitChange = true

    var locale: Locale {
        switch language {
        case .system: return Locale.autoupdatingCurrent
        case .de: return Locale(identifier: "de_DE")
        case .en: return Locale(identifier: "en_US")
        }
    }

    var colorScheme: ColorScheme? {
        switch theme {
        case .system: return nil
        case .light: return .light
        case .dark: return .dark
        }
    }

    var rounding: Units.RoundingOptions {
        Units.RoundingOptions(roundingAccuracy: roundingAccuracy, roundOnUnitChange: roundOnUnitChange)
    }
}

/// App-wide state: active repository (guest/local or signed-in/API), vehicle list and the vehicle
/// selection shared by all module tabs.
@MainActor
@Observable
final class AppModel {
    let client: APIClient
    let localRepository: LocalRepository
    private let defaults: UserDefaults
    let isUITesting: Bool

    var selectedTab: AppTab = .vehicles
    var repository: any SimRepository
    var user: UserDto?
    var vehicles: [VehicleDto] = []
    var environments: [EnvironmentDto] = []
    var isLoading = false
    var errorMessage: String?

    var selectedVehicleId: String? {
        didSet { defaults.set(selectedVehicleId, forKey: Keys.selectedVehicle) }
    }

    var preferences: Preferences {
        didSet {
            if let data = try? JSONEncoder().encode(preferences) { defaults.set(data, forKey: Keys.preferences) }
        }
    }

    private enum Keys {
        static let selectedVehicle = "selectedVehicleId"
        static let preferences = "preferences.v1"
    }

    var selectedVehicle: VehicleDto? {
        vehicles.first { $0.id == selectedVehicleId }
    }

    var isSignedIn: Bool { repository.kind == .api }

    init(client: APIClient, localRepository: LocalRepository, defaults: UserDefaults, isUITesting: Bool) {
        self.client = client
        self.localRepository = localRepository
        self.defaults = defaults
        self.isUITesting = isUITesting
        repository = localRepository
        selectedVehicleId = defaults.string(forKey: Keys.selectedVehicle)
        if let data = defaults.data(forKey: Keys.preferences),
           let stored = try? JSONDecoder().decode(Preferences.self, from: data) {
            preferences = stored
        } else {
            preferences = Preferences()
        }
        if client.isAuthenticated() {
            repository = APIRepository(client: client)
        }
    }

    /// `--uitesting` runs fully offline with in-memory storage and a fresh selection.
    static func makeDefault() -> AppModel {
        let uiTesting = ProcessInfo.processInfo.arguments.contains("--uitesting")
        if uiTesting {
            let suite = "uitesting"
            let defaults = UserDefaults(suiteName: suite)!
            defaults.removePersistentDomain(forName: suite)
            return AppModel(
                client: APIClient(tokenStore: InMemoryTokenStore()),
                localRepository: LocalRepository(fileURL: nil),
                defaults: defaults,
                isUITesting: true
            )
        }
        return AppModel(client: APIClient(), localRepository: LocalRepository(),
                        defaults: .standard, isUITesting: false)
    }

    func bootstrap() async {
        await client.setLanguage(preferences.locale.language.languageCode?.identifier ?? "de")
        if isSignedIn {
            do {
                user = try await client.me()
                if let p = user?.preferences {
                    preferences.roundingAccuracy = p.roundingAccuracy
                    preferences.roundOnUnitChange = p.roundOnUnitChange
                    if let language = p.language { preferences.language = language }
                    if let theme = ThemePreference(rawValue: p.theme) { preferences.theme = theme }
                }
            } catch APIClientError.unauthorized {
                repository = localRepository
            } catch {
                // Offline: keep the session, data calls will report errors.
            }
        }
        await reload()
    }

    func reload() async {
        isLoading = true
        defer { isLoading = false }
        do {
            vehicles = try await repository.listVehicles()
            environments = try await repository.listEnvironments()
            if selectedVehicle == nil { selectedVehicleId = vehicles.first?.id }
        } catch {
            report(error)
        }
    }

    func report(_ error: Error) {
        errorMessage = (error as? LocalizedError)?.errorDescription ?? error.localizedDescription
    }

    /// Runs a repository mutation, refreshes the list and reports errors to the user.
    @discardableResult
    func perform<T>(_ action: @escaping (any SimRepository) async throws -> T) async -> T? {
        do {
            let result = try await action(repository)
            await reload()
            return result
        } catch {
            report(error)
            return nil
        }
    }

    /// Saves (partial) motor values for the selected vehicle. Starter vehicles are duplicated first.
    func saveToSelectedVehicle(_ input: VehicleInputDto) async {
        guard let vehicle = selectedVehicle else { return }
        var targetId = vehicle.id
        if vehicle.isSystem {
            guard let copy = await perform({ try await $0.duplicateVehicle(id: vehicle.id, name: nil) }) else { return }
            targetId = copy.id
        }
        if await perform({ try await $0.updateVehicle(id: targetId, input) }) != nil {
            selectedVehicleId = targetId
        }
    }

    // MARK: - Auth

    func signIn(email: String, password: String) async -> Bool {
        do {
            user = try await client.login(email: email, password: password)
            repository = APIRepository(client: client)
            await reload()
            return true
        } catch {
            report(error)
            return false
        }
    }

    func register(email: String, password: String, name: String?) async -> String? {
        do {
            let result = try await client.register(RegisterDto(email: email, password: password, name: name))
            if result.tokens != nil {
                user = result
                repository = APIRepository(client: client)
                await reload()
                return nil
            }
            return result.message ?? String(localized: "Bitte bestätige deine E-Mail-Adresse.")
        } catch {
            report(error)
            return nil
        }
    }

    func signOut() async {
        await client.logout()
        user = nil
        repository = localRepository
        selectedVehicleId = nil
        await reload()
    }

    func syncPreferences() async {
        guard isSignedIn else { return }
        await client.setLanguage(preferences.locale.language.languageCode?.identifier ?? "de")
        _ = try? await client.updatePreferences(UpdatePreferencesDto(
            theme: preferences.theme, language: preferences.language,
            roundingAccuracy: preferences.roundingAccuracy, roundOnUnitChange: preferences.roundOnUnitChange
        ))
    }

    func resetGuestData() async {
        do {
            try await localRepository.reset()
            selectedVehicleId = nil
            await reload()
        } catch {
            report(error)
        }
    }

    // MARK: - Formatting

    /// Formats a result with the user's rounding accuracy (v2 `RoundingAccuracy`).
    func format(_ value: Double?, unit: String? = nil) -> String {
        guard let value, value.isFinite else { return "–" }
        let digits = preferences.roundingAccuracy
        let rounded = preferences.roundOnUnitChange ? Rounding.roundTo(value, digits) : value
        let text = rounded.formatted(.number.precision(.fractionLength(0...digits)).locale(preferences.locale))
        return unit.map { "\(text) \($0)" } ?? text
    }
}
