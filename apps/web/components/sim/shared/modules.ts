/** The five modules of the app, in sheet order ("Blatt n" of the Werkstattzeichnung). */
export type SimModule = 'vehicles' | 'motor' | 'einlass' | 'auslass' | 'dyno';

export interface ModuleMeta {
  /** Sheet number shown in the Schriftfeld and the Blattverzeichnis. */
  sheet: number;
  /** CSS custom property of the module colour (Simson enamel colour). */
  colorVar: string;
  /** Literal Tailwind classes so the scanner picks them up. */
  bg: string;
  text: string;
  border: string;
}

export const MODULE_META: Record<SimModule, ModuleMeta> = {
  vehicles: {
    sheet: 1,
    colorVar: 'var(--mod-vehicles)',
    bg: 'bg-mod-vehicles',
    text: 'text-mod-vehicles',
    border: 'border-mod-vehicles',
  },
  motor: {
    sheet: 2,
    colorVar: 'var(--mod-motor)',
    bg: 'bg-mod-motor',
    text: 'text-mod-motor',
    border: 'border-mod-motor',
  },
  einlass: {
    sheet: 3,
    colorVar: 'var(--mod-einlass)',
    bg: 'bg-mod-einlass',
    text: 'text-mod-einlass',
    border: 'border-mod-einlass',
  },
  auslass: {
    sheet: 4,
    colorVar: 'var(--mod-auslass)',
    bg: 'bg-mod-auslass',
    text: 'text-mod-auslass',
    border: 'border-mod-auslass',
  },
  dyno: {
    sheet: 5,
    colorVar: 'var(--mod-dyno)',
    bg: 'bg-mod-dyno',
    text: 'text-mod-dyno',
    border: 'border-mod-dyno',
  },
};

export const SIM_MODULES: SimModule[] = ['vehicles', 'motor', 'einlass', 'auslass', 'dyno'];
