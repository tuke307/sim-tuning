import type {
  DynoDataInputDto,
  DynoDto,
  DynoExportDto,
  DynoInputDto,
  EnvironmentDto,
  EnvironmentInputDto,
  VehicleDto,
  VehicleInputDto,
} from '@sim-tuning/common/schemas';

/**
 * Data access for the SimTuning modules. Two implementations share this contract
 * (docs/sim-api.md): `ApiRepository` for signed-in users and `LocalRepository` (IndexedDB) for
 * guest mode. Views must only talk to this interface (via `useSimData()`), never to `fetch`.
 */
export interface SimRepository {
  readonly kind: 'api' | 'local';

  listVehicles(): Promise<VehicleDto[]>;
  getVehicle(id: string): Promise<VehicleDto | null>;
  createVehicle(input: VehicleInputDto): Promise<VehicleDto>;
  /** Partial update: only provided (nested) fields change. System vehicles are read-only. */
  updateVehicle(id: string, input: Partial<VehicleInputDto>): Promise<VehicleDto>;
  deleteVehicle(id: string): Promise<void>;
  duplicateVehicle(id: string, name?: string): Promise<VehicleDto>;

  listEnvironments(): Promise<EnvironmentDto[]>;
  createEnvironment(input: EnvironmentInputDto): Promise<EnvironmentDto>;
  updateEnvironment(id: string, input: Partial<EnvironmentInputDto>): Promise<EnvironmentDto>;
  deleteEnvironment(id: string): Promise<void>;

  getDyno(vehicleId: string): Promise<DynoDto | null>;
  saveDyno(vehicleId: string, input: DynoInputDto): Promise<DynoDto>;
  /** Each provided series replaces the stored one; omitted series stay untouched. */
  saveDynoData(dynoId: string, data: DynoDataInputDto): Promise<DynoDto>;
  deleteDyno(dynoId: string): Promise<void>;
  exportDyno(dynoId: string): Promise<DynoExportDto>;
  importDyno(data: DynoExportDto): Promise<VehicleDto>;
}

export class ReadOnlyVehicleError extends Error {
  constructor() {
    super('Starter vehicles are read-only. Duplicate the vehicle to edit it.');
    this.name = 'ReadOnlyVehicleError';
  }
}

export class ReadOnlyEnvironmentError extends Error {
  constructor() {
    super('Starter environments are read-only.');
    this.name = 'ReadOnlyEnvironmentError';
  }
}

export class NotFoundError extends Error {
  constructor(what: string) {
    super(`${what} not found`);
    this.name = 'NotFoundError';
  }
}
