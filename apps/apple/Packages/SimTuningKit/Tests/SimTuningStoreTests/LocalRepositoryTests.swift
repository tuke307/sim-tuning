import Foundation
import Testing
import SimTuningAPI
@testable import SimTuningStore

/// Swift mirror of `apps/web/lib/data/local-repository.spec.ts`.
@Suite("LocalRepository (guest mode)")
struct LocalRepositoryTests {
    @Test func listsStarterVehiclesFirstFullyNestedAndReadOnly() async throws {
        let repo = LocalRepository(fileURL: nil)
        let vehicles = await repo.listVehicles()
        #expect(vehicles.count == StarterCatalog.vehicles.count)
        let s51 = try #require(vehicles.first { $0.systemKey == "veh-s51b1-4" })
        #expect(s51.id == "sys-veh-s51b1-4")
        #expect(s51.isSystem)
        #expect(s51.motor?.hubL == 44)
        #expect(s51.motor?.einlass?.steuerzeitSZ == 135)
        #expect(s51.motor?.einlass?.vergaser != nil)
        #expect(s51.motor?.auslass?.auspuff?.diffusorStage == 0)
        await #expect(throws: RepositoryError.readOnlyVehicle) {
            try await repo.updateVehicle(id: s51.id, VehicleInputDto(gewicht: 1))
        }
        #expect(await repo.listEnvironments().count == 4)
        // Starter ids are stable across calls (guest data references them).
        #expect(await repo.listVehicles().map(\.motor?.id) == vehicles.map(\.motor?.id))
    }

    @Test func duplicatesPartiallyUpdatesAndDeletesOwnVehicles() async throws {
        let repo = LocalRepository(fileURL: nil)
        let starter = try #require(await repo.listVehicles().first)
        let copy = try await repo.duplicateVehicle(id: starter.id)
        #expect(!copy.isSystem)
        #expect(copy.name == "\(starter.name) (Kopie)")
        #expect(copy.motor?.hubL == starter.motor?.hubL)

        var motor = MotorInputDto(name: "Tuned")
        var auspuff = AuspuffInputDto()
        auspuff.abgasT = 550
        motor.auslass = AuslassInputDto(auspuff: auspuff)
        let updated = try await repo.updateVehicle(id: copy.id, VehicleInputDto(motor: motor))
        #expect(updated.motor?.name == "Tuned")
        #expect(updated.motor?.auslass?.auspuff?.abgasT == 550)
        #expect(updated.motor?.hubL == starter.motor?.hubL, "untouched fields stay")

        // Explicit null clears a field.
        var clear = MotorInputDto()
        clear.hubL = .some(nil)
        let cleared = try await repo.updateVehicle(id: copy.id, VehicleInputDto(motor: clear))
        #expect(cleared.motor?.hubL == nil)
        #expect(cleared.motor?.auslass?.auspuff?.abgasT == 550)

        try await repo.deleteVehicle(id: copy.id)
        #expect(await repo.getVehicle(id: copy.id) == nil)
    }

    @Test func storesDynoSeriesAndRoundTripsJsonExport() async throws {
        let repo = LocalRepository(fileURL: nil)
        let vehicle = try await repo.createVehicle(VehicleInputDto(name: "Mein S51", motor: MotorInputDto(name: "M 541")))
        let dyno = try await repo.saveDyno(vehicleId: vehicle.id, DynoInputDto(name: "Lauf 1"))
        _ = try await repo.saveDynoData(dynoId: dyno.id, DynoDataInputDto(drehzahl: [
            DrehzahlPointDto(zeit: 0, drehzahl: 3000),
            DrehzahlPointDto(zeit: 5, drehzahl: 3100),
        ]))
        // Omitted series stay, provided ones replace.
        let withPs = try await repo.saveDynoData(dynoId: dyno.id, DynoDataInputDto(dynoPs: [DynoPsPointDto(drehzahl: 3000, ps: 2)]))
        #expect(withPs.drehzahl.count == 2)

        let exported = try await repo.exportDyno(id: dyno.id)
        // Through JSON, like a file on disk.
        let json = try JSONEncoder().encode(exported)
        let decoded = try JSONDecoder().decode(DynoExportDto.self, from: json).validated()
        #expect(decoded.format == "sim-tuning-dyno")
        #expect(decoded.version == 3)
        #expect(decoded.dyno.drehzahl?.count == 2)

        let imported = try await repo.importDyno(decoded)
        #expect(imported.id != vehicle.id)
        let importedDyno = await repo.getDyno(vehicleId: imported.id)
        #expect(importedDyno?.drehzahl == decoded.dyno.drehzahl)
        #expect(await repo.getVehicle(id: vehicle.id)?.dynoId == dyno.id)
        #expect(imported.motor?.name == "M 541")
    }

    @Test func rejectsInvalidImports() async throws {
        let repo = LocalRepository(fileURL: nil)
        let bad = DynoExportDto(format: "zip", exportedAt: ISO8601.string(), vehicle: VehicleInputDto(name: "x"),
                                dyno: .init(name: "y"))
        await #expect(throws: ValidationError.self) { try await repo.importDyno(bad) }
        let tooMany = DynoExportDto(exportedAt: ISO8601.string(), vehicle: VehicleInputDto(name: "x"),
                                    dyno: .init(name: "y", drehzahl: Array(repeating: .init(zeit: 0, drehzahl: 0),
                                                                           count: maxDynoPoints + 1)))
        await #expect(throws: ValidationError.self) { try await repo.importDyno(tooMany) }
    }

    @Test func persistsToDiskWithoutStarterContent() async throws {
        let url = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString).appendingPathComponent("state.json")
        defer { try? FileManager.default.removeItem(at: url.deletingLastPathComponent()) }
        let repo = LocalRepository(fileURL: url)
        let created = try await repo.createVehicle(VehicleInputDto(name: "Persisted"))
        _ = try await repo.createEnvironment(EnvironmentInputDto(name: "Track", temperaturT: 20, luftdruckP: 1000))

        let reopened = LocalRepository(fileURL: url)
        #expect(await reopened.getVehicle(id: created.id)?.name == "Persisted")
        #expect(await reopened.listEnvironments().count == 5)
        let raw = try String(contentsOf: url, encoding: .utf8)
        #expect(!raw.contains("sys-veh"))
    }

    @Test func deletingEnvironmentDetachesDynos() async throws {
        let repo = LocalRepository(fileURL: nil)
        let env = try await repo.createEnvironment(EnvironmentInputDto(name: "Track"))
        let vehicle = try await repo.createVehicle(VehicleInputDto(name: "V"))
        let dyno = try await repo.saveDyno(vehicleId: vehicle.id, DynoInputDto(name: "D", environmentId: .some(env.id)))
        #expect(dyno.environmentId == env.id)
        try await repo.deleteEnvironment(id: env.id)
        #expect(await repo.getDyno(vehicleId: vehicle.id)?.environmentId == nil)
        await #expect(throws: RepositoryError.notFound("Environment")) {
            try await repo.deleteEnvironment(id: "sys-env-sommer")
        }
    }
}
