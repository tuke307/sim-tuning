import Foundation

// Codable mirror of `packages/common/src/schemas/sim.ts`. Every numeric field is stored in its
// base unit (`FieldBaseUnits`); the UI converts for display only.
//
// Input payloads use double optionals for nullable fields to keep the TS `undefined` vs `null`
// distinction of partial updates: `nil` = not provided (key omitted), `.some(nil)` = clear (JSON
// `null`), `.some(value)` = set.

public typealias Patch<T> = T??

// MARK: - Inputs

public struct VergaserInputDto: Codable, Sendable, Equatable {
    public var benzinLuftF: Double??
    public var durchmesserD: Double??

    public init(benzinLuftF: Double?? = nil, durchmesserD: Double?? = nil) {
        self.benzinLuftF = benzinLuftF
        self.durchmesserD = durchmesserD
    }
}

public struct EinlassInputDto: Codable, Sendable, Equatable {
    public var breiteB: Double??
    public var durchmesserD: Double??
    public var flaecheA: Double??
    public var hoeheH: Double??
    public var laengeL: Double??
    public var luftBedarf: Double??
    public var steuerzeitSZ: Double??
    public var vergaser: VergaserInputDto?

    public init(breiteB: Double?? = nil, durchmesserD: Double?? = nil, flaecheA: Double?? = nil,
                hoeheH: Double?? = nil, laengeL: Double?? = nil, luftBedarf: Double?? = nil,
                steuerzeitSZ: Double?? = nil, vergaser: VergaserInputDto? = nil) {
        self.breiteB = breiteB
        self.durchmesserD = durchmesserD
        self.flaecheA = flaecheA
        self.hoeheH = hoeheH
        self.laengeL = laengeL
        self.luftBedarf = luftBedarf
        self.steuerzeitSZ = steuerzeitSZ
        self.vergaser = vergaser
    }
}

public struct AuspuffInputDto: Codable, Sendable, Equatable {
    public var abgasT: Double??
    public var abgasV: Double??
    public var diffusorD: Double??
    public var diffusorD1: Double??
    public var diffusorD2: Double??
    public var diffusorD3: Double??
    public var diffusorL: Double??
    public var diffusorL1: Double??
    public var diffusorL2: Double??
    public var diffusorL3: Double??
    public var diffusorStage: Int?
    public var diffusorW: Double??
    public var diffusorW1: Double??
    public var diffusorW2: Double??
    public var diffusorW3: Double??
    public var endrohrD: Double??
    public var endrohrL: Double??
    public var gegenkonusD: Double??
    public var gegenkonusL: Double??
    public var gegenKonusW: Double??
    public var gesamtL: Double??
    public var kruemmerD: Double??
    public var kruemmerF: Double??
    public var kruemmerL: Double??
    public var kruemmerW: Double??
    public var mittelteilD: Double??
    public var mittelteilF: Double??
    public var mittelteilL: Double??
    public var resonanzL: Double??

    public init() {}
}

public struct AuslassInputDto: Codable, Sendable, Equatable {
    public var breiteB: Double??
    public var durchmesserD: Double??
    public var flaecheA: Double??
    public var hoeheH: Double??
    public var laengeL: Double??
    public var steuerzeitSZ: Double??
    public var auspuff: AuspuffInputDto?

    public init(breiteB: Double?? = nil, durchmesserD: Double?? = nil, flaecheA: Double?? = nil,
                hoeheH: Double?? = nil, laengeL: Double?? = nil, steuerzeitSZ: Double?? = nil,
                auspuff: AuspuffInputDto? = nil) {
        self.breiteB = breiteB
        self.durchmesserD = durchmesserD
        self.flaecheA = flaecheA
        self.hoeheH = hoeheH
        self.laengeL = laengeL
        self.steuerzeitSZ = steuerzeitSZ
        self.auspuff = auspuff
    }
}

public struct UeberstroemerInputDto: Codable, Sendable, Equatable {
    public var anzahl: Double??
    public var breiteB: Double??
    public var flaecheA: Double??
    public var hoeheH: Double??
    public var steuerzeitSZ: Double??

    public init(anzahl: Double?? = nil, breiteB: Double?? = nil, flaecheA: Double?? = nil,
                hoeheH: Double?? = nil, steuerzeitSZ: Double?? = nil) {
        self.anzahl = anzahl
        self.breiteB = breiteB
        self.flaecheA = flaecheA
        self.hoeheH = hoeheH
        self.steuerzeitSZ = steuerzeitSZ
    }
}

/// `MotorInputSchema`. `name` is required on create; partial updates may omit it.
public struct MotorInputDto: Codable, Sendable, Equatable {
    public var name: String?
    public var bohrungD: Double??
    public var brennraumV: Double??
    public var deachsierungL: Double??
    public var heizwertU: Double??
    public var hubL: Double??
    public var hubraumV: Double??
    public var kolbenG: Double??
    public var kurbelgehaeuseV: Double??
    public var pleulL: Double??
    public var resonanzU: Double??
    public var verdichtungV: Double??
    public var zuendzeitpunkt: Double??
    public var zylinderAnz: Double??
    public var einlass: EinlassInputDto?
    public var auslass: AuslassInputDto?
    public var ueberstroemer: UeberstroemerInputDto?

    public init(name: String? = nil) {
        self.name = name
    }
}

/// `VehicleInputSchema`. `name` is required on create; partial updates may omit it.
public struct VehicleInputDto: Codable, Sendable, Equatable {
    public var name: String?
    public var beschreibung: String??
    public var gewicht: Double??
    public var motor: MotorInputDto?

    public init(name: String? = nil, beschreibung: String?? = nil, gewicht: Double?? = nil,
                motor: MotorInputDto? = nil) {
        self.name = name
        self.beschreibung = beschreibung
        self.gewicht = gewicht
        self.motor = motor
    }
}

public struct EnvironmentInputDto: Codable, Sendable, Equatable {
    public var name: String?
    public var temperaturT: Double??
    public var luftdruckP: Double??

    public init(name: String? = nil, temperaturT: Double?? = nil, luftdruckP: Double?? = nil) {
        self.name = name
        self.temperaturT = temperaturT
        self.luftdruckP = luftdruckP
    }
}

public struct DynoInputDto: Codable, Sendable, Equatable {
    public var name: String
    public var beschreibung: String??
    public var active: Bool?
    public var environmentId: String??

    public init(name: String, beschreibung: String?? = nil, active: Bool? = nil, environmentId: String?? = nil) {
        self.name = name
        self.beschreibung = beschreibung
        self.active = active
        self.environmentId = environmentId
    }
}

public let maxDynoPoints = 20000

public struct DrehzahlPointDto: Codable, Sendable, Equatable, Hashable {
    /// Time in ms.
    public var zeit: Double
    /// Engine speed in U/min.
    public var drehzahl: Double

    public init(zeit: Double, drehzahl: Double) {
        self.zeit = zeit
        self.drehzahl = drehzahl
    }
}

public struct DynoPsPointDto: Codable, Sendable, Equatable, Hashable {
    public var drehzahl: Double
    public var ps: Double

    public init(drehzahl: Double, ps: Double) {
        self.drehzahl = drehzahl
        self.ps = ps
    }
}

/// `GpsSampleSchema` (input) and `GpsSampleResponseDto` (all nullable). `nil` values are omitted
/// when encoding, which matches the export format (`definedGps`).
public struct GpsSampleDto: Codable, Sendable, Equatable, Hashable {
    public var latitude: Double
    public var longitude: Double
    public var speed: Double?
    public var accuracy: Double?
    public var altitude: Double?
    public var altitudeAccuracy: Double?
    public var heading: Double?
    public var headingAccuracy: Double?
    /// ISO 8601 date-time.
    public var zeitpunkt: String?

    public init(latitude: Double, longitude: Double, speed: Double? = nil, accuracy: Double? = nil,
                altitude: Double? = nil, altitudeAccuracy: Double? = nil, heading: Double? = nil,
                headingAccuracy: Double? = nil, zeitpunkt: String? = nil) {
        self.latitude = latitude
        self.longitude = longitude
        self.speed = speed
        self.accuracy = accuracy
        self.altitude = altitude
        self.altitudeAccuracy = altitudeAccuracy
        self.heading = heading
        self.headingAccuracy = headingAccuracy
        self.zeitpunkt = zeitpunkt
    }
}

public struct DynoDataInputDto: Codable, Sendable, Equatable {
    public var drehzahl: [DrehzahlPointDto]?
    public var dynoPs: [DynoPsPointDto]?
    public var ausrollen: [GpsSampleDto]?
    public var geschwindigkeit: [GpsSampleDto]?

    public init(drehzahl: [DrehzahlPointDto]? = nil, dynoPs: [DynoPsPointDto]? = nil,
                ausrollen: [GpsSampleDto]? = nil, geschwindigkeit: [GpsSampleDto]? = nil) {
        self.drehzahl = drehzahl
        self.dynoPs = dynoPs
        self.ausrollen = ausrollen
        self.geschwindigkeit = geschwindigkeit
    }
}

/// Portable dyno export (`DynoExportSchema`, replaces the v2 ZIP with `DataExport.json`).
public let dynoExportFormat = "sim-tuning-dyno"

public struct DynoExportDto: Codable, Sendable, Equatable {
    public struct Dyno: Codable, Sendable, Equatable {
        public var name: String
        public var beschreibung: String??
        public var active: Bool?
        public var drehzahl: [DrehzahlPointDto]?
        public var dynoPs: [DynoPsPointDto]?
        public var ausrollen: [GpsSampleDto]?
        public var geschwindigkeit: [GpsSampleDto]?

        public init(name: String, beschreibung: String?? = nil, active: Bool? = nil,
                    drehzahl: [DrehzahlPointDto]? = nil, dynoPs: [DynoPsPointDto]? = nil,
                    ausrollen: [GpsSampleDto]? = nil, geschwindigkeit: [GpsSampleDto]? = nil) {
            self.name = name
            self.beschreibung = beschreibung
            self.active = active
            self.drehzahl = drehzahl
            self.dynoPs = dynoPs
            self.ausrollen = ausrollen
            self.geschwindigkeit = geschwindigkeit
        }
    }

    public var format: String
    public var version: Int
    public var exportedAt: String
    public var vehicle: VehicleInputDto
    public var dyno: Dyno

    public init(format: String = dynoExportFormat, version: Int = 3, exportedAt: String,
                vehicle: VehicleInputDto, dyno: Dyno) {
        self.format = format
        self.version = version
        self.exportedAt = exportedAt
        self.vehicle = vehicle
        self.dyno = dyno
    }
}

// MARK: - Responses

public struct VergaserDto: Codable, Sendable, Equatable, Hashable {
    public var id: String
    public var createdAt: String
    public var updatedAt: String
    public var benzinLuftF: Double?
    public var durchmesserD: Double?
}

public struct EinlassDto: Codable, Sendable, Equatable, Hashable {
    public var id: String
    public var createdAt: String
    public var updatedAt: String
    public var breiteB: Double?
    public var durchmesserD: Double?
    public var flaecheA: Double?
    public var hoeheH: Double?
    public var laengeL: Double?
    public var luftBedarf: Double?
    public var steuerzeitSZ: Double?
    public var vergaser: VergaserDto?
}

public struct AuspuffDto: Codable, Sendable, Equatable, Hashable {
    public var id: String
    public var createdAt: String
    public var updatedAt: String
    public var abgasT: Double?
    public var abgasV: Double?
    public var diffusorD: Double?
    public var diffusorD1: Double?
    public var diffusorD2: Double?
    public var diffusorD3: Double?
    public var diffusorL: Double?
    public var diffusorL1: Double?
    public var diffusorL2: Double?
    public var diffusorL3: Double?
    public var diffusorStage: Int
    public var diffusorW: Double?
    public var diffusorW1: Double?
    public var diffusorW2: Double?
    public var diffusorW3: Double?
    public var endrohrD: Double?
    public var endrohrL: Double?
    public var gegenkonusD: Double?
    public var gegenkonusL: Double?
    public var gegenKonusW: Double?
    public var gesamtL: Double?
    public var kruemmerD: Double?
    public var kruemmerF: Double?
    public var kruemmerL: Double?
    public var kruemmerW: Double?
    public var mittelteilD: Double?
    public var mittelteilF: Double?
    public var mittelteilL: Double?
    public var resonanzL: Double?
}

public struct AuslassDto: Codable, Sendable, Equatable, Hashable {
    public var id: String
    public var createdAt: String
    public var updatedAt: String
    public var breiteB: Double?
    public var durchmesserD: Double?
    public var flaecheA: Double?
    public var hoeheH: Double?
    public var laengeL: Double?
    public var steuerzeitSZ: Double?
    public var auspuff: AuspuffDto?
}

public struct UeberstroemerDto: Codable, Sendable, Equatable, Hashable {
    public var id: String
    public var createdAt: String
    public var updatedAt: String
    public var anzahl: Double?
    public var breiteB: Double?
    public var flaecheA: Double?
    public var hoeheH: Double?
    public var steuerzeitSZ: Double?
}

public struct MotorDto: Codable, Sendable, Equatable, Hashable {
    public var id: String
    public var createdAt: String
    public var updatedAt: String
    public var name: String
    public var bohrungD: Double?
    public var brennraumV: Double?
    public var deachsierungL: Double?
    public var heizwertU: Double?
    public var hubL: Double?
    public var hubraumV: Double?
    public var kolbenG: Double?
    public var kurbelgehaeuseV: Double?
    public var pleulL: Double?
    public var resonanzU: Double?
    public var verdichtungV: Double?
    public var zuendzeitpunkt: Double?
    public var zylinderAnz: Double?
    public var einlass: EinlassDto?
    public var auslass: AuslassDto?
    public var ueberstroemer: UeberstroemerDto?
}

public struct VehicleDto: Codable, Sendable, Equatable, Hashable, Identifiable {
    public var id: String
    public var createdAt: String
    public var updatedAt: String
    /// Set for shared starter vehicles (read-only, can be duplicated).
    public var systemKey: String?
    public var isSystem: Bool
    public var name: String
    public var beschreibung: String?
    public var gewicht: Double?
    public var motor: MotorDto?
    public var dynoId: String?
}

public struct EnvironmentDto: Codable, Sendable, Equatable, Hashable, Identifiable {
    public var id: String
    public var createdAt: String
    public var updatedAt: String
    public var systemKey: String?
    public var isSystem: Bool
    public var name: String
    public var temperaturT: Double?
    public var luftdruckP: Double?

    public init(id: String, createdAt: String, updatedAt: String, systemKey: String?, isSystem: Bool,
                name: String, temperaturT: Double?, luftdruckP: Double?) {
        self.id = id
        self.createdAt = createdAt
        self.updatedAt = updatedAt
        self.systemKey = systemKey
        self.isSystem = isSystem
        self.name = name
        self.temperaturT = temperaturT
        self.luftdruckP = luftdruckP
    }
}

public struct DynoDto: Codable, Sendable, Equatable, Hashable, Identifiable {
    public var id: String
    public var createdAt: String
    public var updatedAt: String
    public var vehicleId: String
    public var environmentId: String?
    public var name: String
    public var beschreibung: String?
    public var active: Bool
    public var drehzahl: [DrehzahlPointDto]
    public var dynoPs: [DynoPsPointDto]
    public var ausrollen: [GpsSampleDto]
    public var geschwindigkeit: [GpsSampleDto]

    public init(id: String, createdAt: String, updatedAt: String, vehicleId: String, environmentId: String?,
                name: String, beschreibung: String?, active: Bool, drehzahl: [DrehzahlPointDto] = [],
                dynoPs: [DynoPsPointDto] = [], ausrollen: [GpsSampleDto] = [], geschwindigkeit: [GpsSampleDto] = []) {
        self.id = id
        self.createdAt = createdAt
        self.updatedAt = updatedAt
        self.vehicleId = vehicleId
        self.environmentId = environmentId
        self.name = name
        self.beschreibung = beschreibung
        self.active = active
        self.drehzahl = drehzahl
        self.dynoPs = dynoPs
        self.ausrollen = ausrollen
        self.geschwindigkeit = geschwindigkeit
    }
}
