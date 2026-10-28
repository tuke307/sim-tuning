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
import { DynoExportSchema } from '@sim-tuning/common/schemas';
import { generateId, idbGet, idbSet } from '@/lib/offline/offline-store';
import {
  applyDynoData,
  buildDynoExport,
  buildEnvironment,
  buildVehicle,
  mergeVehicle,
  starterEnvironments,
  starterVehicles,
  upsertDyno,
  vehicleToInput,
} from './model';
import { NotFoundError, ReadOnlyVehicleError, type SimRepository } from './repository';

export interface GuestData {
  vehicles: VehicleDto[];
  environments: EnvironmentDto[];
  dynos: DynoDto[];
}

interface LocalState {
  version: 1;
  vehicles: VehicleDto[];
  environments: EnvironmentDto[];
  dynos: DynoDto[];
}

const STATE_KEY = 'sim_local_state_v1';

const byName = (a: { name: string }, b: { name: string }) => a.name.localeCompare(b.name);
const sortSystemFirst = <T extends { isSystem: boolean; name: string }>(rows: T[]) =>
  [...rows].sort((a, b) => Number(b.isSystem) - Number(a.isSystem) || byName(a, b));

/** Guest-mode repository: everything lives in IndexedDB on this device. */
export class LocalRepository implements SimRepository {
  readonly kind = 'local' as const;
  private queue: Promise<unknown> = Promise.resolve();

  private async load(): Promise<LocalState> {
    const stored = await idbGet<LocalState>(STATE_KEY);
    const state: LocalState = stored ?? { version: 1, vehicles: [], environments: [], dynos: [] };
    // Starter content is always the current catalog (read-only, never persisted).
    return {
      ...state,
      vehicles: [...starterVehicles(), ...state.vehicles.filter((v) => !v.isSystem)],
      environments: [...starterEnvironments(), ...state.environments.filter((e) => !e.isSystem)],
    };
  }

  /** Serialises read-modify-write cycles so concurrent saves never lose updates. */
  private mutate<T>(fn: (state: LocalState) => T | Promise<T>): Promise<T> {
    const run = this.queue.then(async () => {
      const state = await this.load();
      const result = await fn(state);
      await idbSet<LocalState>(STATE_KEY, {
        version: 1,
        vehicles: state.vehicles.filter((v) => !v.isSystem),
        environments: state.environments.filter((e) => !e.isSystem),
        dynos: state.dynos,
      });
      return result;
    });
    this.queue = run.catch(() => undefined);
    return run;
  }

  private ownVehicle(state: LocalState, id: string): VehicleDto {
    const vehicle = state.vehicles.find((v) => v.id === id);
    if (!vehicle) throw new NotFoundError('Vehicle');
    if (vehicle.isSystem) throw new ReadOnlyVehicleError();
    return vehicle;
  }

  /** Everything the guest created on this device (starter content excluded). */
  async snapshotOwnData(): Promise<GuestData> {
    const state = await this.load();
    return {
      vehicles: state.vehicles.filter((v) => !v.isSystem),
      environments: state.environments.filter((e) => !e.isSystem),
      dynos: state.dynos,
    };
  }

  /** Removes all guest-created data (after it was moved into an account, or discarded). */
  clearOwnData() {
    return this.mutate((state) => {
      state.vehicles = state.vehicles.filter((v) => v.isSystem);
      state.environments = state.environments.filter((e) => e.isSystem);
      state.dynos = [];
    });
  }

  async listVehicles() {
    return sortSystemFirst((await this.load()).vehicles);
  }

  async getVehicle(id: string) {
    return (await this.load()).vehicles.find((v) => v.id === id) ?? null;
  }

  createVehicle(input: VehicleInputDto) {
    return this.mutate((state) => {
      const vehicle = buildVehicle(input, generateId);
      state.vehicles.push(vehicle);
      return vehicle;
    });
  }

  updateVehicle(id: string, input: Partial<VehicleInputDto>) {
    return this.mutate((state) => {
      const updated = mergeVehicle(this.ownVehicle(state, id), input, generateId);
      state.vehicles = state.vehicles.map((v) => (v.id === id ? updated : v));
      return updated;
    });
  }

  deleteVehicle(id: string) {
    return this.mutate((state) => {
      this.ownVehicle(state, id);
      state.vehicles = state.vehicles.filter((v) => v.id !== id);
      state.dynos = state.dynos.filter((d) => d.vehicleId !== id);
    });
  }

  duplicateVehicle(id: string, name?: string) {
    return this.mutate((state) => {
      const source = state.vehicles.find((v) => v.id === id);
      if (!source) throw new NotFoundError('Vehicle');
      const copy = buildVehicle(
        { ...vehicleToInput(source), name: name?.trim() || `${source.name} (Kopie)` },
        generateId,
      );
      state.vehicles.push(copy);
      return copy;
    });
  }

  async listEnvironments() {
    return sortSystemFirst((await this.load()).environments);
  }

  createEnvironment(input: EnvironmentInputDto) {
    return this.mutate((state) => {
      const env = buildEnvironment(input, generateId);
      state.environments.push(env);
      return env;
    });
  }

  updateEnvironment(id: string, input: Partial<EnvironmentInputDto>) {
    return this.mutate((state) => {
      const env = state.environments.find((e) => e.id === id && !e.isSystem);
      if (!env) throw new NotFoundError('Environment');
      const updated: EnvironmentDto = {
        ...env,
        name: input.name ?? env.name,
        temperaturT:
          input.temperaturT !== undefined ? (input.temperaturT ?? null) : env.temperaturT,
        luftdruckP: input.luftdruckP !== undefined ? (input.luftdruckP ?? null) : env.luftdruckP,
        updatedAt: new Date().toISOString(),
      };
      state.environments = state.environments.map((e) => (e.id === id ? updated : e));
      return updated;
    });
  }

  deleteEnvironment(id: string) {
    return this.mutate((state) => {
      if (!state.environments.some((e) => e.id === id && !e.isSystem)) {
        throw new NotFoundError('Environment');
      }
      state.environments = state.environments.filter((e) => e.id !== id);
      state.dynos = state.dynos.map((d) =>
        d.environmentId === id ? { ...d, environmentId: null } : d,
      );
    });
  }

  async getDyno(vehicleId: string) {
    return (await this.load()).dynos.find((d) => d.vehicleId === vehicleId) ?? null;
  }

  saveDyno(vehicleId: string, input: DynoInputDto) {
    return this.mutate((state) => {
      this.ownVehicle(state, vehicleId);
      const existing = state.dynos.find((d) => d.vehicleId === vehicleId) ?? null;
      const dyno = upsertDyno(existing, vehicleId, input, generateId);
      state.dynos = [...state.dynos.filter((d) => d.vehicleId !== vehicleId), dyno];
      state.vehicles = state.vehicles.map((v) =>
        v.id === vehicleId ? { ...v, dynoId: dyno.id } : v,
      );
      return dyno;
    });
  }

  saveDynoData(dynoId: string, data: DynoDataInputDto) {
    return this.mutate((state) => {
      const dyno = state.dynos.find((d) => d.id === dynoId);
      if (!dyno) throw new NotFoundError('Dyno');
      const updated = applyDynoData(dyno, data);
      state.dynos = state.dynos.map((d) => (d.id === dynoId ? updated : d));
      return updated;
    });
  }

  deleteDyno(dynoId: string) {
    return this.mutate((state) => {
      const dyno = state.dynos.find((d) => d.id === dynoId);
      if (!dyno) throw new NotFoundError('Dyno');
      state.dynos = state.dynos.filter((d) => d.id !== dynoId);
      state.vehicles = state.vehicles.map((v) =>
        v.id === dyno.vehicleId ? { ...v, dynoId: null } : v,
      );
    });
  }

  async exportDyno(dynoId: string): Promise<DynoExportDto> {
    const state = await this.load();
    const dyno = state.dynos.find((d) => d.id === dynoId);
    const vehicle = dyno && state.vehicles.find((v) => v.id === dyno.vehicleId);
    if (!dyno || !vehicle) throw new NotFoundError('Dyno');
    return buildDynoExport(vehicle, dyno);
  }

  importDyno(data: DynoExportDto) {
    // Validate untrusted file content before it touches local state.
    const parsed = DynoExportSchema.parse(data);
    return this.mutate((state) => {
      const vehicle = buildVehicle(parsed.vehicle, generateId);
      const { drehzahl, dynoPs, ausrollen, geschwindigkeit, ...dynoInput } = parsed.dyno;
      const dyno = applyDynoData(upsertDyno(null, vehicle.id, dynoInput, generateId), {
        drehzahl,
        dynoPs,
        ausrollen,
        geschwindigkeit,
      });
      const withDyno = { ...vehicle, dynoId: dyno.id };
      state.vehicles.push(withDyno);
      state.dynos.push(dyno);
      return withDyno;
    });
  }
}
