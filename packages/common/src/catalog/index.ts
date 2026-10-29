/**
 * Starter content ported from the v2 EF Core seed (`DatabaseContext.OnModelCreating`).
 * Rows are identified by a stable `systemKey`; they are shared, read-only templates in the API
 * and the initial data of the offline guest mode. Values are in field base units (mm, mm³, kg,
 * °KW, U/min, °C, hPa).
 */

export interface StarterEnvironment {
  systemKey: string;
  name: string;
  temperaturT: number;
  luftdruckP: number;
}

export const STARTER_ENVIRONMENTS: readonly StarterEnvironment[] = [
  { systemKey: 'env-fruehling', name: 'Frühling', temperaturT: 10, luftdruckP: 1010 },
  { systemKey: 'env-sommer', name: 'Sommer', temperaturT: 25, luftdruckP: 1010 },
  { systemKey: 'env-herbst', name: 'Herbst', temperaturT: 10, luftdruckP: 1010 },
  { systemKey: 'env-winter', name: 'Winter', temperaturT: 1, luftdruckP: 1010 },
];

export interface StarterVehicle {
  systemKey: string;
  name: string;
  beschreibung: string;
  gewicht: number;
  motor: {
    name: string;
    hubL: number;
    bohrungD: number;
    pleulL: number | null;
    deachsierungL: number | null;
    resonanzU: number;
    hubraumV: number;
    brennraumV: number | null;
    kurbelgehaeuseV: number;
    einlassSteuerzeitSZ: number | null;
    auslassSteuerzeitSZ: number | null;
    ueberstroemerSteuerzeitSZ: number | null;
  };
}

export const STARTER_VEHICLES: readonly StarterVehicle[] = [
  {
    systemKey: 'veh-sr2e',
    name: 'SR 2 E',
    beschreibung: 'Baujahre: 1960 - 1964\nStückzahl: 515.000',
    gewicht: 53,
    motor: {
      name: 'Rh 50 II',
      hubL: 42,
      bohrungD: 38,
      pleulL: null,
      deachsierungL: null,
      resonanzU: 5000,
      hubraumV: 47600,
      brennraumV: null,
      kurbelgehaeuseV: 142800,
      einlassSteuerzeitSZ: null,
      auslassSteuerzeitSZ: null,
      ueberstroemerSteuerzeitSZ: null,
    },
  },
  {
    systemKey: 'veh-kr50',
    name: 'KR 50',
    beschreibung: 'Baujahre: 1959 - 1964\nStückzahl: 164.500',
    gewicht: 73,
    motor: {
      name: 'KRo Rh 50',
      hubL: 42,
      bohrungD: 38,
      pleulL: null,
      deachsierungL: null,
      resonanzU: 5500,
      hubraumV: 47600,
      brennraumV: null,
      kurbelgehaeuseV: 142800,
      einlassSteuerzeitSZ: null,
      auslassSteuerzeitSZ: null,
      ueberstroemerSteuerzeitSZ: null,
    },
  },
  {
    systemKey: 'veh-kr51-1',
    name: 'KR 51/1 (F)',
    beschreibung: 'Baujahre: 1968 - 1980\nStückzahl: 375.000',
    gewicht: 80,
    motor: {
      name: 'M53/1 KFR',
      hubL: 39.5,
      bohrungD: 40,
      pleulL: null,
      deachsierungL: null,
      resonanzU: 5750,
      hubraumV: 49600,
      brennraumV: null,
      kurbelgehaeuseV: 148800,
      einlassSteuerzeitSZ: 136,
      auslassSteuerzeitSZ: 148,
      ueberstroemerSteuerzeitSZ: 122,
    },
  },
  {
    systemKey: 'veh-s50b1',
    name: 'S 50 B1',
    beschreibung: 'Baujahre: 1976 - 1980\nStückzahl: 287.000',
    gewicht: 81,
    motor: {
      name: 'M 53/21 KF',
      hubL: 39.5,
      bohrungD: 40,
      pleulL: null,
      deachsierungL: null,
      resonanzU: 5500,
      hubraumV: 49600,
      brennraumV: null,
      kurbelgehaeuseV: 148800,
      einlassSteuerzeitSZ: null,
      auslassSteuerzeitSZ: null,
      ueberstroemerSteuerzeitSZ: null,
    },
  },
  {
    systemKey: 'veh-s51b1-4',
    name: 'S51 B1-4',
    beschreibung: 'Baujahre: 1980 - 1989\nStückzahl: 360.600',
    gewicht: 79.5,
    motor: {
      name: 'M 541 KF',
      hubL: 44,
      bohrungD: 38,
      pleulL: 95,
      deachsierungL: 2,
      resonanzU: 5500,
      hubraumV: 49900,
      brennraumV: 5880,
      kurbelgehaeuseV: 149700,
      einlassSteuerzeitSZ: 135,
      auslassSteuerzeitSZ: 145,
      ueberstroemerSteuerzeitSZ: 117,
    },
  },
  {
    systemKey: 'veh-s70c',
    name: 'S 70 C',
    beschreibung: 'Baujahre: 1984 - 1988\nStückzahl: 20.000',
    gewicht: 84,
    motor: {
      name: 'M 741/1 KF',
      hubL: 44,
      bohrungD: 45,
      pleulL: null,
      deachsierungL: null,
      resonanzU: 6000,
      hubraumV: 49900,
      brennraumV: null,
      kurbelgehaeuseV: 209700,
      einlassSteuerzeitSZ: null,
      auslassSteuerzeitSZ: null,
      ueberstroemerSteuerzeitSZ: null,
    },
  },
];
