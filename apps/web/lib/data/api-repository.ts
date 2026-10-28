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
import { apiFetch } from '@/lib/api/client';
import type { SimRepository } from './repository';

async function call<T>(endpoint: string, init?: RequestInit): Promise<T> {
  const res = await apiFetch<T>(endpoint, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  });
  return res.data as T;
}

const json = (method: string, body: unknown): RequestInit => ({
  method,
  body: JSON.stringify(body),
});

/** Signed-in repository backed by the NestJS API (routes: docs/sim-api.md). */
export class ApiRepository implements SimRepository {
  readonly kind = 'api' as const;

  listVehicles() {
    return call<VehicleDto[]>('/vehicles');
  }
  getVehicle(id: string) {
    return call<VehicleDto>(`/vehicles/${encodeURIComponent(id)}`);
  }
  createVehicle(input: VehicleInputDto) {
    return call<VehicleDto>('/vehicles', json('POST', input));
  }
  updateVehicle(id: string, input: Partial<VehicleInputDto>) {
    return call<VehicleDto>(`/vehicles/${encodeURIComponent(id)}`, json('PATCH', input));
  }
  async deleteVehicle(id: string) {
    await call<null>(`/vehicles/${encodeURIComponent(id)}`, { method: 'DELETE' });
  }
  duplicateVehicle(id: string, name?: string) {
    return call<VehicleDto>(
      `/vehicles/${encodeURIComponent(id)}/duplicate`,
      json('POST', name ? { name } : {}),
    );
  }

  listEnvironments() {
    return call<EnvironmentDto[]>('/environments');
  }
  createEnvironment(input: EnvironmentInputDto) {
    return call<EnvironmentDto>('/environments', json('POST', input));
  }
  updateEnvironment(id: string, input: Partial<EnvironmentInputDto>) {
    return call<EnvironmentDto>(`/environments/${encodeURIComponent(id)}`, json('PATCH', input));
  }
  async deleteEnvironment(id: string) {
    await call<null>(`/environments/${encodeURIComponent(id)}`, { method: 'DELETE' });
  }

  getDyno(vehicleId: string) {
    return call<DynoDto | null>(`/vehicles/${encodeURIComponent(vehicleId)}/dyno`);
  }
  saveDyno(vehicleId: string, input: DynoInputDto) {
    return call<DynoDto>(`/vehicles/${encodeURIComponent(vehicleId)}/dyno`, json('PUT', input));
  }
  saveDynoData(dynoId: string, data: DynoDataInputDto) {
    return call<DynoDto>(`/dynos/${encodeURIComponent(dynoId)}/data`, json('PUT', data));
  }
  async deleteDyno(dynoId: string) {
    await call<null>(`/dynos/${encodeURIComponent(dynoId)}`, { method: 'DELETE' });
  }
  exportDyno(dynoId: string) {
    return call<DynoExportDto>(`/dynos/${encodeURIComponent(dynoId)}/export`);
  }
  importDyno(data: DynoExportDto) {
    return call<VehicleDto>('/dynos/import', json('POST', data));
  }
}
