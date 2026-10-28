import Foundation
import SimTuningAPI

public enum RepositoryError: Error, Equatable, LocalizedError {
    case notFound(String)
    case readOnlyVehicle

    public var errorDescription: String? {
        switch self {
        case let .notFound(what): return "\(what) not found"
        case .readOnlyVehicle: return "Starter vehicles are read-only. Duplicate the vehicle to edit it."
        }
    }
}

/// Data access shared by the API-backed and the local (guest) implementation (`docs/sim-api.md`).
public protocol SimRepository: Sendable {
    var kind: RepositoryKind { get }
    func listVehicles() async throws -> [VehicleDto]
    func getVehicle(id: String) async throws -> VehicleDto?
    func createVehicle(_ input: VehicleInputDto) async throws -> VehicleDto
    /// Partial update: only provided (nested) fields change. System vehicles are read-only.
    func updateVehicle(id: String, _ input: VehicleInputDto) async throws -> VehicleDto
    func deleteVehicle(id: String) async throws
    func duplicateVehicle(id: String, name: String?) async throws -> VehicleDto
    func listEnvironments() async throws -> [EnvironmentDto]
    func createEnvironment(_ input: EnvironmentInputDto) async throws -> EnvironmentDto
    func updateEnvironment(id: String, _ input: EnvironmentInputDto) async throws -> EnvironmentDto
    func deleteEnvironment(id: String) async throws
    func getDyno(vehicleId: String) async throws -> DynoDto?
    func saveDyno(vehicleId: String, _ input: DynoInputDto) async throws -> DynoDto
    /// Each provided series replaces the stored one; omitted series stay untouched.
    func saveDynoData(dynoId: String, _ data: DynoDataInputDto) async throws -> DynoDto
    func deleteDyno(id: String) async throws
    func exportDyno(id: String) async throws -> DynoExportDto
    func importDyno(_ data: DynoExportDto) async throws -> VehicleDto
}

public enum RepositoryKind: String, Sendable { case api, local }

/// Guest-mode repository — Swift port of `apps/web/lib/data/local-repository.ts`. Everything lives
/// in one JSON file in Application Support (excluded from backups is not required: it is user
/// data). Starter content is always the current catalog (read-only, never persisted). Actor
/// isolation serialises read-modify-write cycles so concurrent saves never lose updates.
public actor LocalRepository: SimRepository {
    public nonisolated let kind = RepositoryKind.local

    struct State: Codable {
        var version = 1
        var vehicles: [VehicleDto] = []
        var environments: [EnvironmentDto] = []
        var dynos: [DynoDto] = []
    }

    private let fileURL: URL?
    private let idFactory: SimModel.IdFactory
    private var cache: State?

    /// - Parameter fileURL: storage file; `nil` keeps everything in memory (tests, UI tests).
    public init(fileURL: URL? = LocalRepository.defaultFileURL,
                idFactory: @escaping @Sendable (String) -> String = SimModel.generateId) {
        self.fileURL = fileURL
        self.idFactory = idFactory
    }

    public static var defaultFileURL: URL? {
        guard let base = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first
        else { return nil }
        return base.appendingPathComponent("SimTuning", isDirectory: true)
            .appendingPathComponent("local-state-v1.json")
    }

    // MARK: Starter content

    public static func starterVehicleToInput(_ starter: StarterCatalog.Vehicle) -> VehicleInputDto {
        let m = starter.motor
        var motor = MotorInputDto(name: m.name)
        motor.hubL = .some(m.hubL)
        motor.bohrungD = .some(m.bohrungD)
        motor.pleulL = .some(m.pleulL)
        motor.deachsierungL = .some(m.deachsierungL)
        motor.resonanzU = .some(m.resonanzU)
        motor.hubraumV = .some(m.hubraumV)
        motor.brennraumV = .some(m.brennraumV)
        motor.kurbelgehaeuseV = .some(m.kurbelgehaeuseV)
        motor.einlass = EinlassInputDto(steuerzeitSZ: .some(m.einlassSteuerzeitSZ))
        motor.auslass = AuslassInputDto(steuerzeitSZ: .some(m.auslassSteuerzeitSZ))
        motor.ueberstroemer = UeberstroemerInputDto(steuerzeitSZ: .some(m.ueberstroemerSteuerzeitSZ))
        return VehicleInputDto(name: starter.name, beschreibung: .some(starter.beschreibung),
                               gewicht: .some(starter.gewicht), motor: motor)
    }

    /// Stable ids for starter content so guest data survives restarts.
    public static func starterVehicles() -> [VehicleDto] {
        StarterCatalog.vehicles.map { starter in
            final class Counter: @unchecked Sendable { var n = 0 }
            let counter = Counter()
            let id: SimModel.IdFactory = { prefix in
                defer { counter.n += 1 }
                return "sys-\(starter.systemKey)-\(prefix)-\(counter.n)"
            }
            var vehicle = SimModel.buildVehicle(starterVehicleToInput(starter), id: id, systemKey: starter.systemKey)
            vehicle.id = "sys-\(starter.systemKey)"
            return vehicle
        }
    }

    public static func starterEnvironments() -> [EnvironmentDto] {
        let at = "2026-01-01T00:00:00.000Z"
        return StarterCatalog.environments.map {
            EnvironmentDto(id: "sys-\($0.systemKey)", createdAt: at, updatedAt: at, systemKey: $0.systemKey,
                           isSystem: true, name: $0.name, temperaturT: $0.temperaturT, luftdruckP: $0.luftdruckP)
        }
    }

    // MARK: Persistence

    private func load() -> State {
        var state: State
        if let cache {
            state = cache
        } else if let fileURL, let data = try? Data(contentsOf: fileURL),
                  let stored = try? JSONDecoder().decode(State.self, from: data) {
            state = stored
        } else {
            state = State()
        }
        cache = state
        state.vehicles = Self.starterVehicles() + state.vehicles.filter { !$0.isSystem }
        state.environments = Self.starterEnvironments() + state.environments.filter { !$0.isSystem }
        return state
    }

    private func mutate<T>(_ body: (inout State) throws -> T) throws -> T {
        var state = load()
        let result = try body(&state)
        let persisted = State(version: 1, vehicles: state.vehicles.filter { !$0.isSystem },
                              environments: state.environments.filter { !$0.isSystem }, dynos: state.dynos)
        if let fileURL {
            try FileManager.default.createDirectory(at: fileURL.deletingLastPathComponent(),
                                                    withIntermediateDirectories: true)
            try JSONEncoder().encode(persisted).write(to: fileURL, options: [.atomic, .completeFileProtection])
        }
        cache = persisted
        return result
    }

    private static func sortSystemFirst<T>(_ rows: [T], isSystem: (T) -> Bool, name: (T) -> String) -> [T] {
        rows.enumerated().sorted { a, b in
            let sa = isSystem(a.element)
            let sb = isSystem(b.element)
            if sa != sb { return sa }
            let order = name(a.element).localizedCompare(name(b.element))
            if order != .orderedSame { return order == .orderedAscending }
            return a.offset < b.offset
        }.map(\.element)
    }

    private func ownVehicle(_ state: State, _ id: String) throws -> VehicleDto {
        guard let vehicle = state.vehicles.first(where: { $0.id == id }) else { throw RepositoryError.notFound("Vehicle") }
        if vehicle.isSystem { throw RepositoryError.readOnlyVehicle }
        return vehicle
    }

    // MARK: Vehicles

    public func listVehicles() -> [VehicleDto] {
        Self.sortSystemFirst(load().vehicles, isSystem: \.isSystem, name: \.name)
    }

    public func getVehicle(id: String) -> VehicleDto? {
        load().vehicles.first { $0.id == id }
    }

    public func createVehicle(_ input: VehicleInputDto) throws -> VehicleDto {
        let valid = try input.validated(requireName: true)
        return try mutate { state in
            let vehicle = SimModel.buildVehicle(valid, id: idFactory)
            state.vehicles.append(vehicle)
            return vehicle
        }
    }

    public func updateVehicle(id: String, _ input: VehicleInputDto) throws -> VehicleDto {
        let valid = try input.validated(requireName: false)
        return try mutate { state in
            let updated = SimModel.mergeVehicle(try ownVehicle(state, id), valid, id: idFactory)
            state.vehicles = state.vehicles.map { $0.id == id ? updated : $0 }
            return updated
        }
    }

    public func deleteVehicle(id: String) throws {
        try mutate { state in
            _ = try ownVehicle(state, id)
            state.vehicles.removeAll { $0.id == id }
            state.dynos.removeAll { $0.vehicleId == id }
        }
    }

    public func duplicateVehicle(id: String, name: String? = nil) throws -> VehicleDto {
        try mutate { state in
            guard let source = state.vehicles.first(where: { $0.id == id }) else {
                throw RepositoryError.notFound("Vehicle")
            }
            var input = SimModel.vehicleToInput(source)
            let trimmed = name?.trimmingCharacters(in: .whitespacesAndNewlines) ?? ""
            input.name = trimmed.isEmpty ? "\(source.name) (Kopie)" : trimmed
            let copy = SimModel.buildVehicle(input, id: idFactory)
            state.vehicles.append(copy)
            return copy
        }
    }

    // MARK: Environments

    public func listEnvironments() -> [EnvironmentDto] {
        Self.sortSystemFirst(load().environments, isSystem: \.isSystem, name: \.name)
    }

    public func createEnvironment(_ input: EnvironmentInputDto) throws -> EnvironmentDto {
        var valid = input
        valid.name = try SimValidation.name(input.name, "name", required: true)
        return try mutate { state in
            let env = SimModel.buildEnvironment(valid, id: idFactory)
            state.environments.append(env)
            return env
        }
    }

    public func updateEnvironment(id: String, _ input: EnvironmentInputDto) throws -> EnvironmentDto {
        let name = try SimValidation.name(input.name, "name", required: false)
        return try mutate { state in
            guard var env = state.environments.first(where: { $0.id == id && !$0.isSystem }) else {
                throw RepositoryError.notFound("Environment")
            }
            if let name { env.name = name }
            if let t = input.temperaturT { env.temperaturT = t }
            if let p = input.luftdruckP { env.luftdruckP = p }
            env.updatedAt = ISO8601.string()
            let updated = env
            state.environments = state.environments.map { $0.id == id ? updated : $0 }
            return updated
        }
    }

    public func deleteEnvironment(id: String) throws {
        try mutate { state in
            guard state.environments.contains(where: { $0.id == id && !$0.isSystem }) else {
                throw RepositoryError.notFound("Environment")
            }
            state.environments.removeAll { $0.id == id }
            state.dynos = state.dynos.map { dyno in
                var next = dyno
                if next.environmentId == id { next.environmentId = nil }
                return next
            }
        }
    }

    // MARK: Dyno

    public func getDyno(vehicleId: String) -> DynoDto? {
        load().dynos.first { $0.vehicleId == vehicleId }
    }

    public func saveDyno(vehicleId: String, _ input: DynoInputDto) throws -> DynoDto {
        var valid = input
        valid.name = try SimValidation.name(input.name, "name", required: true)!
        return try mutate { state in
            _ = try ownVehicle(state, vehicleId)
            let existing = state.dynos.first { $0.vehicleId == vehicleId }
            let dyno = SimModel.upsertDyno(existing, vehicleId: vehicleId, input: valid, id: idFactory)
            state.dynos = state.dynos.filter { $0.vehicleId != vehicleId } + [dyno]
            state.vehicles = state.vehicles.map { vehicle in
                var next = vehicle
                if next.id == vehicleId { next.dynoId = dyno.id }
                return next
            }
            return dyno
        }
    }

    public func saveDynoData(dynoId: String, _ data: DynoDataInputDto) throws -> DynoDto {
        try data.validate()
        return try mutate { state in
            guard let dyno = state.dynos.first(where: { $0.id == dynoId }) else { throw RepositoryError.notFound("Dyno") }
            let updated = SimModel.applyDynoData(dyno, data)
            state.dynos = state.dynos.map { $0.id == dynoId ? updated : $0 }
            return updated
        }
    }

    public func deleteDyno(id: String) throws {
        try mutate { state in
            guard let dyno = state.dynos.first(where: { $0.id == id }) else { throw RepositoryError.notFound("Dyno") }
            state.dynos.removeAll { $0.id == id }
            state.vehicles = state.vehicles.map { vehicle in
                var next = vehicle
                if next.id == dyno.vehicleId { next.dynoId = nil }
                return next
            }
        }
    }

    public func exportDyno(id: String) throws -> DynoExportDto {
        let state = load()
        guard let dyno = state.dynos.first(where: { $0.id == id }),
              let vehicle = state.vehicles.first(where: { $0.id == dyno.vehicleId }) else {
            throw RepositoryError.notFound("Dyno")
        }
        return SimModel.buildDynoExport(vehicle: vehicle, dyno: dyno)
    }

    /// Validates untrusted file content before it touches local state.
    public func importDyno(_ data: DynoExportDto) throws -> VehicleDto {
        let parsed = try data.validated()
        return try mutate { state in
            var vehicle = SimModel.buildVehicle(parsed.vehicle, id: idFactory)
            let d = parsed.dyno
            let dynoInput = DynoInputDto(name: d.name, beschreibung: d.beschreibung, active: d.active)
            let dyno = SimModel.applyDynoData(
                SimModel.upsertDyno(nil, vehicleId: vehicle.id, input: dynoInput, id: idFactory),
                DynoDataInputDto(drehzahl: d.drehzahl, dynoPs: d.dynoPs, ausrollen: d.ausrollen,
                                 geschwindigkeit: d.geschwindigkeit)
            )
            vehicle.dynoId = dyno.id
            state.vehicles.append(vehicle)
            state.dynos.append(dyno)
            return vehicle
        }
    }

    /// Removes all guest data (keeps the starter catalog).
    public func reset() throws {
        cache = State()
        if let fileURL, FileManager.default.fileExists(atPath: fileURL.path) {
            try FileManager.default.removeItem(at: fileURL)
        }
    }
}
