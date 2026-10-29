import Accelerate
import AVFoundation
import Charts
import SimTuningAPI
import SimTuningLogic
import SwiftUI
import UniformTypeIdentifiers

/// Thread-safe sample sink for the AVAudioEngine tap (runs on the realtime audio thread).
final class SampleBuffer: @unchecked Sendable {
    private let lock = NSLock()
    private var samples: [Float] = []
    private var rms: Float = 0
    private var peak: Float = 0
    /// Hard cap (~10 min at 48 kHz) so a forgotten recording cannot exhaust memory.
    private let limit = 48000 * 600

    func append(_ pointer: UnsafePointer<Float>, count: Int, rms bufferRms: Float, peak bufferPeak: Float) {
        lock.lock()
        defer { lock.unlock() }
        rms = bufferRms
        peak = max(peak, bufferPeak)
        guard samples.count < limit else { return }
        samples.append(contentsOf: UnsafeBufferPointer(start: pointer, count: min(count, limit - samples.count)))
    }

    func drain() -> [Float] {
        lock.lock()
        defer { lock.unlock() }
        let out = samples
        samples = []
        rms = 0
        peak = 0
        return out
    }

    /// Latest buffer RMS and the peak since the previous call (peak hold between UI refreshes).
    func readLevel() -> InputLevel {
        lock.lock()
        defer { lock.unlock() }
        let level = InputLevel(rms: rms, peak: peak)
        peak = 0
        return level
    }

    var count: Int {
        lock.lock()
        defer { lock.unlock() }
        return samples.count
    }

    var isFull: Bool { count >= limit }
}

/// Microphone input level of one UI refresh (linear full scale, 0…1).
struct InputLevel: Equatable, Sendable {
    var rms: Float = 0
    var peak: Float = 0

    enum Rating { case low, good, clipping }

    var decibels: Float { rms > 0 ? 20 * log10(rms) : -120 }
    /// -60 dBFS … 0 dBFS mapped to 0 … 1 for the meter bar.
    var fraction: Double { Double(min(max((decibels + 60) / 60, 0), 1)) }
    var rating: Rating {
        if peak >= 0.98 { return .clipping }
        return decibels < -45 ? .low : .good
    }
}

/// Microphone recording via AVAudioEngine (mono, input sample rate).
@MainActor
final class AudioRecorder {
    private let engine = AVAudioEngine()
    private let buffer = SampleBuffer()
    private(set) var sampleRate: Double = 44100
    private(set) var isRecording = false

    func start() async throws {
        guard await AVAudioApplication.requestRecordPermission() else { throw RecorderError.permissionDenied }
        let session = AVAudioSession.sharedInstance()
        try session.setCategory(.record, mode: .measurement)
        try session.setActive(true)
        let input = engine.inputNode
        let format = input.outputFormat(forBus: 0)
        sampleRate = format.sampleRate
        _ = buffer.drain()
        input.installTap(onBus: 0, bufferSize: 4096, format: format, block: Self.tap(into: buffer))
        engine.prepare()
        try engine.start()
        isRecording = true
    }

    /// Built outside the main actor: the block runs on the realtime audio thread and must not
    /// inherit main-actor isolation.
    private nonisolated static func tap(into sink: SampleBuffer) -> AVAudioNodeTapBlock {
        { pcm, _ in
            guard let channel = pcm.floatChannelData?[0] else { return }
            let count = Int(pcm.frameLength)
            var rms: Float = 0
            var peak: Float = 0
            vDSP_rmsqv(channel, 1, &rms, vDSP_Length(count))
            vDSP_maxmgv(channel, 1, &peak, vDSP_Length(count))
            sink.append(channel, count: count, rms: rms, peak: peak)
        }
    }

    func stop() -> PcmAudio {
        engine.inputNode.removeTap(onBus: 0)
        engine.stop()
        try? AVAudioSession.sharedInstance().setActive(false, options: .notifyOthersOnDeactivation)
        isRecording = false
        return PcmAudio(sampleRate: Int(sampleRate), samples: Wav.floatToPcm16(buffer.drain()))
    }

    func readLevel() -> InputLevel { buffer.readLevel() }
    var recordedSeconds: Double { Double(buffer.count) / max(sampleRate, 1) }
    var isFull: Bool { buffer.isFull }
    /// False when the system stopped the engine (e.g. a phone call interrupted the session).
    var isEngineRunning: Bool { engine.isRunning }

    enum RecorderError: LocalizedError {
        case permissionDenied
        var errorDescription: String? {
            String(localized: "Kein Mikrofonzugriff. Bitte in den Einstellungen erlauben.")
        }
    }
}

/// `mm:ss` for recording timers.
func elapsedText(_ seconds: Double) -> String {
    let total = max(Int(seconds), 0)
    return String(format: "%02d:%02d", total / 60, total % 60)
}

/// Large start/stop control for use with gloves on a mounted phone.
struct RecordButton: View {
    let isRecording: Bool
    let startTitle: LocalizedStringKey
    let stopTitle: LocalizedStringKey
    let startImage: String
    let action: () -> Void

    var body: some View {
        Button(role: isRecording ? .destructive : nil, action: action) {
            Label(isRecording ? stopTitle : startTitle, systemImage: isRecording ? "stop.fill" : startImage)
                .font(.stHeading(.title3))
                .frame(maxWidth: .infinity, minHeight: 56)
        }
        .buttonStyle(.borderedProminent)
        .controlSize(.large)
        .tint(isRecording ? Theme.destructive : Theme.dyno)
        .foregroundStyle(Theme.primaryForeground)
    }
}

/// Horizontal input level bar with a text rating (not colour alone).
struct LevelMeter: View {
    let level: InputLevel
    @Environment(\.accessibilityReduceMotion) private var reduceMotion

    private var color: Color {
        switch level.rating {
        case .low: Theme.mutedForeground
        case .good: Theme.success
        case .clipping: Theme.destructive
        }
    }

    private var ratingText: LocalizedStringKey {
        switch level.rating {
        case .low: "Pegel niedrig"
        case .good: "Pegel gut"
        case .clipping: "Pegel übersteuert"
        }
    }

    var body: some View {
        VStack(alignment: .leading, spacing: Theme.Spacing.xs) {
            GeometryReader { proxy in
                ZStack(alignment: .leading) {
                    RoundedRectangle(cornerRadius: Theme.Radius.standard).fill(Theme.raised)
                    RoundedRectangle(cornerRadius: Theme.Radius.standard).fill(color)
                        .frame(width: proxy.size.width * level.fraction)
                }
            }
            .frame(height: 14)
            .animation(reduceMotion ? nil : .linear(duration: 0.08), value: level.fraction)
            Text(ratingText).font(.footnote).foregroundStyle(level.rating == .good ? Theme.foreground : color)
        }
        .accessibilityElement(children: .ignore)
        .accessibilityLabel(Text("Eingangspegel"))
        .accessibilityValue(Text(ratingText))
    }
}

enum AudioFileLoader {
    static let maxBytes = 200 * 1024 * 1024

    /// 16-bit PCM WAV is read exactly like v2; other formats are decoded with AVAudioFile.
    static func load(_ url: URL) throws -> PcmAudio {
        let size = try url.resourceValues(forKeys: [.fileSizeKey]).fileSize ?? 0
        guard size <= maxBytes else { throw CocoaError(.fileReadTooLarge) }
        if url.pathExtension.lowercased() == "wav", let wav = try? Wav.readMono(Data(contentsOf: url)) {
            return wav
        }
        let file = try AVAudioFile(forReading: url)
        let format = file.processingFormat
        guard let pcm = AVAudioPCMBuffer(pcmFormat: format, frameCapacity: AVAudioFrameCount(file.length)) else {
            throw CocoaError(.fileReadCorruptFile)
        }
        try file.read(into: pcm)
        guard let channel = pcm.floatChannelData?[0] else { throw CocoaError(.fileReadCorruptFile) }
        let floats = Array(UnsafeBufferPointer(start: channel, count: Int(pcm.frameLength)))
        return PcmAudio(sampleRate: Int(format.sampleRate), samples: Wav.floatToPcm16(floats))
    }
}

/// v2 `DynoAudioViewModel`: record or import, analyse on device (off the main actor), pick a
/// cluster and store its fitted RPM curve as the dyno's `drehzahl` series.
struct AudioDynoView: View {
    @Environment(AppModel.self) private var model
    @Environment(\.scenePhase) private var scenePhase
    let dyno: DynoDto?
    let ensureDyno: () async -> DynoDto?
    let onSaved: () async -> Void

    enum Interruption { case background, system, limit }

    @State private var recorder = AudioRecorder()
    @State private var isRecording = false
    @State private var audio: PcmAudio?
    @State private var options = AudioDyno.Options()
    @State private var analysis: AudioDyno.Analysis?
    @State private var isAnalysing = false
    @State private var selectedCluster: Int?
    @State private var importing = false
    @State private var elapsed: Double = 0
    @State private var level = InputLevel()
    @State private var lowLevelSince: Date?
    @State private var interruption: Interruption?
    @State private var isSaving = false
    @State private var savedPoints: Int?
    @State private var confirmReplace = false
    @State private var confirmDiscard = false

    private var levelTooLow: Bool {
        guard let lowLevelSince else { return false }
        return Date.now.timeIntervalSince(lowLevelSince) >= 3
    }

    var body: some View {
        SheetForm(module: .dyno, subsheet: 1, subtitle: "Audio-Dyno") {
            Section {
                if isRecording {
                    Text(verbatim: elapsedText(elapsed))
                        .font(.stData(.largeTitle, weight: .semibold))
                        .monospacedDigit()
                        .frame(maxWidth: .infinity)
                        .accessibilityLabel(Text("Aufnahmedauer"))
                    LevelMeter(level: level)
                    if levelTooLow {
                        Label("Sehr leises Signal. Prüfe, ob das Mikrofon frei ist (Hülle, Halterung).",
                              systemImage: "exclamationmark.triangle")
                            .foregroundStyle(Theme.warning)
                    }
                }
                RecordButton(isRecording: isRecording, startTitle: "Aufnahme starten",
                             stopTitle: "Aufnahme beenden", startImage: "mic.fill") {
                    if isRecording {
                        stopRecording()
                    } else if audio != nil, savedPoints == nil {
                        confirmDiscard = true
                    } else {
                        Task { await startRecording() }
                    }
                }
                if let interruption {
                    Label(interruptionText(interruption), systemImage: "info.circle")
                        .foregroundStyle(Theme.warning)
                }
                if !isRecording {
                    Button("Audiodatei importieren", systemImage: "doc.badge.plus") { importing = true }
                }
                if let audio, !isRecording {
                    LabeledContent("Dauer") {
                        Text(verbatim: model.format(Double(audio.samples.count) / Double(max(audio.sampleRate, 1)), unit: "s"))
                    }
                    LabeledContent("Abtastrate") { Text(verbatim: "\(audio.sampleRate) Hz") }
                }
            } header: {
                Text("Aufnahme")
            } footer: {
                Text("Der Bildschirm bleibt während der Aufnahme an. Die Aufnahme wird nur auf diesem Gerät ausgewertet.")
            }
            Section("Analyse") {
                Picker("FFT-Größe", selection: $options.fftSize) {
                    ForEach(AudioDyno.fftSizes, id: \.self) { Text(verbatim: "\($0)").tag($0) }
                }
                NumberField("Drehzahl von", value: optionalBinding(\.frequenzbeginn), unit: "U/min")
                NumberField("Drehzahl bis", value: optionalBinding(\.frequenzende), unit: "U/min")
                NumberField("Intensität", value: optionalBinding(\.intensity), unit: "%")
                NumberField("Epsilon", value: optionalBinding(\.epsilon))
                Button {
                    Task { await analyse() }
                } label: {
                    if isAnalysing {
                        HStack { ProgressView(); Text("Analysiere …") }
                    } else {
                        Label("Analysieren", systemImage: "waveform.badge.magnifyingglass")
                    }
                }
                .disabled(audio == nil || isAnalysing || isRecording)
            }
            if let analysis {
                Section {
                    Chart {
                        ForEach(Array(analysis.clusters.enumerated()), id: \.offset) { index, cluster in
                            ForEach(Array(DynoView.downsample(cluster, max: 300).enumerated()), id: \.offset) { _, point in
                                PointMark(x: .value("Zeit (ms)", point.x), y: .value("U/min", point.y))
                                    .symbolSize(index == selectedCluster ? 20 : 6)
                                    .foregroundStyle(by: .value("Graph", "Graph \(index + 1)"))
                            }
                        }
                    }
                    .chartForegroundStyleScale(range: Theme.series)
                    .stChartStyle()
                    .frame(height: 240)
                    Picker("Graph", selection: $selectedCluster) {
                        Text("Keiner").tag(Int?.none)
                        ForEach(Array(analysis.clusters.enumerated()), id: \.offset) { index, cluster in
                            Text(verbatim: "Graph \(index + 1) (\(cluster.count))").tag(Int?.some(index))
                        }
                    }
                    Button {
                        if let dyno, !dyno.drehzahl.isEmpty {
                            confirmReplace = true
                        } else {
                            Task { await saveCurve() }
                        }
                    } label: {
                        if isSaving {
                            HStack { ProgressView(); Text("Speichere …") }
                        } else {
                            Label("Drehzahlkurve übernehmen", systemImage: "checkmark.circle")
                        }
                    }
                    .disabled(selectedCluster == nil || isSaving)
                } header: {
                    Text("Ergebnis")
                } footer: {
                    VStack(alignment: .leading, spacing: Theme.Spacing.xs) {
                        Text("\(analysis.points.count) Punkte in \(analysis.clusters.count) Graphen gefunden.")
                        if let savedPoints {
                            Label("Drehzahlkurve gespeichert (\(savedPoints) Punkte)", systemImage: "checkmark.circle.fill")
                                .foregroundStyle(Theme.success)
                        }
                    }
                }
            }
        }
        .navigationTitle("Audio-Dyno")
        .fileImporter(isPresented: $importing, allowedContentTypes: [.audio, .wav]) { result in
            Task { await importFile(result) }
        }
        .confirmationDialog("Vorhandene Daten ersetzen?", isPresented: $confirmReplace, titleVisibility: .visible) {
            Button("Ersetzen", role: .destructive) { Task { await saveCurve() } }
        } message: {
            Text("Die gespeicherte Drehzahlkurve wird ersetzt.")
        }
        .confirmationDialog("Aufnahme verwerfen?", isPresented: $confirmDiscard, titleVisibility: .visible) {
            Button("Verwerfen und neu aufnehmen", role: .destructive) { Task { await startRecording() } }
        } message: {
            Text("Die aktuelle Aufnahme wurde noch nicht als Drehzahlkurve gespeichert.")
        }
        .sensoryFeedback(trigger: isRecording) { _, recording in recording ? .start : .stop }
        .sensoryFeedback(.success, trigger: savedPoints) { _, new in new != nil }
        .task(id: isRecording) { await monitorRecording() }
        .onChange(of: scenePhase) { _, phase in
            if phase == .background, isRecording { stopRecording(interruption: .background) }
        }
        .onDisappear {
            if isRecording { stopRecording() }
            UIApplication.shared.isIdleTimerDisabled = false
        }
    }

    private func interruptionText(_ interruption: Interruption) -> LocalizedStringKey {
        switch interruption {
        case .background: "Aufnahme beendet, weil die App in den Hintergrund gewechselt ist. Die bisher aufgenommenen Daten bleiben erhalten."
        case .system: "Aufnahme wurde vom System unterbrochen (z. B. Anruf). Die bisher aufgenommenen Daten bleiben erhalten."
        case .limit: "Maximale Aufnahmedauer erreicht. Die Aufnahme wurde beendet."
        }
    }

    /// Polls the tap's level and duration at ~15 Hz on the main actor; the audio thread only
    /// writes into the lock-protected buffer.
    private func monitorRecording() async {
        guard isRecording else { return }
        while !Task.isCancelled, isRecording {
            level = recorder.readLevel()
            elapsed = recorder.recordedSeconds
            if level.rating == .low {
                if lowLevelSince == nil { lowLevelSince = .now }
            } else {
                lowLevelSince = nil
            }
            if recorder.isFull {
                stopRecording(interruption: .limit)
            } else if !recorder.isEngineRunning {
                stopRecording(interruption: .system)
            }
            try? await Task.sleep(for: .milliseconds(66))
        }
    }

    private func optionalBinding(_ keyPath: WritableKeyPath<AudioDyno.Options, Double>) -> Binding<Double?> {
        Binding(get: { options[keyPath: keyPath] }, set: { if let v = $0 { options[keyPath: keyPath] = v } })
    }

    private func startRecording() async {
        do {
            interruption = nil
            elapsed = 0
            level = InputLevel()
            lowLevelSince = nil
            try await recorder.start()
            UIApplication.shared.isIdleTimerDisabled = true
            isRecording = true
        } catch {
            model.report(error)
        }
    }

    private func stopRecording(interruption: Interruption? = nil) {
        guard recorder.isRecording else { return }
        audio = recorder.stop()
        isRecording = false
        UIApplication.shared.isIdleTimerDisabled = false
        self.interruption = interruption
        lowLevelSince = nil
        analysis = nil
        savedPoints = nil
    }

    private func importFile(_ result: Result<URL, Error>) async {
        do {
            let url = try result.get()
            let scoped = url.startAccessingSecurityScopedResource()
            defer { if scoped { url.stopAccessingSecurityScopedResource() } }
            audio = try AudioFileLoader.load(url)
            analysis = nil
            savedPoints = nil
            interruption = nil
        } catch {
            model.report(error)
        }
    }

    /// The FFT pipeline is CPU heavy: run it detached from the main actor.
    private func analyse() async {
        guard let audio else { return }
        isAnalysing = true
        defer { isAnalysing = false }
        let options = options
        do {
            let result = try await Task.detached(priority: .userInitiated) {
                try AudioDyno.analyze(audio.samples, sampleRate: Double(audio.sampleRate), options: options)
            }.value
            analysis = result
            savedPoints = nil
            // Firing-frequency track, not the largest cluster: harmonics are often louder.
            selectedCluster = result.fundamental
        } catch {
            model.report(error)
        }
    }

    private func saveCurve() async {
        guard let analysis, let selectedCluster, !isSaving else { return }
        isSaving = true
        defer { isSaving = false }
        savedPoints = nil
        let cluster = analysis.clusters[selectedCluster]
        let curve = await Task.detached { AudioDyno.fitDrehzahlCurve(cluster) }.value
        let points = Array(curve.prefix(maxDynoPoints).map { DrehzahlPointDto(zeit: $0.x, drehzahl: $0.y) })
        guard let target = await ensureDyno() else { return }
        if await model.perform({ try await $0.saveDynoData(dynoId: target.id, DynoDataInputDto(drehzahl: points)) }) != nil {
            savedPoints = points.count
        }
        await onSaved()
    }
}
