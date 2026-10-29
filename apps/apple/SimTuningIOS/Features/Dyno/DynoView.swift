import Charts
import SimTuningAPI
import SimTuningLogic
import SwiftUI
import UniformTypeIdentifiers

/// Dyno hub: one dyno per vehicle (v2 1:1) with measurement data, audio dyno, GPS runs and the
/// power diagnosis.
struct DynoView: View {
    @Environment(AppModel.self) private var model
    @State private var dyno: DynoDto?
    @State private var name = ""
    @State private var exportDocument: JSONFileDocument?
    @State private var importing = false
    @State private var confirmDelete = false
    @FocusState private var nameFocused: Bool

    var body: some View {
        NavigationStack {
            SheetList(module: .dyno) {
                if let vehicle = model.selectedVehicle {
                    if vehicle.isSystem {
                        Section {
                            Text("Startfahrzeuge sind schreibgeschützt. Dupliziere das Fahrzeug, um einen Dyno anzulegen.")
                                .foregroundStyle(Theme.mutedForeground)
                            Button("Fahrzeug duplizieren", systemImage: "plus.square.on.square") {
                                Task {
                                    if let copy = await model.perform({ try await $0.duplicateVehicle(id: vehicle.id, name: nil) }) {
                                        model.selectedVehicleId = copy.id
                                    }
                                }
                            }
                        }
                    } else {
                        dynoSections(vehicle)
                    }
                } else {
                    ContentUnavailableView("Kein Fahrzeug ausgewählt", systemImage: "car.side",
                                           description: Text("Wähle im Tab Fahrzeuge ein Fahrzeug aus."))
                }
                Section {
                    Button("Dyno importieren (JSON)", systemImage: "square.and.arrow.down") { importing = true }
                }
            }
            .moduleChrome("Dyno")
            .task(id: model.selectedVehicleId) { await load() }
            .refreshable { await load() }
            .fileExporter(isPresented: Binding(get: { exportDocument != nil }, set: { if !$0 { exportDocument = nil } }),
                          document: exportDocument, contentType: .json,
                          defaultFilename: "\(dyno?.name ?? "dyno").json") { result in
                if case let .failure(error) = result { model.report(error) }
            }
            .fileImporter(isPresented: $importing, allowedContentTypes: [.json]) { result in
                Task { await importDyno(result) }
            }
            .confirmationDialog("Dyno löschen?", isPresented: $confirmDelete, titleVisibility: .visible) {
                Button("Löschen", role: .destructive) {
                    Task { await deleteDyno() }
                }
            } message: {
                Text("Alle Messdaten dieses Dynos werden dauerhaft gelöscht.")
            }
        }
    }

    /// Own vehicles go straight to the measurements; the dyno itself is created on the first save.
    @ViewBuilder
    private func dynoSections(_ vehicle: VehicleDto) -> some View {
        Section {
            TextField("Name", text: $name)
                .focused($nameFocused)
                .submitLabel(.done)
                .onSubmit { Task { await rename() } }
            Picker("Umgebung", selection: Binding(
                get: { dyno?.environmentId ?? "" },
                set: { id in Task { await save(DynoInputDto(name: currentName, environmentId: .some(id.isEmpty ? nil : id))) } }
            )) {
                Text("Keine").tag("")
                ForEach(model.environments) { env in
                    Text(verbatim: env.name).tag(env.id)
                }
            }
            if let dyno {
                LabeledContent("Drehzahlpunkte") { Text(verbatim: "\(dyno.drehzahl.count)") }
                LabeledContent("Leistungspunkte") { Text(verbatim: "\(dyno.dynoPs.count)") }
                LabeledContent("GPS (Beschleunigung / Ausrollen)") {
                    Text(verbatim: "\(dyno.geschwindigkeit.count) / \(dyno.ausrollen.count)")
                }
            }
        } header: {
            Text("Dyno")
        } footer: {
            if dyno == nil {
                Text("Der Dyno wird automatisch angelegt, sobald du die erste Messung speicherst.")
            }
        }
        Section("Messung") {
            NavigationLink {
                AudioDynoView(dyno: dyno, ensureDyno: { await ensureDyno() }) { await load() }
            } label: {
                Label("Audio-Dyno", systemImage: "waveform")
            }
            NavigationLink {
                GpsRunView(dyno: dyno, ensureDyno: { await ensureDyno() }) { await load() }
            } label: {
                Label("GPS-Messfahrt", systemImage: "location")
            }
            NavigationLink {
                DiagnosisView(dyno: dyno, ensureDyno: { await ensureDyno() }) { await load() }
            } label: {
                Label("Diagnose", systemImage: "chart.xyaxis.line")
            }
        }
        if let dyno, !dyno.drehzahl.isEmpty {
            Section("Drehzahlverlauf") {
                Chart {
                    ForEach(Array(Self.downsample(dyno.drehzahl, max: 400).enumerated()), id: \.offset) { _, point in
                        LineMark(x: .value("Zeit (ms)", point.zeit), y: .value("U/min", point.drehzahl))
                            .foregroundStyle(Theme.dyno)
                    }
                }
                .stChartStyle()
                .frame(height: 200)
                .accessibilityLabel(Text("Drehzahlverlauf"))
            }
        }
        if let dyno {
            Section {
                Button("Exportieren (JSON)", systemImage: "square.and.arrow.up") {
                    Task {
                        if let export = await model.perform({ try await $0.exportDyno(id: dyno.id) }) {
                            exportDocument = try? JSONFileDocument(export)
                        }
                    }
                }
                Button("Dyno löschen", systemImage: "trash", role: .destructive) { confirmDelete = true }
            }
        }
    }

    static func downsample<T>(_ values: [T], max: Int) -> [T] {
        guard values.count > max, max > 0 else { return values }
        let step = Double(values.count) / Double(max)
        return (0..<max).map { values[Int(Double($0) * step)] }
    }

    private func load() async {
        guard let vehicle = model.selectedVehicle, !vehicle.isSystem else {
            dyno = nil
            name = ""
            return
        }
        do {
            dyno = try await model.repository.getDyno(vehicleId: vehicle.id)
            if !nameFocused { name = dyno?.name ?? Self.defaultName(vehicle) }
        } catch {
            model.report(error)
        }
    }

    static func defaultName(_ vehicle: VehicleDto) -> String {
        String("\(vehicle.name) Dyno".prefix(120))
    }

    private var currentName: String {
        let trimmed = name.trimmingCharacters(in: .whitespacesAndNewlines)
        if !trimmed.isEmpty { return trimmed }
        return dyno?.name ?? model.selectedVehicle.map(Self.defaultName) ?? "Dyno"
    }

    /// Returns the vehicle's dyno, creating it with the entered (or default) name on first use.
    private func ensureDyno() async -> DynoDto? {
        if let dyno { return dyno }
        guard let vehicle = model.selectedVehicle, !vehicle.isSystem else { return nil }
        // `saveDyno` upserts per vehicle, so a concurrent second call cannot create a duplicate.
        let created = await model.perform { try await $0.saveDyno(vehicleId: vehicle.id, DynoInputDto(name: currentName)) }
        if let created { dyno = created }
        return created
    }

    private func rename() async {
        let trimmed = name.trimmingCharacters(in: .whitespacesAndNewlines)
        guard let dyno else {
            if trimmed.isEmpty, let vehicle = model.selectedVehicle { name = Self.defaultName(vehicle) }
            return
        }
        guard !trimmed.isEmpty, trimmed != dyno.name else {
            name = dyno.name
            return
        }
        await save(DynoInputDto(name: trimmed))
    }

    private func save(_ input: DynoInputDto) async {
        guard let vehicle = model.selectedVehicle, !vehicle.isSystem else { return }
        if let updated = await model.perform({ try await $0.saveDyno(vehicleId: vehicle.id, input) }) {
            dyno = updated
            name = updated.name
        }
    }

    private func deleteDyno() async {
        guard let dyno else { return }
        await model.perform { try await $0.deleteDyno(id: dyno.id) }
        await load()
    }

    /// Import is untrusted input: size-limited, decoded strictly and validated by the repository.
    private func importDyno(_ result: Result<URL, Error>) async {
        do {
            let url = try result.get()
            let scoped = url.startAccessingSecurityScopedResource()
            defer { if scoped { url.stopAccessingSecurityScopedResource() } }
            let size = try url.resourceValues(forKeys: [.fileSizeKey]).fileSize ?? 0
            guard size <= 5 * 1024 * 1024 else {
                model.errorMessage = String(localized: "Die Datei ist zu groß (max. 5 MB).")
                return
            }
            let export = try JSONDecoder().decode(DynoExportDto.self, from: Data(contentsOf: url))
            if let vehicle = await model.perform({ try await $0.importDyno(export) }) {
                model.selectedVehicleId = vehicle.id
                await load()
            }
        } catch {
            model.report(error)
        }
    }
}

/// JSON file wrapper for `fileExporter` (`DynoExportSchema` format).
struct JSONFileDocument: FileDocument {
    static let readableContentTypes: [UTType] = [.json]
    let data: Data

    init(_ export: DynoExportDto) throws {
        let encoder = JSONEncoder()
        encoder.outputFormatting = [.prettyPrinted, .sortedKeys]
        data = try encoder.encode(export)
    }

    init(configuration: ReadConfiguration) throws {
        data = configuration.file.regularFileContents ?? Data()
    }

    func fileWrapper(configuration: WriteConfiguration) throws -> FileWrapper {
        FileWrapper(regularFileWithContents: data)
    }
}
