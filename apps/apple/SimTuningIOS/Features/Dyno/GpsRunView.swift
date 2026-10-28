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
    private var task: Task<Void, Never>?
    private var serviceSession: CLServiceSession?
    private var backgroundSession: CLBackgroundActivitySession?

    func start(allowBackground: Bool) {
        guard !isRecording else { return }
        samples = []
        isRecording = true
        authorizationDenied = false
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
    let dyno: DynoDto
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

    private var stored: [GpsSampleDto] {
        mode == .geschwindigkeit ? dyno.geschwindigkeit : dyno.ausrollen
    }

    var body: some View {
        Form {
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
            Section("Aufzeichnung") {
                if recorder.isRecording {
                    LabeledContent("Geschwindigkeit") {
                        Text(verbatim: model.format(recorder.lastSpeedKmh, unit: "km/h"))
                            .font(.title2.monospacedDigit())
                    }
                    LabeledContent("Messpunkte") { Text(verbatim: "\(recorder.samples.count)") }
                    Button("Messfahrt beenden", systemImage: "stop.circle.fill", role: .destructive) { recorder.stop() }
                } else {
                    Button("Messfahrt starten", systemImage: "location.circle") {
                        recorder.start(allowBackground: allowBackground)
                    }
                    if !recorder.samples.isEmpty {
                        LabeledContent("Messpunkte") { Text(verbatim: "\(recorder.samples.count)") }
                        Button("Messung speichern", systemImage: "square.and.arrow.down") { Task { await save() } }
                    }
                }
                if recorder.authorizationDenied {
                    Text("Kein Standortzugriff. Bitte in den Einstellungen erlauben.").foregroundStyle(.red)
                }
            }
            let chartSamples = recorder.samples.isEmpty ? stored : recorder.samples
            if chartSamples.contains(where: { $0.speed != nil }) {
                Section("Geschwindigkeit") {
                    Chart {
                        ForEach(Array(DynoView.downsample(chartSamples, max: 400).enumerated()), id: \.offset) { index, sample in
                            if let speed = sample.speed {
                                LineMark(x: .value("Messpunkt", index), y: .value("km/h", speed * 3.6))
                            }
                        }
                    }
                    .frame(height: 200)
                }
            }
        }
        .navigationTitle("GPS-Messfahrt")
        .onDisappear { recorder.stop() }
    }

    private func save() async {
        let data = mode == .geschwindigkeit
            ? DynoDataInputDto(geschwindigkeit: recorder.samples)
            : DynoDataInputDto(ausrollen: recorder.samples)
        if await model.perform({ try await $0.saveDynoData(dynoId: dyno.id, data) }) != nil {
            recorder.samples = []
            await onSaved()
        }
    }
}
