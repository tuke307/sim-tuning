import type { GuestData } from './local-repository';
import { vehicleToInput } from './model';
import type { SimRepository } from './repository';

export interface GuestMigrationResult {
  vehicles: number;
  environments: number;
  dynos: number;
}

export const isGuestDataEmpty = (data: GuestData) =>
  data.vehicles.length === 0 && data.environments.length === 0 && data.dynos.length === 0;

/**
 * Copies guest-mode data into a signed-in account (Phase 10). Environments are created first so
 * dynos keep their environment; starter environments map by `systemKey`. Runs sequentially so a
 * failure leaves already-copied rows intact and the local copy untouched (caller clears it only
 * after success).
 */
export async function migrateGuestData(
  data: GuestData,
  target: SimRepository,
): Promise<GuestMigrationResult> {
  const remoteEnvironments = await target.listEnvironments();
  const environmentIds = new Map<string, string>();
  for (const env of remoteEnvironments) {
    if (env.systemKey) environmentIds.set(`sys-${env.systemKey}`, env.id);
  }
  for (const env of data.environments) {
    const created = await target.createEnvironment({
      name: env.name,
      temperaturT: env.temperaturT,
      luftdruckP: env.luftdruckP,
    });
    environmentIds.set(env.id, created.id);
  }

  let dynos = 0;
  for (const vehicle of data.vehicles) {
    const created = await target.createVehicle(vehicleToInput(vehicle));
    const dyno = data.dynos.find((d) => d.vehicleId === vehicle.id);
    if (!dyno) continue;
    const remoteDyno = await target.saveDyno(created.id, {
      name: dyno.name,
      beschreibung: dyno.beschreibung,
      active: dyno.active,
      environmentId: dyno.environmentId ? (environmentIds.get(dyno.environmentId) ?? null) : null,
    });
    const strip = (samples: typeof dyno.ausrollen) =>
      samples.map(
        (s) =>
          Object.fromEntries(Object.entries(s).filter(([, v]) => v !== null)) as {
            latitude: number;
            longitude: number;
          },
      );
    await target.saveDynoData(remoteDyno.id, {
      drehzahl: dyno.drehzahl,
      dynoPs: dyno.dynoPs,
      ausrollen: strip(dyno.ausrollen),
      geschwindigkeit: strip(dyno.geschwindigkeit),
    });
    dynos++;
  }

  return { vehicles: data.vehicles.length, environments: data.environments.length, dynos };
}
