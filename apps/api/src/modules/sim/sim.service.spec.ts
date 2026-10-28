import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { ForbiddenException, NotFoundException } from '@nestjs/common';
import { DynoExportSchema, MAX_DYNO_POINTS } from '@sim-tuning/common/schemas';
import type { Environment } from '@sim-tuning/database';
import type { PrismaService } from '../database/prisma.service';
import { DynoService } from '../dyno/dyno.service';
import { EnvironmentService } from '../environment/environment.service';
import { VehicleService } from '../vehicle/vehicle.service';
import { DynoDataInputSchema } from '@sim-tuning/common/schemas';
import {
  IdParamSchema,
  VehicleUpdateSchema,
  type DynoRow,
  type MotorRow,
  type VehicleRow,
} from './sim.fields';
import { toDynoDto, toVehicleDto, vehicleRowToInput } from './sim.mapper';

// ─────────────────────────────────────────────────────────────────────────────
// Fixtures
// ─────────────────────────────────────────────────────────────────────────────
const ME = 'user-me';
const OTHER = 'user-other';
const AT = new Date('2026-01-02T03:04:05.000Z');
const ts = (id: string) => ({ id, createdAt: AT, updatedAt: AT });

const nullFields = <K extends string>(keys: readonly K[]) =>
  Object.fromEntries(keys.map((k) => [k, null])) as Record<K, null>;

function motorRow(id: string, overrides: Partial<MotorRow> = {}): MotorRow {
  return {
    ...ts(id),
    name: 'Motor',
    ...nullFields([
      'bohrungD',
      'brennraumV',
      'deachsierungL',
      'heizwertU',
      'kolbenG',
      'kurbelgehaeuseV',
      'pleulL',
      'resonanzU',
      'verdichtungV',
      'zuendzeitpunkt',
      'zylinderAnz',
    ]),
    hubL: 0.04,
    hubraumV: 0.00005,
    einlass: {
      ...ts(`${id}-einl`),
      motorId: id,
      ...nullFields(['breiteB', 'durchmesserD', 'flaecheA', 'hoeheH', 'laengeL', 'luftBedarf']),
      steuerzeitSZ: 130,
      vergaser: {
        ...ts(`${id}-verg`),
        einlassId: `${id}-einl`,
        benzinLuftF: 14.7,
        durchmesserD: 0.016,
      },
    },
    auslass: {
      ...ts(`${id}-ausl`),
      motorId: id,
      ...nullFields(['breiteB', 'durchmesserD', 'flaecheA', 'hoeheH', 'laengeL']),
      steuerzeitSZ: 180,
      auspuff: null,
    },
    ueberstroemer: null,
    ...overrides,
  } as MotorRow;
}

function vehicleRow(
  id: string,
  userId: string | null,
  extra: Partial<VehicleRow> = {},
): VehicleRow {
  return {
    ...ts(id),
    userId,
    systemKey: userId === null ? `sys-${id}` : null,
    name: `Vehicle ${id}`,
    beschreibung: null,
    gewicht: 80,
    motorId: `${id}-mot`,
    motor: motorRow(`${id}-mot`),
    dyno: null,
    ...extra,
  };
}

function environmentRow(id: string, userId: string | null): Environment {
  return {
    ...ts(id),
    userId,
    systemKey: userId === null ? `sys-${id}` : null,
    name: `Env ${id}`,
    temperaturT: 293.15,
    luftdruckP: 101325,
  };
}

function dynoRow(id: string, vehicleId: string): DynoRow {
  return {
    ...ts(id),
    vehicleId,
    environmentId: null,
    name: 'Run 1',
    beschreibung: 'Prüfstand',
    active: true,
    drehzahl: [
      { id: 'dz1', dynoId: id, position: 0, zeit: 0, drehzahl: 1000 },
      { id: 'dz2', dynoId: id, position: 1, zeit: 0.5, drehzahl: 5000 },
    ],
    dynoPs: [{ id: 'ps1', dynoId: id, position: 0, drehzahl: 5000, ps: 7.5 }],
    ausrollen: [
      {
        id: 'ar1',
        dynoId: id,
        position: 0,
        latitude: 52.5,
        longitude: 13.4,
        speed: 12.5,
        accuracy: null,
        altitude: null,
        altitudeAccuracy: null,
        heading: 90,
        headingAccuracy: null,
        zeitpunkt: AT,
      },
    ],
    geschwindigkeit: [],
  };
}

// ─────────────────────────────────────────────────────────────────────────────
// Recording Prisma mock: every delegate call is logged; handlers emulate the queries used.
// ─────────────────────────────────────────────────────────────────────────────
type Args = Record<string, unknown> & { where?: Record<string, unknown> };
type Handler = (args: Args) => unknown;
interface Call {
  op: string;
  args: Args;
}

interface ReadableWhere {
  id?: string;
  OR?: { userId: string | null; systemKey?: unknown }[];
}

/** Emulates `{ id, OR: [{ userId }, { userId: null, systemKey: { not: null } }] }`. */
function matchesReadable(
  row: { id: string; userId: string | null; systemKey: string | null },
  where: ReadableWhere,
) {
  if (where.id !== undefined && where.id !== row.id) return false;
  return (where.OR ?? []).some(
    (c) => c.userId === row.userId && (c.systemKey === undefined || row.systemKey !== null),
  );
}

function createMockPrisma(handlers: Record<string, Handler>) {
  const calls: Call[] = [];
  const delegate = (model: string) =>
    new Proxy(
      {},
      {
        get: (_target, method: string) => async (args: Args) => {
          const op = `${model}.${method}`;
          calls.push({ op, args });
          const handler = handlers[op];
          if (!handler) throw new Error(`Unexpected Prisma call ${op}`);
          return handler(args);
        },
      },
    );
  const prisma: Record<string, unknown> = new Proxy(
    {},
    {
      get: (_target, key: string) =>
        key === '$transaction'
          ? async <T>(fn: (tx: unknown) => Promise<T>) => fn(prisma)
          : delegate(key),
    },
  );
  return { prisma: prisma as unknown as PrismaService, calls };
}

function vehicleHandlers(rows: VehicleRow[]): Record<string, Handler> {
  return {
    'vehicle.findFirst': ({ where }) =>
      rows.find((r) => matchesReadable(r, where as ReadableWhere)) ?? null,
    'vehicle.findMany': ({ where }) => rows.filter((r) => matchesReadable(r, where as never)),
  };
}

function services(handlers: Record<string, Handler>) {
  const { prisma, calls } = createMockPrisma(handlers);
  const vehicles = new VehicleService(prisma);
  const environments = new EnvironmentService(prisma);
  const dynos = new DynoService(prisma, vehicles, environments);
  return { vehicles, environments, dynos, calls };
}

const ops = (calls: Call[]) => calls.map((c) => c.op);
const callOf = (calls: Call[], op: string) => {
  const call = calls.find((c) => c.op === op);
  assert.ok(call, `expected a ${op} call`);
  return call.args;
};

// ─────────────────────────────────────────────────────────────────────────────
// Vehicles
// ─────────────────────────────────────────────────────────────────────────────
describe('VehicleService — ownership', () => {
  const own = vehicleRow('own', ME);
  const foreign = vehicleRow('foreign', OTHER);
  const starter = vehicleRow('starter', null);
  const rows = [own, foreign, starter];

  it('lists starter vehicles first and never includes other users rows', async () => {
    const { vehicles } = services(vehicleHandlers(rows));
    const list = await vehicles.list(ME);
    assert.deepEqual(
      list.map((v) => [v.id, v.isSystem]),
      [
        ['starter', true],
        ['own', false],
      ],
    );
  });

  it('reads own and starter vehicles, 404 for another users vehicle', async () => {
    const { vehicles } = services(vehicleHandlers(rows));
    assert.equal((await vehicles.get(ME, 'own')).id, 'own');
    assert.equal((await vehicles.get(ME, 'starter')).isSystem, true);
    await assert.rejects(vehicles.get(ME, 'foreign'), NotFoundException);
    await assert.rejects(vehicles.get(ME, 'missing'), NotFoundException);
  });

  it('404s update/delete/duplicate of another users vehicle without writing', async () => {
    const { vehicles, calls } = services(vehicleHandlers(rows));
    await assert.rejects(vehicles.update(ME, 'foreign', { name: 'x' }), NotFoundException);
    await assert.rejects(vehicles.remove(ME, 'foreign'), NotFoundException);
    await assert.rejects(vehicles.duplicate(ME, 'foreign'), NotFoundException);
    assert.deepEqual(new Set(ops(calls)), new Set(['vehicle.findFirst']));
  });

  it('rejects update and delete of starter vehicles with 403', async () => {
    const { vehicles, calls } = services(vehicleHandlers(rows));
    await assert.rejects(vehicles.update(ME, 'starter', { name: 'Mine' }), (err: unknown) => {
      assert.ok(err instanceof ForbiddenException);
      assert.equal(err.message, 'Starter vehicles are read-only');
      return true;
    });
    await assert.rejects(vehicles.remove(ME, 'starter'), ForbiddenException);
    assert.deepEqual(new Set(ops(calls)), new Set(['vehicle.findFirst']));
  });
});

describe('VehicleService — writes', () => {
  it('creates a vehicle with every nested motor part (missing parts empty)', async () => {
    const { vehicles, calls } = services({ 'vehicle.create': () => vehicleRow('new', ME) });
    await vehicles.create(ME, { name: 'Simson', motor: { name: 'M531', hubL: 0.044 } });
    const { data } = callOf(calls, 'vehicle.create') as { data: Record<string, unknown> };
    assert.deepEqual(data, {
      user: { connect: { id: ME } },
      name: 'Simson',
      beschreibung: null,
      gewicht: null,
      motor: {
        create: {
          name: 'M531',
          hubL: 0.044,
          einlass: { create: { vergaser: { create: {} } } },
          auslass: { create: { auspuff: { create: {} } } },
          ueberstroemer: { create: {} },
        },
      },
    });
  });

  it('maps a partial nested PATCH onto nested updates and creates missing parts', async () => {
    const own = vehicleRow('own', ME);
    const { vehicles, calls } = services({
      ...vehicleHandlers([own]),
      'vehicle.update': () => own,
    });
    const input = VehicleUpdateSchema.parse({
      gewicht: null,
      motor: { hubL: 0.05, einlass: { breiteB: 0.012, vergaser: { durchmesserD: null } } },
    });
    await vehicles.update(ME, 'own', input);
    const args = callOf(calls, 'vehicle.update');
    assert.deepEqual(args.where, { id: 'own', userId: ME });
    assert.deepEqual(args.data, {
      gewicht: null,
      motor: {
        update: {
          hubL: 0.05,
          einlass: { update: { breiteB: 0.012, vergaser: { update: { durchmesserD: null } } } },
          // stored auslass has no auspuff, ueberstroemer is missing → created empty
          auslass: { update: { auspuff: { create: {} } } },
          ueberstroemer: { create: {} },
        },
      },
    });
  });

  it('keeps the motor untouched when the PATCH has no motor', async () => {
    const own = vehicleRow('own', ME);
    const { vehicles, calls } = services({
      ...vehicleHandlers([own]),
      'vehicle.update': () => own,
    });
    await vehicles.update(ME, 'own', { name: 'Renamed' });
    const data = callOf(calls, 'vehicle.update').data as Record<string, unknown>;
    assert.equal(data.name, 'Renamed');
    assert.equal(data.motor, undefined);
    assert.equal('gewicht' in data, false);
  });

  it('creates an empty motor when the vehicle has none', async () => {
    const own = vehicleRow('own', ME, { motor: null, motorId: null });
    const { vehicles, calls } = services({
      ...vehicleHandlers([own]),
      'vehicle.update': () => own,
    });
    await vehicles.update(ME, 'own', { beschreibung: 'x' });
    const data = callOf(calls, 'vehicle.update').data as { motor: { create: { name: string } } };
    assert.equal(data.motor.create.name, own.name);
  });

  it('deletes the vehicle and its motor (parts cascade)', async () => {
    const own = vehicleRow('own', ME);
    const { vehicles, calls } = services({
      ...vehicleHandlers([own]),
      'vehicle.delete': () => own,
      'motor.delete': () => own.motor,
    });
    assert.equal(await vehicles.remove(ME, 'own'), null);
    assert.deepEqual(callOf(calls, 'vehicle.delete').where, { id: 'own', userId: ME });
    assert.deepEqual(callOf(calls, 'motor.delete').where, { id: 'own-mot' });
  });

  it('duplicates a starter vehicle deeply into the callers account', async () => {
    const starter = vehicleRow('starter', null, { name: 'S51' });
    const { vehicles, calls } = services({
      ...vehicleHandlers([starter]),
      'vehicle.create': () => vehicleRow('copy', ME),
    });
    await vehicles.duplicate(ME, 'starter');
    const data = callOf(calls, 'vehicle.create').data as {
      user: unknown;
      name: string;
      systemKey?: unknown;
      motor: { create: Record<string, unknown> };
    };
    assert.deepEqual(data.user, { connect: { id: ME } });
    assert.equal(data.name, 'S51 (Kopie)');
    assert.equal('systemKey' in data, false);
    const motor = data.motor.create as {
      hubL: number;
      einlass: { create: { steuerzeitSZ: number; vergaser: { create: unknown } } };
      auslass: { create: { steuerzeitSZ: number; auspuff: { create: unknown } } };
    };
    assert.equal(motor.hubL, 0.04);
    assert.equal(motor.einlass.create.steuerzeitSZ, 130);
    assert.deepEqual(motor.einlass.create.vergaser.create, {
      benzinLuftF: 14.7,
      durchmesserD: 0.016,
    });
    assert.equal(motor.auslass.create.steuerzeitSZ, 180);
    assert.deepEqual(motor.auslass.create.auspuff.create, {});
  });

  it('uses the provided name for duplicates', async () => {
    const own = vehicleRow('own', ME);
    const { vehicles, calls } = services({
      ...vehicleHandlers([own]),
      'vehicle.create': () => own,
    });
    await vehicles.duplicate(ME, 'own', '  Tuned  ');
    assert.equal((callOf(calls, 'vehicle.create').data as { name: string }).name, 'Tuned');
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// Environments
// ─────────────────────────────────────────────────────────────────────────────
describe('EnvironmentService', () => {
  const rows = [
    environmentRow('own', ME),
    environmentRow('foreign', OTHER),
    environmentRow('sys', null),
  ];
  const handlers: Record<string, Handler> = {
    'environment.findFirst': ({ where }) =>
      rows.find((r) => matchesReadable(r, where as ReadableWhere)) ?? null,
    'environment.findMany': ({ where }) => rows.filter((r) => matchesReadable(r, where as never)),
    'environment.delete': () => rows[0],
    'environment.update': () => rows[0],
    'dyno.updateMany': () => ({ count: 2 }),
  };

  it('lists system environments first and hides foreign ones', async () => {
    const { environments } = services(handlers);
    const list = await environments.list(ME);
    assert.deepEqual(
      list.map((e) => e.id),
      ['sys', 'own'],
    );
  });

  it('deleting an environment nulls dyno.environmentId before removing it', async () => {
    const { environments, calls } = services(handlers);
    assert.equal(await environments.remove(ME, 'own'), null);
    assert.deepEqual(ops(calls), [
      'environment.findFirst',
      'dyno.updateMany',
      'environment.delete',
    ]);
    assert.deepEqual(callOf(calls, 'dyno.updateMany'), {
      where: { environmentId: 'own' },
      data: { environmentId: null },
    });
    assert.deepEqual(callOf(calls, 'environment.delete').where, { id: 'own', userId: ME });
  });

  it('404 for foreign environments, 403 for starter environments', async () => {
    const { environments, calls } = services(handlers);
    await assert.rejects(environments.remove(ME, 'foreign'), NotFoundException);
    await assert.rejects(environments.update(ME, 'foreign', { name: 'x' }), NotFoundException);
    await assert.rejects(environments.remove(ME, 'sys'), ForbiddenException);
    assert.deepEqual(new Set(ops(calls)), new Set(['environment.findFirst']));
  });

  it('partial update only writes provided fields', async () => {
    const { environments, calls } = services(handlers);
    await environments.update(ME, 'own', { luftdruckP: null });
    assert.deepEqual(callOf(calls, 'environment.update').data, { luftdruckP: null });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// Dyno
// ─────────────────────────────────────────────────────────────────────────────
describe('DynoService', () => {
  const own = vehicleRow('own', ME, { dyno: { id: 'dyno-own' } });
  const foreign = vehicleRow('foreign', OTHER, { dyno: { id: 'dyno-foreign' } });
  const starter = vehicleRow('starter', null);
  const vehicles = [own, foreign, starter];
  const dynos = [dynoRow('dyno-own', 'own'), dynoRow('dyno-foreign', 'foreign')];
  const envs = [environmentRow('env-own', ME), environmentRow('env-foreign', OTHER)];
  const ownerOf = (vehicleId: string) => vehicles.find((v) => v.id === vehicleId)?.userId;

  const handlers: Record<string, Handler> = {
    ...vehicleHandlers(vehicles),
    'environment.findFirst': ({ where }) =>
      envs.find((r) => matchesReadable(r, where as ReadableWhere)) ?? null,
    'dyno.findFirst': ({ where }) => {
      const w = where as { id: string; vehicle: { userId: string } };
      return dynos.find((d) => d.id === w.id && ownerOf(d.vehicleId) === w.vehicle.userId) ?? null;
    },
    'dyno.findUnique': ({ where }) =>
      dynos.find((d) => d.vehicleId === (where as { vehicleId: string }).vehicleId) ?? null,
    'dyno.upsert': () => dynos[0],
    'dyno.update': () => dynos[0],
    'dyno.delete': () => dynos[0],
    'drehzahl.deleteMany': () => ({ count: 2 }),
    'drehzahl.createMany': ({ data }) => ({ count: (data as unknown[]).length }),
    'ausrollen.deleteMany': () => ({ count: 1 }),
    'vehicle.create': () => vehicleRow('imported', ME, { dyno: { id: 'dyno-imported' } }),
  };

  it('returns null for a starter vehicle without dyno and 404 for foreign vehicles', async () => {
    const { dynos: service } = services(handlers);
    assert.equal(await service.getForVehicle(ME, 'starter'), null);
    assert.equal((await service.getForVehicle(ME, 'own'))?.id, 'dyno-own');
    await assert.rejects(service.getForVehicle(ME, 'foreign'), NotFoundException);
  });

  it('upserts the dyno of an own vehicle', async () => {
    const { dynos: service, calls } = services(handlers);
    await service.save(ME, 'own', { name: 'Run 2', environmentId: 'env-own' });
    const args = callOf(calls, 'dyno.upsert');
    assert.deepEqual(args.where, { vehicleId: 'own' });
    assert.deepEqual(args.create, {
      vehicleId: 'own',
      name: 'Run 2',
      beschreibung: null,
      active: true,
      environmentId: 'env-own',
    });
    assert.deepEqual(args.update, { name: 'Run 2', environmentId: 'env-own' });
  });

  it('refuses dynos on starter (403) and foreign vehicles or environments (404)', async () => {
    const { dynos: service, calls } = services(handlers);
    await assert.rejects(service.save(ME, 'starter', { name: 'x' }), ForbiddenException);
    await assert.rejects(service.save(ME, 'foreign', { name: 'x' }), NotFoundException);
    await assert.rejects(
      service.save(ME, 'own', { name: 'x', environmentId: 'env-foreign' }),
      NotFoundException,
    );
    assert.equal(ops(calls).includes('dyno.upsert'), false);
  });

  it('replaces only the provided series, in order, with positions', async () => {
    const { dynos: service, calls } = services(handlers);
    const data = DynoDataInputSchema.parse({
      drehzahl: [
        { zeit: 0, drehzahl: 2000 },
        { zeit: 0.1, drehzahl: 2500 },
        { zeit: 0.2, drehzahl: 3100 },
      ],
      ausrollen: [],
    });
    await service.saveData(ME, 'dyno-own', data);
    assert.deepEqual(ops(calls), [
      'dyno.findFirst',
      'drehzahl.deleteMany',
      'drehzahl.createMany',
      'ausrollen.deleteMany',
      'dyno.update',
    ]);
    assert.deepEqual(callOf(calls, 'drehzahl.deleteMany'), { where: { dynoId: 'dyno-own' } });
    assert.deepEqual(callOf(calls, 'drehzahl.createMany').data, [
      { position: 0, zeit: 0, drehzahl: 2000, dynoId: 'dyno-own' },
      { position: 1, zeit: 0.1, drehzahl: 2500, dynoId: 'dyno-own' },
      { position: 2, zeit: 0.2, drehzahl: 3100, dynoId: 'dyno-own' },
    ]);
  });

  it('404s series writes, deletes and exports of foreign dynos', async () => {
    const { dynos: service, calls } = services(handlers);
    await assert.rejects(service.saveData(ME, 'dyno-foreign', { drehzahl: [] }), NotFoundException);
    await assert.rejects(service.remove(ME, 'dyno-foreign'), NotFoundException);
    await assert.rejects(service.exportDyno(ME, 'dyno-foreign'), NotFoundException);
    assert.deepEqual(new Set(ops(calls)), new Set(['dyno.findFirst']));
  });

  it('rejects series above MAX_DYNO_POINTS at validation', () => {
    const tooMany = Array.from({ length: MAX_DYNO_POINTS + 1 }, () => ({ drehzahl: 1, ps: 1 }));
    assert.equal(DynoDataInputSchema.safeParse({ dynoPs: tooMany }).success, false);
  });

  it('round-trips export → import into a new owned vehicle with the same data', async () => {
    const { dynos: service, calls } = services(handlers);
    const exported = await service.exportDyno(ME, 'dyno-own');
    // The export must be a valid import payload (what the web/iOS clients upload again).
    const parsed = DynoExportSchema.parse(JSON.parse(JSON.stringify(exported)));
    assert.equal(parsed.dyno.ausrollen?.[0]?.zeitpunkt, AT.toISOString());
    assert.equal('accuracy' in (parsed.dyno.ausrollen?.[0] ?? {}), false);
    assert.deepEqual(parsed.vehicle, JSON.parse(JSON.stringify(vehicleRowToInput(own))));

    const vehicle = await service.importDyno(ME, parsed);
    assert.equal(vehicle.dynoId, 'dyno-imported');

    const data = callOf(calls, 'vehicle.create').data as {
      user: unknown;
      name: string;
      motor: { create: { hubL: number } };
      dyno: { create: Record<string, { createMany: { data: unknown[] } } | unknown> };
    };
    assert.deepEqual(data.user, { connect: { id: ME } });
    assert.equal(data.name, own.name);
    assert.equal(data.motor.create.hubL, own.motor?.hubL);
    const dyno = data.dyno.create as {
      name: string;
      beschreibung: string;
      active: boolean;
      drehzahl: { createMany: { data: unknown[] } };
      dynoPs: { createMany: { data: unknown[] } };
      ausrollen: { createMany: { data: { zeitpunkt: Date; accuracy: null }[] } };
      geschwindigkeit?: unknown;
    };
    assert.equal(dyno.name, 'Run 1');
    assert.equal(dyno.beschreibung, 'Prüfstand');
    assert.deepEqual(dyno.drehzahl.createMany.data, [
      { position: 0, zeit: 0, drehzahl: 1000 },
      { position: 1, zeit: 0.5, drehzahl: 5000 },
    ]);
    assert.deepEqual(dyno.dynoPs.createMany.data, [{ position: 0, drehzahl: 5000, ps: 7.5 }]);
    assert.deepEqual(dyno.ausrollen.createMany.data[0]?.zeitpunkt, AT);
    assert.equal(dyno.ausrollen.createMany.data[0]?.accuracy, null);
    assert.equal(dyno.geschwindigkeit, undefined);

    // Mapping the stored rows back yields the original series.
    const dto = toDynoDto(dynos[0]!);
    assert.deepEqual(
      dto.drehzahl,
      parsed.dyno.drehzahl?.map((p) => ({ zeit: p.zeit, drehzahl: p.drehzahl })),
    );
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// Mapper + request schemas
// ─────────────────────────────────────────────────────────────────────────────
describe('sim mapper and request schemas', () => {
  it('maps vehicle rows to fully nested DTOs with ISO dates', () => {
    const dto = toVehicleDto(vehicleRow('starter', null, { dyno: { id: 'd1' } }));
    assert.equal(dto.isSystem, true);
    assert.equal(dto.dynoId, 'd1');
    assert.equal(dto.createdAt, AT.toISOString());
    assert.equal(dto.motor?.einlass?.vergaser?.benzinLuftF, 14.7);
    assert.equal(dto.motor?.auslass?.auspuff, null);
    assert.equal(dto.motor?.ueberstroemer, null);
    assert.equal('einlassId' in (dto.motor?.einlass?.vergaser ?? {}), false);

    const own = toVehicleDto(vehicleRow('own', ME));
    assert.equal(own.isSystem, false);
    assert.equal(own.dynoId, null);
  });

  it('validates route ids and partial vehicle updates', () => {
    assert.equal(IdParamSchema.safeParse('').success, false);
    assert.equal(IdParamSchema.safeParse('a'.repeat(65)).success, false);
    assert.equal(IdParamSchema.safeParse('6f1c0a8e-1d2b-4c3d-9e8f-001122334455').success, true);
    assert.equal(VehicleUpdateSchema.safeParse({ motor: { hubL: 1 } }).success, true);
    assert.equal(VehicleUpdateSchema.safeParse({ name: '' }).success, false);
  });
});
