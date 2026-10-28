import Foundation
import SimTuningAPI

/// Signed-in repository: forwards every call to the REST API (`docs/sim-api.md`).
public struct APIRepository: SimRepository {
    public let kind = RepositoryKind.api
    public let client: APIClient

    public init(client: APIClient) {
        self.client = client
    }

    public func listVehicles() async throws -> [VehicleDto] { try await client.listVehicles() }

    public func getVehicle(id: String) async throws -> VehicleDto? {
        do {
            return try await client.getVehicle(id: id)
        } catch APIClientError.notFound {
            return nil
        }
    }

    public func createVehicle(_ input: VehicleInputDto) async throws -> VehicleDto {
        try await client.createVehicle(input)
    }

    public func updateVehicle(id: String, _ input: VehicleInputDto) async throws -> VehicleDto {
        try await client.updateVehicle(id: id, input)
    }

    public func deleteVehicle(id: String) async throws { try await client.deleteVehicle(id: id) }

    public func duplicateVehicle(id: String, name: String?) async throws -> VehicleDto {
        try await client.duplicateVehicle(id: id, name: name)
    }

    public func listEnvironments() async throws -> [EnvironmentDto] { try await client.listEnvironments() }

    public func createEnvironment(_ input: EnvironmentInputDto) async throws -> EnvironmentDto {
        try await client.createEnvironment(input)
    }

    public func updateEnvironment(id: String, _ input: EnvironmentInputDto) async throws -> EnvironmentDto {
        try await client.updateEnvironment(id: id, input)
    }

    public func deleteEnvironment(id: String) async throws { try await client.deleteEnvironment(id: id) }

    public func getDyno(vehicleId: String) async throws -> DynoDto? { try await client.getDyno(vehicleId: vehicleId) }

    public func saveDyno(vehicleId: String, _ input: DynoInputDto) async throws -> DynoDto {
        try await client.saveDyno(vehicleId: vehicleId, input)
    }

    public func saveDynoData(dynoId: String, _ data: DynoDataInputDto) async throws -> DynoDto {
        try await client.saveDynoData(dynoId: dynoId, data)
    }

    public func deleteDyno(id: String) async throws { try await client.deleteDyno(id: id) }

    public func exportDyno(id: String) async throws -> DynoExportDto { try await client.exportDyno(id: id) }

    public func importDyno(_ data: DynoExportDto) async throws -> VehicleDto { try await client.importDyno(data) }
}
