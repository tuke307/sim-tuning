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
    /// Hard cap (~10 min at 48 kHz) so a forgotten recording cannot exhaust memory.
    private let limit = 48000 * 600

    func append(_ pointer: UnsafePointer<Float>, count: Int) {
        lock.lock()
        defer { lock.unlock() }
        guard samples.count < limit else { return }
        samples.append(contentsOf: UnsafeBufferPointer(start: pointer, count: min(count, limit - samples.count)))
    }

    func drain() -> [Float] {
        lock.lock()
        defer { lock.unlock() }
        let out = samples
        samples = []
        return out
    }

    var count: Int {
        lock.lock()
        defer { lock.unlock() }
        return samples.count
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
        let sink = buffer
        input.installTap(onBus: 0, bufferSize: 4096, format: format) { pcm, _ in
            guard let channel = pcm.floatChannelData?[0] else { return }
            sink.append(channel, count: Int(pcm.frameLength))
        }
        engine.prepare()
        try engine.start()
        isRecording = true
    }

    func stop() -> PcmAudio {
        engine.inputNode.removeTap(onBus: 0)
        engine.stop()
        try? AVAudioSession.sharedInstance().setActive(false, options: .notifyOthersOnDeactivation)
        isRecording = false
        return PcmAudio(sampleRate: Int(sampleRate), samples: Wav.floatToPcm16(buffer.drain()))
    }

    enum RecorderError: LocalizedError {
        case permissionDenied
        var errorDescription: String? {
            String(localized: "Kein Mikrofonzugriff. Bitte in den Einstellungen erlauben.")
        }
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
    let dyno: DynoDto
    let onSaved: () async -> Void

    @State private var recorder = AudioRecorder()
    @State private var isRecording = false
    @State private var audio: PcmAudio?
    @State private var options = AudioDyno.Options()
    @State private var analysis: AudioDyno.Analysis?
    @State private var isAnalysing = false
    @State private var selectedCluster: Int?
    @State private var importing = false

    var body: some View {
        SheetForm(module: .dyno, subsheet: 1, subtitle: "Audio-Dyno") {
            Section("Aufnahme") {
                if isRecording {
                    Button("Aufnahme beenden", systemImage: "stop.circle.fill", role: .destructive) { stopRecording() }
                } else {
                    Button("Aufnahme starten", systemImage: "mic.circle") { Task { await startRecording() } }
                }
                Button("Audiodatei importieren", systemImage: "doc.badge.plus") { importing = true }
                if let audio {
                    LabeledContent("Dauer") {
                        Text(verbatim: model.format(Double(audio.samples.count) / Double(max(audio.sampleRate, 1)), unit: "s"))
                    }
                    LabeledContent("Abtastrate") { Text(verbatim: "\(audio.sampleRate) Hz") }
                }
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
                .disabled(audio == nil || isAnalysing)
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
                    Button("Drehzahlkurve übernehmen", systemImage: "checkmark.circle") {
                        Task { await saveCurve() }
                    }
                    .disabled(selectedCluster == nil)
                } header: {
                    Text("Ergebnis")
                } footer: {
                    Text("\(analysis.points.count) Punkte in \(analysis.clusters.count) Graphen gefunden.")
                }
            }
        }
        .navigationTitle("Audio-Dyno")
        .fileImporter(isPresented: $importing, allowedContentTypes: [.audio, .wav]) { result in
            Task { await importFile(result) }
        }
        .onDisappear { if isRecording { stopRecording() } }
    }

    private func optionalBinding(_ keyPath: WritableKeyPath<AudioDyno.Options, Double>) -> Binding<Double?> {
        Binding(get: { options[keyPath: keyPath] }, set: { if let v = $0 { options[keyPath: keyPath] = v } })
    }

    private func startRecording() async {
        do {
            try await recorder.start()
            isRecording = true
        } catch {
            model.report(error)
        }
    }

    private func stopRecording() {
        audio = recorder.stop()
        isRecording = false
        analysis = nil
    }

    private func importFile(_ result: Result<URL, Error>) async {
        do {
            let url = try result.get()
            let scoped = url.startAccessingSecurityScopedResource()
            defer { if scoped { url.stopAccessingSecurityScopedResource() } }
            audio = try AudioFileLoader.load(url)
            analysis = nil
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
            // Firing-frequency track, not the largest cluster: harmonics are often louder.
            selectedCluster = result.fundamental
        } catch {
            model.report(error)
        }
    }

    private func saveCurve() async {
        guard let analysis, let selectedCluster else { return }
        let cluster = analysis.clusters[selectedCluster]
        let curve = await Task.detached { AudioDyno.fitDrehzahlCurve(cluster) }.value
        let points = curve.prefix(maxDynoPoints).map { DrehzahlPointDto(zeit: $0.x, drehzahl: $0.y) }
        await model.perform { try await $0.saveDynoData(dynoId: dyno.id, DynoDataInputDto(drehzahl: Array(points))) }
        await onSaved()
    }
}
