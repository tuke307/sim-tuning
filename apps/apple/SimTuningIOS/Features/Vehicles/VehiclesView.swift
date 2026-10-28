import SimTuningAPI
import SwiftUI

struct VehiclesView: View {
    @Environment(AppModel.self) private var model
    @State private var editing: VehicleDto?
    @State private var creating = false
    @State private var pendingDelete: VehicleDto?

    var body: some View {
        NavigationStack {
            List {
                let system = model.vehicles.filter(\.isSystem)
                let own = model.vehicles.filter { !$0.isSystem }
                Section("Eigene Fahrzeuge") {
                    if own.isEmpty {
                        Text("Noch keine eigenen Fahrzeuge. Dupliziere ein Startfahrzeug oder lege ein neues an.")
                            .foregroundStyle(.secondary)
                    }
                    ForEach(own) { row($0) }
                }
                Section {
                    ForEach(system) { row($0) }
                } header: {
                    Text("Startfahrzeuge")
                } footer: {
                    Text("Startfahrzeuge sind schreibgeschützt und können dupliziert werden.")
                }
            }
            .overlay {
                if model.isLoading && model.vehicles.isEmpty { ProgressView() }
            }
            .refreshable { await model.reload() }
            .navigationTitle("Fahrzeuge")
            .toolbar {
                ToolbarItem(placement: .topBarTrailing) {
                    Button("Fahrzeug anlegen", systemImage: "plus") { creating = true }
                        .accessibilityIdentifier("vehicles.add")
                }
            }
            .sheet(isPresented: $creating) {
                VehicleEditView(vehicle: nil)
            }
            .sheet(item: $editing) { vehicle in
                VehicleEditView(vehicle: vehicle)
            }
            .confirmationDialog("Fahrzeug löschen?", isPresented: Binding(
                get: { pendingDelete != nil }, set: { if !$0 { pendingDelete = nil } }
            ), titleVisibility: .visible, presenting: pendingDelete) { vehicle in
                Button("Löschen", role: .destructive) {
                    Task { await model.perform { try await $0.deleteVehicle(id: vehicle.id) } }
                }
            } message: { _ in
                Text("Das Fahrzeug und sein Dyno werden dauerhaft gelöscht.")
            }
        }
    }

    @ViewBuilder
    private func row(_ vehicle: VehicleDto) -> some View {
        let selected = vehicle.id == model.selectedVehicleId
        Button {
            model.selectedVehicleId = vehicle.id
        } label: {
            HStack(alignment: .firstTextBaseline) {
                VStack(alignment: .leading, spacing: 2) {
                    Text(verbatim: vehicle.name).font(.headline)
                    if let motor = vehicle.motor {
                        Text(verbatim: [motor.name, motor.hubraumV.map { model.format($0 / 1000, unit: "cm³") }]
                            .compactMap { $0 }.joined(separator: " · "))
                            .font(.subheadline)
                            .foregroundStyle(.secondary)
                    }
                }
                Spacer()
                if selected {
                    Image(systemName: "checkmark.circle.fill")
                        .foregroundStyle(.tint)
                        .accessibilityHidden(true)
                }
            }
            .contentShape(Rectangle())
        }
        .buttonStyle(.plain)
        .accessibilityAddTraits(selected ? .isSelected : [])
        .accessibilityHint(Text("Für alle Module auswählen"))
        .swipeActions(edge: .trailing) {
            if !vehicle.isSystem {
                Button("Löschen", systemImage: "trash", role: .destructive) { pendingDelete = vehicle }
            }
            Button("Duplizieren", systemImage: "plus.square.on.square") {
                Task {
                    if let copy = await model.perform({ try await $0.duplicateVehicle(id: vehicle.id, name: nil) }) {
                        model.selectedVehicleId = copy.id
                    }
                }
            }
            .tint(.indigo)
        }
        .contextMenu {
            Button("Duplizieren", systemImage: "plus.square.on.square") {
                Task { _ = await model.perform { try await $0.duplicateVehicle(id: vehicle.id, name: nil) } }
            }
            if !vehicle.isSystem {
                Button("Bearbeiten", systemImage: "pencil") { editing = vehicle }
                Button("Löschen", systemImage: "trash", role: .destructive) { pendingDelete = vehicle }
            }
        }
        .swipeActions(edge: .leading) {
            if !vehicle.isSystem {
                Button("Bearbeiten", systemImage: "pencil") { editing = vehicle }.tint(.orange)
            }
        }
    }
}

/// Create/edit sheet for own vehicles (basic data + engine geometry). Module specific values are
/// edited and saved from the module calculators.
struct VehicleEditView: View {
    @Environment(AppModel.self) private var model
    @Environment(\.dismiss) private var dismiss
    let vehicle: VehicleDto?

    @State private var name = ""
    @State private var beschreibung = ""
    @State private var gewicht: Double?
    @State private var motorName = ""
    @State private var hubraumCm3: Double?
    @State private var hubL: Double?
    @State private var bohrungD: Double?
    @State private var pleulL: Double?
    @State private var deachsierungL: Double?
    @State private var brennraumCm3: Double?
    @State private var kurbelgehaeuseCm3: Double?
    @State private var resonanzU: Double?

    var body: some View {
        NavigationStack {
            Form {
                Section("Fahrzeug") {
                    TextField("Name", text: $name)
                        .accessibilityIdentifier("vehicle.edit.name")
                    TextField("Beschreibung", text: $beschreibung, axis: .vertical)
                    NumberField("Gewicht", value: $gewicht, unit: "kg")
                }
                Section("Motor") {
                    TextField("Motorname", text: $motorName)
                    NumberField("Hubraum", value: $hubraumCm3, unit: "cm³")
                    NumberField("Hub", value: $hubL, unit: "mm")
                    NumberField("Bohrung", value: $bohrungD, unit: "mm")
                    NumberField("Pleuellänge", value: $pleulL, unit: "mm")
                    NumberField("Deachsierung", value: $deachsierungL, unit: "mm")
                    NumberField("Brennraum", value: $brennraumCm3, unit: "cm³")
                    NumberField("Kurbelgehäuse", value: $kurbelgehaeuseCm3, unit: "cm³")
                    NumberField("Resonanzdrehzahl", value: $resonanzU, unit: "U/min")
                }
            }
            .navigationTitle(vehicle == nil ? "Neues Fahrzeug" : "Fahrzeug bearbeiten")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) { Button("Abbrechen") { dismiss() } }
                ToolbarItem(placement: .confirmationAction) {
                    Button("Sichern") { Task { await save() } }
                        .disabled(name.trimmingCharacters(in: .whitespaces).isEmpty)
                        .accessibilityIdentifier("vehicle.edit.save")
                }
            }
            .onAppear(perform: load)
        }
    }

    private func load() {
        guard let vehicle else { return }
        name = vehicle.name
        beschreibung = vehicle.beschreibung ?? ""
        gewicht = vehicle.gewicht
        let m = vehicle.motor
        motorName = m?.name ?? ""
        hubraumCm3 = m?.hubraumV.map { $0 / 1000 }
        hubL = m?.hubL
        bohrungD = m?.bohrungD
        pleulL = m?.pleulL
        deachsierungL = m?.deachsierungL
        brennraumCm3 = m?.brennraumV.map { $0 / 1000 }
        kurbelgehaeuseCm3 = m?.kurbelgehaeuseV.map { $0 / 1000 }
        resonanzU = m?.resonanzU
    }

    private func save() async {
        var motor = MotorInputDto(name: motorName.isEmpty ? name : motorName)
        // Persist in base units (mm³).
        motor.hubraumV = .some(hubraumCm3.map { $0 * 1000 })
        motor.hubL = .some(hubL)
        motor.bohrungD = .some(bohrungD)
        motor.pleulL = .some(pleulL)
        motor.deachsierungL = .some(deachsierungL)
        motor.brennraumV = .some(brennraumCm3.map { $0 * 1000 })
        motor.kurbelgehaeuseV = .some(kurbelgehaeuseCm3.map { $0 * 1000 })
        motor.resonanzU = .some(resonanzU)
        let input = VehicleInputDto(name: name, beschreibung: .some(beschreibung.isEmpty ? nil : beschreibung),
                                    gewicht: .some(gewicht), motor: motor)
        let saved: VehicleDto?
        if let vehicle {
            saved = await model.perform { try await $0.updateVehicle(id: vehicle.id, input) }
        } else {
            saved = await model.perform { try await $0.createVehicle(input) }
        }
        if let saved {
            model.selectedVehicleId = saved.id
            dismiss()
        }
    }
}
