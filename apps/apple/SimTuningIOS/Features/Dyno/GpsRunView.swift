import Charts
import CoreLocation
import SimTuningAPI
import SwiftUI

/// GPS run recorder based on `CLLocationUpdate.liveUpdates` (asks for when-in-use only).
/// Background updates are opt-in per run through a `CLBackgroundActivitySession`.
@MainActor
@Observable
final class LocationRecorder {
    var samples: [GpsSampleDto] = []
    var isRecording = false
    var lastSpeedKmh: Double?
    var authorizationDenied = false
    var startedAt: Date?
    private var task: Task<Void, Never>?
    private var serviceSession: CLServiceSession?
    private var backgroundSession: CLBackgroundActivitySession?

    func start(allowBackground: Bool) {
        guard !isRecording else { return }
        samples = []
        isRecording = true
        authorizationDenied = false
        startedAt = .now
        serviceSession = CLServiceSession(authorization: .whenInUse)
        if allowBackground { backgroundSession = CLBackgroundActivitySession() }
        let formatter = ISO8601DateFormatter()
        formatter.formatOptions = [.withInternetDateTime, .withFractionalSeconds]
        task = Task { [weak self] in
            do {
                for try await update in CLLocationUpdate.liveUpdates(.otherNavigation) {
                    guard let self, self.isRecording else { break }
                    if update.authorizationDenied || update.authorizationDeniedGlobally {
                        self.authorizationDenied = true
                    }
                    guard let location = update.location else { continue }
                    let speed = location.speed >= 0 ? location.speed : nil
                    self.lastSpeedKmh = speed.map { $0 * 3.6 }
                    guard self.samples.count < maxDynoPoints else { continue }
                    self.samples.append(GpsSampleDto(
                        latitude: location.coordinate.latitude,
                        longitude: location.coordinate.longitude,
                        speed: speed,
                        accuracy: location.horizontalAccuracy >= 0 ? location.horizontalAccuracy : nil,
                        altitude: location.altitude,
                        altitudeAccuracy: location.verticalAccuracy >= 0 ? location.verticalAccuracy : nil,
                        heading: location.course >= 0 ? location.course : nil,
                        headingAccuracy: location.courseAccuracy >= 0 ? location.courseAccuracy : nil,
                        zeitpunkt: formatter.string(from: location.timestamp)
                    ))
                }
            } catch {
                // Stream ended (cancelled or location services unavailable).
            }
        }
    }

    func stop() {
        isRecording = false
        task?.cancel()
        task = nil
        backgroundSession?.invalidate()
        backgroundSession = nil
        serviceSession?.invalidate()
        serviceSession = nil
    }
}

struct GpsRunView: View {
    @Environment(AppModel.self) private var model
    @Environment(\.scenePhase) private var scenePhase
    let dyno: DynoDto?
    let ensureDyno: () async -> DynoDto?
    let onSaved: () async -> Void

    enum Mode: String, CaseIterable, Identifiable {
        case geschwindigkeit, ausrollen
        var id: String { rawValue }
        var title: LocalizedStringKey {
            switch self {
            case .geschwindigkeit: "Beschleunigung"
            case .ausrollen: "Ausrollen"
            }
        }
    }

    @State private var recorder = LocationRecorder()
    @State private var mode = Mode.geschwindigkeit
    @State private var allowBackground = false
    @State private var stoppedInBackground = false
    @State private var isSaving = false
    @State private var savedPoints: Int?
    @State private var confirmReplace = false
    @State private var confirmDiscard = false

    private var stored: [GpsSampleDto] {
        guard let dyno else { return [] }
        return mode == .geschwindigkeit ? dyno.geschwindigkeit : dyno.ausrollen
    }

    var body: some View {
        SheetForm(module: .dyno, subsheet: 2, subtitle: "GPS-Messfahrt") {
            Section {
                Picker("Messung", selection: $mode) {
                    ForEach(Mode.allCases) { Text($0.title).tag($0) }
                }
                .pickerStyle(.segmented)
                .disabled(recorder.isRecording)
                Toggle("Bei gesperrtem Bildschirm weiter aufzeichnen", isOn: $allowBackground)
                    .disabled(recorder.isRecording)
            } footer: {
                Text("Standort wird nur während der Messfahrt verwendet. Hintergrundaufzeichnung nur, wenn du sie hier aktivierst.")
            }
            Section {
                if recorder.isRecording {
                    if let startedAt = recorder.startedAt {
                        TimelineView(.periodic(from: startedAt, by: 1)) { context in
                            Text(verbatim: elapsedText(context.date.timeIntervalSince(startedAt)))
                                .font(.stData(.largeTitle, weight: .semibold))
                                .monospacedDigit()
                                .frame(maxWidth: .infinity)
                        }
                        .accessibilityLabel(Text("Dauer"))
                    }
                    LabeledContent("Geschwindigkeit") {
                        Text(verbatim: model.format(recorder.lastSpeedKmh, unit: "km/h"))
                            .font(.stData(.title2, weight: .semibold))
                    }
                    LabeledContent("Messpunkte") { Text(verbatim: "\(recorder.samples.count)") }
                }
                RecordButton(isRecording: recorder.isRecording, startTitle: "Messfahrt starten",
                             stopTitle: "Messfahrt beenden", startImage: "location.fill") {
                    if recorder.isRecording {
                        stop()
                    } else if !recorder.samples.isEmpty {
                        confirmDiscard = true
                    } else {
                        start()
                    }
                }
                if stoppedInBackground {
                    Label("Messfahrt beendet, weil die App in den Hintergrund gewechselt ist. Die bisherigen Messpunkte bleiben erhalten.",
                          systemImage: "info.circle")
                        .foregroundStyle(Theme.warning)
                }
                if !recorder.isRecording, !recorder.samples.isEmpty {
                    LabeledContent("Messpunkte") { Text(verbatim: "\(recorder.samples.count)") }
                    Button {
                        if !stored.isEmpty {
                            confirmReplace = true
                        } else {
                            Task { await save() }
                        }
                    } label: {
                        if isSaving {
                            HStack { ProgressView(); Text("Speichere …") }
                        } else {
                            Label("Messung speichern", systemImage: "square.and.arrow.down")
                        }
                    }
                    .disabled(isSaving)
                }
                if recorder.authorizationDenied {
                    Text("Kein Standortzugriff. Bitte in den Einstellungen erlauben.").foregroundStyle(Theme.destructive)
                }
            } header: {
                Text("Aufzeichnung")
            } footer: {
                if let savedPoints {
                    Label("Messfahrt gespeichert (\(savedPoints) Messpunkte)", systemImage: "checkmark.circle.fill")
                        .foregroundStyle(Theme.success)
                } else {
                    Text("Der Bildschirm bleibt während der Messfahrt an.")
                }
            }
            let chartSamples = recorder.samples.isEmpty ? stored : recorder.samples
            if chartSamples.contains(where: { $0.speed != nil }) {
                Section("Geschwindigkeit") {
                    Chart {
                        ForEach(Array(DynoView.downsample(chartSamples, max: 400).enumerated()), id: \.offset) { index, sample in
                            if let speed = sample.speed {
                                LineMark(x: .value("Messpunkt", index), y: .value("km/h", speed * 3.6))
                                    .foregroundStyle(Theme.dyno)
                            }
                        }
                    }
                    .stChartStyle()
                    .frame(height: 200)
                }
            }
        }
        .navigationTitle("GPS-Messfahrt")
        .confirmationDialog("Vorhandene Daten ersetzen?", isPresented: $confirmReplace, titleVisibility: .visible) {
            Button("Ersetzen", role: .destructive) { Task { await save() } }
        } message: {
            Text("Die gespeicherte Messfahrt dieser Messart wird ersetzt.")
        }
        .confirmationDialog("Messfahrt verwerfen?", isPresented: $confirmDiscard, titleVisibility: .visible) {
            Button("Verwerfen und neu starten", role: .destructive) { start() }
        } message: {
            Text("Die letzte Messfahrt wurde noch nicht gespeichert.")
        }
        .sensoryFeedback(trigger: recorder.isRecording) { _, recording in recording ? .start : .stop }
        .sensoryFeedback(.success, trigger: savedPoints) { _, new in new != nil }
        .onChange(of: scenePhase) { _, phase in
            // Without a background session iOS pauses the updates: end the run cleanly instead.
            if phase == .background, recorder.isRecording, !allowBackground {
                stop()
                stoppedInBackground = true
            }
        }
        .onDisappear {
            recorder.stop()
            UIApplication.shared.isIdleTimerDisabled = false
        }
    }

    private func start() {
        stoppedInBackground = false
        savedPoints = nil
        recorder.start(allowBackground: allowBackground)
        UIApplication.shared.isIdleTimerDisabled = true
    }

    private func stop() {
        recorder.stop()
        UIApplication.shared.isIdleTimerDisabled = false
    }

    private func save() async {
        guard !isSaving else { return }
        isSaving = true
        defer { isSaving = false }
        let samples = recorder.samples
        let data = mode == .geschwindigkeit
            ? DynoDataInputDto(geschwindigkeit: samples)
            : DynoDataInputDto(ausrollen: samples)
        guard let target = await ensureDyno() else { return }
        if await model.perform({ try await $0.saveDynoData(dynoId: target.id, data) }) != nil {
            recorder.samples = []
            stoppedInBackground = false
            savedPoints = samples.count
            await onSaved()
        }
    }
}
