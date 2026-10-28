import type { UnitId } from '@sim-tuning/common/units';
import { FIELD_BASE_UNITS } from '@sim-tuning/common/units';
import type { VehicleDto, VehicleInputDto } from '@sim-tuning/common/schemas';

/**
 * Editable master data of a vehicle (v2 VehiclesView "Daten" tab) and the pure helpers that turn
 * a stored vehicle into a form draft and the draft back into a partial update payload.
 */

export interface FieldSpec<K extends string> {
  key: K;
  /** Storage unit (`FIELD_BASE_UNITS`) for unit-aware fields. */
  baseUnit?: UnitId;
  /** Fixed suffix for unit-less fields (°KW, U/min…). */
  suffix?: string;
}

export const MOTOR_FIELDS = [
  { key: 'hubL', baseUnit: FIELD_BASE_UNITS.motor.hubL },
  { key: 'bohrungD', baseUnit: FIELD_BASE_UNITS.motor.bohrungD },
  { key: 'pleulL', baseUnit: FIELD_BASE_UNITS.motor.pleulL },
  { key: 'deachsierungL', baseUnit: FIELD_BASE_UNITS.motor.deachsierungL },
  { key: 'hubraumV', baseUnit: FIELD_BASE_UNITS.motor.hubraumV },
  { key: 'brennraumV', baseUnit: FIELD_BASE_UNITS.motor.brennraumV },
  { key: 'kurbelgehaeuseV', baseUnit: FIELD_BASE_UNITS.motor.kurbelgehaeuseV },
  { key: 'resonanzU', suffix: 'U/min' },
  { key: 'zylinderAnz' },
  { key: 'verdichtungV', suffix: ': 1' },
  { key: 'zuendzeitpunkt', suffix: '°KW' },
  { key: 'heizwertU' },
] as const satisfies readonly FieldSpec<string>[];

export const EINLASS_FIELDS = [
  { key: 'steuerzeitSZ', suffix: '°KW' },
  { key: 'breiteB', baseUnit: FIELD_BASE_UNITS.einlass.breiteB },
  { key: 'hoeheH', baseUnit: FIELD_BASE_UNITS.einlass.hoeheH },
  { key: 'flaecheA', baseUnit: FIELD_BASE_UNITS.einlass.flaecheA },
  { key: 'laengeL', baseUnit: FIELD_BASE_UNITS.einlass.laengeL },
  { key: 'durchmesserD', baseUnit: FIELD_BASE_UNITS.einlass.durchmesserD },
] as const satisfies readonly FieldSpec<string>[];

export const AUSLASS_FIELDS = [
  { key: 'steuerzeitSZ', suffix: '°KW' },
  { key: 'breiteB', baseUnit: FIELD_BASE_UNITS.auslass.breiteB },
  { key: 'hoeheH', baseUnit: FIELD_BASE_UNITS.auslass.hoeheH },
  { key: 'flaecheA', baseUnit: FIELD_BASE_UNITS.auslass.flaecheA },
  { key: 'laengeL', baseUnit: FIELD_BASE_UNITS.auslass.laengeL },
  { key: 'durchmesserD', baseUnit: FIELD_BASE_UNITS.auslass.durchmesserD },
] as const satisfies readonly FieldSpec<string>[];

export const UEBERSTROEMER_FIELDS = [
  { key: 'steuerzeitSZ', suffix: '°KW' },
  { key: 'anzahl' },
  { key: 'breiteB', baseUnit: FIELD_BASE_UNITS.ueberstroemer.breiteB },
  { key: 'hoeheH', baseUnit: FIELD_BASE_UNITS.ueberstroemer.hoeheH },
  { key: 'flaecheA', baseUnit: FIELD_BASE_UNITS.ueberstroemer.flaecheA },
] as const satisfies readonly FieldSpec<string>[];

export type MotorFieldKey = (typeof MOTOR_FIELDS)[number]['key'];
export type EinlassFieldKey = (typeof EINLASS_FIELDS)[number]['key'];
export type AuslassFieldKey = (typeof AUSLASS_FIELDS)[number]['key'];
export type UeberstroemerFieldKey = (typeof UEBERSTROEMER_FIELDS)[number]['key'];

type NumRecord<K extends string> = Record<K, number | null>;

export interface VehicleDraft {
  name: string;
  beschreibung: string;
  gewicht: number | null;
  motorName: string;
  motor: NumRecord<MotorFieldKey>;
  einlass: NumRecord<EinlassFieldKey>;
  auslass: NumRecord<AuslassFieldKey>;
  ueberstroemer: NumRecord<UeberstroemerFieldKey>;
}

export type DraftSection = 'motor' | 'einlass' | 'auslass' | 'ueberstroemer';

function pickNums<K extends string>(
  source: object | null | undefined,
  fields: readonly { key: K }[],
): NumRecord<K> {
  const record = (source ?? {}) as Record<string, unknown>;
  return Object.fromEntries(
    fields.map(({ key }) => {
      const value = record[key];
      return [key, typeof value === 'number' && Number.isFinite(value) ? value : null];
    }),
  ) as NumRecord<K>;
}

export function vehicleToDraft(vehicle: VehicleDto): VehicleDraft {
  const motor = vehicle.motor;
  return {
    name: vehicle.name,
    beschreibung: vehicle.beschreibung ?? '',
    gewicht: vehicle.gewicht,
    motorName: motor?.name ?? vehicle.name,
    motor: pickNums(motor, MOTOR_FIELDS),
    einlass: pickNums(motor?.einlass, EINLASS_FIELDS),
    auslass: pickNums(motor?.auslass, AUSLASS_FIELDS),
    ueberstroemer: pickNums(motor?.ueberstroemer, UEBERSTROEMER_FIELDS),
  };
}

/** Immutable update of one numeric field inside a nested section. */
export function setDraftField<S extends DraftSection>(
  draft: VehicleDraft,
  section: S,
  key: keyof VehicleDraft[S],
  value: number | null,
): VehicleDraft {
  return { ...draft, [section]: { ...draft[section], [key]: value } };
}

export type DraftError = 'nameRequired' | 'motorNameRequired';

export function validateDraft(draft: VehicleDraft): DraftError | null {
  if (!draft.name.trim()) return 'nameRequired';
  if (!draft.motorName.trim()) return 'motorNameRequired';
  return null;
}

/** Converts a draft into the partial update payload of `SimRepository.updateVehicle`. */
export function draftToInput(draft: VehicleDraft): Partial<VehicleInputDto> {
  const beschreibung = draft.beschreibung.trim();
  return {
    name: draft.name.trim(),
    beschreibung: beschreibung === '' ? null : beschreibung,
    gewicht: draft.gewicht,
    motor: {
      name: draft.motorName.trim(),
      ...draft.motor,
      einlass: { ...draft.einlass },
      auslass: { ...draft.auslass },
      ueberstroemer: { ...draft.ueberstroemer },
    },
  };
}

/** True when the draft differs from the stored vehicle (ignores whitespace-only edits). */
export function isDraftDirty(draft: VehicleDraft, vehicle: VehicleDto): boolean {
  return (
    JSON.stringify(draftToInput(draft)) !== JSON.stringify(draftToInput(vehicleToDraft(vehicle)))
  );
}

/** Payload for creating a vehicle from the "new vehicle" dialog (v2 VehiclePopup). */
export function newVehicleInput(input: {
  name: string;
  beschreibung?: string;
  gewicht?: number | null;
  motorName?: string;
}): VehicleInputDto {
  const name = input.name.trim();
  const beschreibung = input.beschreibung?.trim();
  return {
    name,
    beschreibung: beschreibung ? beschreibung : null,
    gewicht: input.gewicht ?? null,
    motor: { name: input.motorName?.trim() || name },
  };
}

/** Replaces `{name}`-style placeholders in translated strings. */
export function interpolate(template: string, values: Record<string, string | number>): string {
  return template.replace(/\{(\w+)\}/g, (match, key: string) =>
    key in values ? String(values[key]) : match,
  );
}
