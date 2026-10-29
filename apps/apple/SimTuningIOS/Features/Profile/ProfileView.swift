import SimTuningAPI
import SwiftUI

struct ProfileView: View {
    @Environment(AppModel.self) private var model
    @Environment(\.dismiss) private var dismiss
    @State private var email = ""
    @State private var password = ""
    @State private var displayName = ""
    @State private var isRegistering = false
    @State private var isBusy = false
    @State private var info: String?
    @State private var confirmReset = false

    var body: some View {
        @Bindable var model = model
        NavigationStack {
            List {
                Section {
                    VStack(spacing: Theme.Spacing.s) {
                        BrandLogo(size: 112)
                        Text(verbatim: "SimTuning")
                            .font(.stHeading(.title3, weight: .bold))
                            .foregroundStyle(Theme.foreground)
                    }
                    .frame(maxWidth: .infinity)
                    .padding(.vertical, Theme.Spacing.m)
                    .accessibilityElement(children: .combine)
                }
                .listRowBackground(Color.clear)
                Group {
                    if model.isSignedIn {
                        Section("Konto") {
                            LabeledContent("E-Mail") { Text(verbatim: model.user?.email ?? "–") }
                            if let name = model.user?.name { LabeledContent("Name") { Text(verbatim: name) } }
                            Button("Abmelden", systemImage: "rectangle.portrait.and.arrow.right", role: .destructive) {
                                Task { await model.signOut() }
                            }
                        }
                    } else {
                        Section {
                            Picker("Modus", selection: $isRegistering) {
                                Text("Anmelden").tag(false)
                                Text("Registrieren").tag(true)
                            }
                            .pickerStyle(.segmented)
                            TextField("E-Mail", text: $email)
                                .textContentType(.username)
                                .keyboardType(.emailAddress)
                                .textInputAutocapitalization(.never)
                                .autocorrectionDisabled()
                            SecureField("Passwort", text: $password)
                                .textContentType(isRegistering ? .newPassword : .password)
                            if isRegistering {
                                TextField("Name (optional)", text: $displayName)
                                    .textContentType(.name)
                            }
                            Button {
                                Task { await submit() }
                            } label: {
                                if isBusy { ProgressView() } else { Text(isRegistering ? "Registrieren" : "Anmelden") }
                            }
                            .disabled(!canSubmit || isBusy)
                        } header: {
                            Text("Konto")
                        } footer: {
                            if isRegistering {
                                Text("Mindestens 8 Zeichen, mit mindestens einem Buchstaben und einer Zahl.")
                            } else {
                                Text("Ohne Anmeldung arbeitest du im Gastmodus. Deine Daten bleiben auf diesem Gerät.")
                            }
                        }
                        if let info {
                            Section { Text(verbatim: info) }
                        }
                    }

                    Section("Darstellung") {
                        Picker("Sprache", selection: $model.preferences.language) {
                            Text("System").tag(LanguagePreference.system)
                            Text(verbatim: "Deutsch").tag(LanguagePreference.de)
                            Text(verbatim: "English").tag(LanguagePreference.en)
                        }
                        Picker("Design", selection: $model.preferences.theme) {
                            Text("System").tag(ThemePreference.system)
                            Text("Hell").tag(ThemePreference.light)
                            Text("Dunkel").tag(ThemePreference.dark)
                        }
                    }
                    Section {
                        Stepper(value: $model.preferences.roundingAccuracy,
                                in: RoundingAccuracy.min...RoundingAccuracy.max) {
                            LabeledContent("Nachkommastellen") {
                                Text(verbatim: "\(model.preferences.roundingAccuracy)")
                            }
                        }
                        Toggle("Ergebnisse runden", isOn: $model.preferences.roundOnUnitChange)
                    } header: {
                        Text("Einheiten")
                    }

                    if !model.isSignedIn {
                        Section {
                            Button("Gastdaten zurücksetzen", systemImage: "trash", role: .destructive) { confirmReset = true }
                        }
                    }

                    Section {
                        LabeledContent("Version") {
                            Text(verbatim: Bundle.main.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String ?? "–")
                        }
                    }
                }
                .stRows()
            }
            .stScreen()
            .navigationTitle("Profil")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .confirmationAction) {
                    Button("Fertig") { dismiss() }
                        .accessibilityIdentifier("profile.done")
                }
            }
            .errorBanner()
            .onChange(of: model.preferences) { _, _ in Task { await model.syncPreferences() } }
            .confirmationDialog("Gastdaten zurücksetzen?", isPresented: $confirmReset, titleVisibility: .visible) {
                Button("Zurücksetzen", role: .destructive) { Task { await model.resetGuestData() } }
            } message: {
                Text("Alle eigenen Fahrzeuge, Umgebungen und Dynos auf diesem Gerät werden gelöscht.")
            }
        }
    }

    private var canSubmit: Bool {
        let trimmed = email.trimmingCharacters(in: .whitespaces)
        guard trimmed.contains("@"), trimmed.count >= 3 else { return false }
        return isRegistering ? PasswordRules.isValid(password) : !password.isEmpty
    }

    private func submit() async {
        isBusy = true
        defer { isBusy = false }
        let trimmed = email.trimmingCharacters(in: .whitespaces)
        if isRegistering {
            let name = displayName.trimmingCharacters(in: .whitespaces)
            info = await model.register(email: trimmed, password: password, name: name.isEmpty ? nil : name)
        } else if await model.signIn(email: trimmed, password: password) {
            info = nil
        }
        password = ""
    }
}
