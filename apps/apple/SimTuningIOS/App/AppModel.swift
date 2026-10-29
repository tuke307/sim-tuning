import Foundation
import Observation
import SimTuningAPI
import SimTuningLogic
import SimTuningStore
import SwiftUI

/// User preferences (mirrors `UpdatePreferencesSchema`): language, theme and unit rounding.
struct Preferences: Codable, Equatable {
    var language: LanguagePreference = .system
    /// New installs start in the dark "Werkstattzeichnung" look; `.system` follows the device.
    var theme: ThemePreference = .dark
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

/// Module calculators whose inputs are kept as per-vehicle work drafts.
enum Calculator: String, CaseIterable {
    case hubraum, verdichtung, umrechnung, steuerdiagramm, kanal, vergaser, auslassTheorie, auslassAnwendung
}

/// Inline confirmation after saving calculator values into a vehicle.
struct SaveFeedback: Identifiable, Equatable {
    enum Kind: Equatable {
        /// Saved into the user's own vehicle.
        case updated
        /// A starter vehicle was copied; the copy received the values and is now selected.
        case copied(name: String, copyId: String, starterId: String)
    }

    let id = UUID()
    let calculator: Calculator
    let kind: Kind
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
    /// Profil is not a tab: it opens as a sheet from the toolbar of every module.
    var isProfilePresented = false
    var repository: any SimRepository
    var user: UserDto?
    var vehicles: [VehicleDto] = []
    var environments: [EnvironmentDto] = []
    var isLoading = false

    /// Shown as a non-modal banner; cleared automatically after a few seconds.
    var errorMessage: String? {
        didSet {
            guard errorMessage != oldValue else { return }
            errorDismissTask?.cancel()
            guard let message = errorMessage else { return }
            AccessibilityNotification.Announcement("\(String(localized: "Fehler")): \(message)").post()
            errorDismissTask = Task { [weak self] in
                try? await Task.sleep(for: .seconds(6))
                guard !Task.isCancelled, let self, self.errorMessage == message else { return }
                self.errorMessage = nil
            }
        }
    }

    @ObservationIgnored private var errorDismissTask: Task<Void, Never>?

    var saveFeedback: SaveFeedback? {
        didSet {
            feedbackDismissTask?.cancel()
            guard let feedback = saveFeedback else { return }
            feedbackDismissTask = Task { [weak self] in
                try? await Task.sleep(for: .seconds(8))
                guard !Task.isCancelled, let self, self.saveFeedback?.id == feedback.id else { return }
                self.saveFeedback = nil
            }
        }
    }

    @ObservationIgnored private var feedbackDismissTask: Task<Void, Never>?

    /// Calculator inputs per "vehicleId:calculator" → field → value (`nil` = cleared by the user).
    var drafts: [String: [String: Double?]] {
        didSet {
            if let data = try? JSONEncoder().encode(drafts) { defaults.set(data, forKey: Keys.drafts) }
        }
    }

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
        static let drafts = "calculatorDrafts.v1"
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
        if let data = defaults.data(forKey: Keys.drafts),
           let stored = try? JSONDecoder().decode([String: [String: Double?]].self, from: data) {
            drafts = stored
        } else {
            drafts = [:]
        }
        if client.isAuthenticated() {
            repository = APIRepository(client: client)
        }
    }

    /// `--uitesting` runs fully offline with in-memory storage and a fresh selection
    /// (`--uitesting-light` additionally starts in the light appearance).
    static func makeDefault() -> AppModel {
        let arguments = ProcessInfo.processInfo.arguments
        if arguments.contains("--uitesting") {
            let suite = "uitesting"
            let defaults = UserDefaults(suiteName: suite)!
            defaults.removePersistentDomain(forName: suite)
            let model = AppModel(
                client: APIClient(tokenStore: InMemoryTokenStore()),
                localRepository: LocalRepository(fileURL: nil),
                defaults: defaults,
                isUITesting: true
            )
            if arguments.contains("--uitesting-light") { model.preferences.theme = .light }
            if arguments.contains("--uitesting-error") { model.errorMessage = "UI-Test-Fehler" }
            return model
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

    /// Saves (partial) motor values for the selected vehicle. Starter vehicles are duplicated first;
    /// the copy takes over the calculator drafts and becomes the selection. The result is shown as
    /// inline feedback in the calculator (`saveFeedback`).
    func saveToSelectedVehicle(_ input: VehicleInputDto, from calculator: Calculator) async {
        guard let vehicle = selectedVehicle else { return }
        var targetId = vehicle.id
        var copyName: String?
        if vehicle.isSystem {
            guard let copy = await perform({ try await $0.duplicateVehicle(id: vehicle.id, name: nil) }) else { return }
            targetId = copy.id
            copyName = copy.name
            copyDrafts(from: vehicle.id, to: copy.id)
        }
        guard await perform({ try await $0.updateVehicle(id: targetId, input) }) != nil else { return }
        selectedVehicleId = targetId
        let kind: SaveFeedback.Kind = if let copyName {
            .copied(name: copyName, copyId: targetId, starterId: vehicle.id)
        } else {
            .updated
        }
        saveFeedback = SaveFeedback(calculator: calculator, kind: kind)
        let announcement = switch kind {
        case .updated: String(localized: "Im Fahrzeug gespeichert")
        case let .copied(name, _, _): String(localized: "Kopie ‚\(name)‘ angelegt und ausgewählt")
        }
        AccessibilityNotification.Announcement(announcement).post()
    }

    /// Undoes a starter save: deletes the copy and selects the starter vehicle again.
    func undoStarterCopy(_ feedback: SaveFeedback) async {
        guard case let .copied(_, copyId, starterId) = feedback.kind else { return }
        saveFeedback = nil
        if await perform({ try await $0.deleteVehicle(id: copyId) }) != nil {
            removeDrafts(of: copyId)
            selectedVehicleId = starterId
        }
    }

    // MARK: - Calculator drafts

    private func draftKey(_ calculator: Calculator, vehicleId: String? = nil) -> String {
        "\(vehicleId ?? selectedVehicleId ?? "-"):\(calculator.rawValue)"
    }

    /// `nil` = field not in the draft (use the default), `.some(nil)` = cleared by the user.
    func draftValue(_ calculator: Calculator, _ field: String) -> Double?? {
        guard let draft = drafts[draftKey(calculator)], let value = draft[field] else { return nil }
        return .some(value)
    }

    func setDraftValue(_ calculator: Calculator, _ field: String, _ value: Double?) {
        let key = draftKey(calculator)
        if let draft = drafts[key], let current = draft[field], current == value { return }
        drafts[key, default: [:]][field] = .some(value)
    }

    func hasDraft(_ calculator: Calculator) -> Bool {
        drafts[draftKey(calculator)] != nil
    }

    func markDraft(_ calculator: Calculator) {
        let key = draftKey(calculator)
        if drafts[key] == nil { drafts[key] = [:] }
    }

    func resetDraft(_ calculator: Calculator) {
        drafts[draftKey(calculator)] = nil
    }

    private func copyDrafts(from sourceId: String, to targetId: String) {
        for calculator in Calculator.allCases {
            if let draft = drafts[draftKey(calculator, vehicleId: sourceId)] {
                drafts[draftKey(calculator, vehicleId: targetId)] = draft
            }
        }
    }

    private func removeDrafts(of vehicleId: String) {
        drafts = drafts.filter { !$0.key.hasPrefix("\(vehicleId):") }
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
        saveFeedback = nil
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
            saveFeedback = nil
            drafts = drafts.filter { $0.key.hasPrefix("sys-") }
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
