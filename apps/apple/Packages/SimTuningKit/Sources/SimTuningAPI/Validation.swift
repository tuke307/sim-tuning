import Foundation

/// Mirrors the Zod constraints of `packages/common/src/schemas/sim.ts` so untrusted input (import
/// files, API payloads) is validated at the boundary before it touches local state.
public struct ValidationError: Error, Equatable, LocalizedError, Sendable {
    public let path: String
    public let message: String

    public init(_ path: String, _ message: String) {
        self.path = path
        self.message = message
    }

    public var errorDescription: String? { "\(path): \(message)" }
}

public enum SimValidation {
    public static let maxAbs = 1e12

    static func number(_ value: Double??, _ path: String) throws {
        guard case let .some(.some(v)) = value else { return }
        guard v.isFinite, abs(v) <= maxAbs else { throw ValidationError(path, "Invalid number") }
    }

    static func number(_ value: Double?, _ path: String) throws {
        try number(Optional(value), path)
    }

    /// `name()`: trimmed, 1…120 characters.
    public static func name(_ value: String?, _ path: String, required: Bool) throws -> String? {
        guard let value else {
            if required { throw ValidationError(path, "Name is required") }
            return nil
        }
        let trimmed = value.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else { throw ValidationError(path, "Name is required") }
        guard trimmed.count <= 120 else { throw ValidationError(path, "Name is too long") }
        return trimmed
    }

    /// `text()`: trimmed, ≤ 2000 characters, nullable.
    static func text(_ value: String??, _ path: String) throws -> String?? {
        guard case let .some(.some(v)) = value else { return value }
        let trimmed = v.trimmingCharacters(in: .whitespacesAndNewlines)
        guard trimmed.count <= 2000 else { throw ValidationError(path, "Text is too long") }
        return .some(trimmed)
    }
}

public extension VehicleInputDto {
    /// Validates and normalises (trims) the payload. `requireName` is true for create/import.
    func validated(requireName: Bool) throws -> VehicleInputDto {
        var copy = self
        copy.name = try SimValidation.name(name, "vehicle.name", required: requireName)
        copy.beschreibung = try SimValidation.text(beschreibung, "vehicle.beschreibung")
        try SimValidation.number(gewicht, "vehicle.gewicht")
        if let motor {
            var m = motor
            m.name = try SimValidation.name(motor.name, "vehicle.motor.name", required: requireName)
            let numbers: [(Double??, String)] = [
                (m.bohrungD, "bohrungD"), (m.brennraumV, "brennraumV"), (m.deachsierungL, "deachsierungL"),
                (m.heizwertU, "heizwertU"), (m.hubL, "hubL"), (m.hubraumV, "hubraumV"), (m.kolbenG, "kolbenG"),
                (m.kurbelgehaeuseV, "kurbelgehaeuseV"), (m.pleulL, "pleulL"), (m.resonanzU, "resonanzU"),
                (m.verdichtungV, "verdichtungV"), (m.zuendzeitpunkt, "zuendzeitpunkt"),
                (m.zylinderAnz, "zylinderAnz"),
            ]
            for (value, key) in numbers { try SimValidation.number(value, "vehicle.motor.\(key)") }
            if let e = m.einlass {
                for (value, key) in [(e.breiteB, "breiteB"), (e.durchmesserD, "durchmesserD"),
                                     (e.flaecheA, "flaecheA"), (e.hoeheH, "hoeheH"), (e.laengeL, "laengeL"),
                                     (e.luftBedarf, "luftBedarf"), (e.steuerzeitSZ, "steuerzeitSZ")] {
                    try SimValidation.number(value, "vehicle.motor.einlass.\(key)")
                }
                if let v = e.vergaser {
                    try SimValidation.number(v.benzinLuftF, "vehicle.motor.einlass.vergaser.benzinLuftF")
                    try SimValidation.number(v.durchmesserD, "vehicle.motor.einlass.vergaser.durchmesserD")
                }
            }
            if let a = m.auslass {
                for (value, key) in [(a.breiteB, "breiteB"), (a.durchmesserD, "durchmesserD"),
                                     (a.flaecheA, "flaecheA"), (a.hoeheH, "hoeheH"), (a.laengeL, "laengeL"),
                                     (a.steuerzeitSZ, "steuerzeitSZ")] {
                    try SimValidation.number(value, "vehicle.motor.auslass.\(key)")
                }
                if let p = a.auspuff {
                    if let stage = p.diffusorStage, !(0...3).contains(stage) {
                        throw ValidationError("vehicle.motor.auslass.auspuff.diffusorStage", "Must be 0…3")
                    }
                    let values: [Double??] = [
                        p.abgasT, p.abgasV, p.diffusorD, p.diffusorD1, p.diffusorD2, p.diffusorD3, p.diffusorL,
                        p.diffusorL1, p.diffusorL2, p.diffusorL3, p.diffusorW, p.diffusorW1, p.diffusorW2,
                        p.diffusorW3, p.endrohrD, p.endrohrL, p.gegenkonusD, p.gegenkonusL, p.gegenKonusW,
                        p.gesamtL, p.kruemmerD, p.kruemmerF, p.kruemmerL, p.kruemmerW, p.mittelteilD,
                        p.mittelteilF, p.mittelteilL, p.resonanzL,
                    ]
                    for value in values { try SimValidation.number(value, "vehicle.motor.auslass.auspuff") }
                }
            }
            if let u = m.ueberstroemer {
                for (value, key) in [(u.anzahl, "anzahl"), (u.breiteB, "breiteB"), (u.flaecheA, "flaecheA"),
                                     (u.hoeheH, "hoeheH"), (u.steuerzeitSZ, "steuerzeitSZ")] {
                    try SimValidation.number(value, "vehicle.motor.ueberstroemer.\(key)")
                }
            }
            copy.motor = m
        }
        return copy
    }
}

public extension DynoDataInputDto {
    func validate() throws {
        func limit(_ count: Int?, _ path: String) throws {
            if let count, count > maxDynoPoints { throw ValidationError(path, "Too many points") }
        }
        try limit(drehzahl?.count, "drehzahl")
        try limit(dynoPs?.count, "dynoPs")
        try limit(ausrollen?.count, "ausrollen")
        try limit(geschwindigkeit?.count, "geschwindigkeit")
        for p in drehzahl ?? [] where !(p.zeit.isFinite && p.drehzahl.isFinite) {
            throw ValidationError("drehzahl", "Invalid number")
        }
        for p in dynoPs ?? [] where !(p.drehzahl.isFinite && p.ps.isFinite) {
            throw ValidationError("dynoPs", "Invalid number")
        }
        for s in (ausrollen ?? []) + (geschwindigkeit ?? []) {
            guard (-90...90).contains(s.latitude), (-180...180).contains(s.longitude) else {
                throw ValidationError("gps", "Invalid coordinate")
            }
            for value in [s.speed, s.accuracy, s.altitude, s.altitudeAccuracy, s.heading, s.headingAccuracy] {
                try SimValidation.number(value, "gps")
            }
            if let zeitpunkt = s.zeitpunkt, ISO8601.parse(zeitpunkt) == nil {
                throw ValidationError("gps.zeitpunkt", "Invalid date-time")
            }
        }
    }
}

public extension DynoExportDto {
    /// `DynoExportSchema.parse`: validates untrusted file content and returns the normalised copy.
    func validated() throws -> DynoExportDto {
        guard format == dynoExportFormat else { throw ValidationError("format", "Unsupported export format") }
        guard version == 3 else { throw ValidationError("version", "Unsupported export version") }
        guard ISO8601.parse(exportedAt) != nil else { throw ValidationError("exportedAt", "Invalid date-time") }
        var copy = self
        copy.vehicle = try vehicle.validated(requireName: true)
        copy.dyno.name = try SimValidation.name(dyno.name, "dyno.name", required: true)!
        copy.dyno.beschreibung = try SimValidation.text(dyno.beschreibung, "dyno.beschreibung")
        try DynoDataInputDto(drehzahl: dyno.drehzahl, dynoPs: dyno.dynoPs, ausrollen: dyno.ausrollen,
                             geschwindigkeit: dyno.geschwindigkeit).validate()
        return copy
    }
}

/// ISO 8601 helpers matching JS `Date.prototype.toISOString()` (`2026-01-01T00:00:00.000Z`).
public enum ISO8601 {
    public static func string(from date: Date = Date()) -> String {
        date.formatted(.iso8601.year().month().day().dateSeparator(.dash).time(includingFractionalSeconds: true)
            .timeSeparator(.colon).timeZone(separator: .omitted))
    }

    public static func parse(_ text: String) -> Date? {
        if let date = try? Date(text, strategy: .iso8601.year().month().day().dateSeparator(.dash)
            .time(includingFractionalSeconds: true).timeSeparator(.colon).timeZone(separator: .omitted)) {
            return date
        }
        return try? Date(text, strategy: .iso8601)
    }
}
