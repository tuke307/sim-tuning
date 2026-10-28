import Foundation

/// Starter content ported from `packages/common/src/catalog/index.ts` (v2 EF Core seed). Rows are
/// identified by a stable `systemKey`; they are read-only templates in guest mode (ids
/// `sys-<systemKey>`). Values are in field base units (mm, mm³, kg, °KW, U/min, °C, hPa).
public enum StarterCatalog {
    public struct Environment: Sendable, Equatable {
        public let systemKey: String
        public let name: String
        public let temperaturT: Double
        public let luftdruckP: Double
    }

    public struct Motor: Sendable, Equatable {
        public let name: String
        public let hubL: Double
        public let bohrungD: Double
        public let pleulL: Double?
        public let deachsierungL: Double?
        public let resonanzU: Double
        public let hubraumV: Double
        public let brennraumV: Double?
        public let kurbelgehaeuseV: Double
        public let einlassSteuerzeitSZ: Double?
        public let auslassSteuerzeitSZ: Double?
        public let ueberstroemerSteuerzeitSZ: Double?
    }

    public struct Vehicle: Sendable, Equatable {
        public let systemKey: String
        public let name: String
        public let beschreibung: String
        public let gewicht: Double
        public let motor: Motor
    }

    public static let environments: [Environment] = [
        Environment(systemKey: "env-fruehling", name: "Frühling", temperaturT: 10, luftdruckP: 1010),
        Environment(systemKey: "env-sommer", name: "Sommer", temperaturT: 25, luftdruckP: 1010),
        Environment(systemKey: "env-herbst", name: "Herbst", temperaturT: 10, luftdruckP: 1010),
        Environment(systemKey: "env-winter", name: "Winter", temperaturT: 1, luftdruckP: 1010),
    ]

    public static let vehicles: [Vehicle] = [
        Vehicle(systemKey: "veh-sr2e", name: "SR 2 E", beschreibung: "Baujahre: 1960 - 1964\nStückzahl: 515.000",
                gewicht: 53,
                motor: Motor(name: "Rh 50 II", hubL: 42, bohrungD: 38, pleulL: nil, deachsierungL: nil,
                             resonanzU: 5000, hubraumV: 47600, brennraumV: nil, kurbelgehaeuseV: 142_800,
                             einlassSteuerzeitSZ: nil, auslassSteuerzeitSZ: nil, ueberstroemerSteuerzeitSZ: nil)),
        Vehicle(systemKey: "veh-kr50", name: "KR 50", beschreibung: "Baujahre: 1959 - 1964\nStückzahl: 164.500",
                gewicht: 73,
                motor: Motor(name: "KRo Rh 50", hubL: 42, bohrungD: 38, pleulL: nil, deachsierungL: nil,
                             resonanzU: 5500, hubraumV: 47600, brennraumV: nil, kurbelgehaeuseV: 142_800,
                             einlassSteuerzeitSZ: nil, auslassSteuerzeitSZ: nil, ueberstroemerSteuerzeitSZ: nil)),
        Vehicle(systemKey: "veh-kr51-1", name: "KR 51/1 (F)",
                beschreibung: "Baujahre: 1968 - 1980\nStückzahl: 375.000", gewicht: 80,
                motor: Motor(name: "M53/1 KFR", hubL: 39.5, bohrungD: 40, pleulL: nil, deachsierungL: nil,
                             resonanzU: 5750, hubraumV: 49600, brennraumV: nil, kurbelgehaeuseV: 148_800,
                             einlassSteuerzeitSZ: 136, auslassSteuerzeitSZ: 148, ueberstroemerSteuerzeitSZ: 122)),
        Vehicle(systemKey: "veh-s50b1", name: "S 50 B1", beschreibung: "Baujahre: 1976 - 1980\nStückzahl: 287.000",
                gewicht: 81,
                motor: Motor(name: "M 53/21 KF", hubL: 39.5, bohrungD: 40, pleulL: nil, deachsierungL: nil,
                             resonanzU: 5500, hubraumV: 49600, brennraumV: nil, kurbelgehaeuseV: 148_800,
                             einlassSteuerzeitSZ: nil, auslassSteuerzeitSZ: nil, ueberstroemerSteuerzeitSZ: nil)),
        Vehicle(systemKey: "veh-s51b1-4", name: "S51 B1-4",
                beschreibung: "Baujahre: 1980 - 1989\nStückzahl: 360.600", gewicht: 79.5,
                motor: Motor(name: "M 541 KF", hubL: 44, bohrungD: 38, pleulL: 95, deachsierungL: 2,
                             resonanzU: 5500, hubraumV: 49900, brennraumV: 5880, kurbelgehaeuseV: 149_700,
                             einlassSteuerzeitSZ: 135, auslassSteuerzeitSZ: 145, ueberstroemerSteuerzeitSZ: 117)),
        Vehicle(systemKey: "veh-s70c", name: "S 70 C", beschreibung: "Baujahre: 1984 - 1988\nStückzahl: 20.000",
                gewicht: 84,
                motor: Motor(name: "M 741/1 KF", hubL: 44, bohrungD: 45, pleulL: nil, deachsierungL: nil,
                             resonanzU: 6000, hubraumV: 49900, brennraumV: nil, kurbelgehaeuseV: 209_700,
                             einlassSteuerzeitSZ: nil, auslassSteuerzeitSZ: nil, ueberstroemerSteuerzeitSZ: nil)),
    ]
}
