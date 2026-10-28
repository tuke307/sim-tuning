import Foundation

/// Unit catalogue ported from `packages/common/src/units/index.ts` (v2 UnitsNet lists). Every
/// quantity converts through an SI reference unit; values are stored in field base units.
public enum QuantityKind: String, CaseIterable, Sendable, Codable {
    case length, area, volume, mass, speed, pressure, temperature, duration
}

public struct UnitDefinition: Sendable, Hashable {
    public let id: UnitId
    public let quantity: QuantityKind
    public let abbreviation: String
    /// Linear factor to the quantity's reference unit. Ignored for temperature.
    public let factor: Double
}

public enum UnitId: String, CaseIterable, Sendable, Codable, Identifiable {
    // length → metre
    case micrometer, millimeter, centimeter, decimeter, meter, kilometer
    // area → square metre
    case squareMillimeter, squareCentimeter, squareDecimeter, squareMeter
    // volume → cubic metre
    case cubicMicrometer, cubicMillimeter, cubicCentimeter, cubicDecimeter, milliliter, liter
    // mass → kilogram
    case milligram, gram, decigram, kilogram, tonne
    // speed → metre per second
    case millimeterPerMinute, centimeterPerMinute, decimeterPerMinute, meterPerMinute, kilometerPerMinute
    case millimeterPerSecond, centimeterPerSecond, decimeterPerSecond, meterPerSecond, kilometerPerSecond
    case millimeterPerHour, centimeterPerHour, meterPerHour, kilometerPerHour
    // pressure → pascal
    case millibar, bar, kilobar, megabar, millipascal, pascal, hectopascal, kilopascal, megapascal
    // temperature (affine)
    case degreeCelsius, degreeFahrenheit, kelvin
    // duration → second
    case millisecond, second, hour, day

    public var id: String { rawValue }

    public var definition: UnitDefinition {
        let (quantity, abbreviation, factor) = Self.table[self]!
        return UnitDefinition(id: self, quantity: quantity, abbreviation: abbreviation, factor: factor)
    }

    public var quantity: QuantityKind { definition.quantity }
    public var abbreviation: String { definition.abbreviation }

    private static let table: [UnitId: (QuantityKind, String, Double)] = [
        .micrometer: (.length, "µm", 1e-6),
        .millimeter: (.length, "mm", 1e-3),
        .centimeter: (.length, "cm", 1e-2),
        .decimeter: (.length, "dm", 1e-1),
        .meter: (.length, "m", 1),
        .kilometer: (.length, "km", 1e3),
        .squareMillimeter: (.area, "mm²", 1e-6),
        .squareCentimeter: (.area, "cm²", 1e-4),
        .squareDecimeter: (.area, "dm²", 1e-2),
        .squareMeter: (.area, "m²", 1),
        .cubicMicrometer: (.volume, "µm³", 1e-18),
        .cubicMillimeter: (.volume, "mm³", 1e-9),
        .cubicCentimeter: (.volume, "cm³", 1e-6),
        .cubicDecimeter: (.volume, "dm³", 1e-3),
        .milliliter: (.volume, "ml", 1e-6),
        .liter: (.volume, "l", 1e-3),
        .milligram: (.mass, "mg", 1e-6),
        .gram: (.mass, "g", 1e-3),
        .decigram: (.mass, "dg", 1e-4),
        .kilogram: (.mass, "kg", 1),
        .tonne: (.mass, "t", 1e3),
        .millimeterPerMinute: (.speed, "mm/min", 1e-3 / 60),
        .centimeterPerMinute: (.speed, "cm/min", 1e-2 / 60),
        .decimeterPerMinute: (.speed, "dm/min", 1e-1 / 60),
        .meterPerMinute: (.speed, "m/min", 1.0 / 60),
        .kilometerPerMinute: (.speed, "km/min", 1e3 / 60),
        .millimeterPerSecond: (.speed, "mm/s", 1e-3),
        .centimeterPerSecond: (.speed, "cm/s", 1e-2),
        .decimeterPerSecond: (.speed, "dm/s", 1e-1),
        .meterPerSecond: (.speed, "m/s", 1),
        .kilometerPerSecond: (.speed, "km/s", 1e3),
        .millimeterPerHour: (.speed, "mm/h", 1e-3 / 3600),
        .centimeterPerHour: (.speed, "cm/h", 1e-2 / 3600),
        .meterPerHour: (.speed, "m/h", 1.0 / 3600),
        .kilometerPerHour: (.speed, "km/h", 1e3 / 3600),
        .millibar: (.pressure, "mbar", 1e2),
        .bar: (.pressure, "bar", 1e5),
        .kilobar: (.pressure, "kbar", 1e8),
        .megabar: (.pressure, "Mbar", 1e11),
        .millipascal: (.pressure, "mPa", 1e-3),
        .pascal: (.pressure, "Pa", 1),
        .hectopascal: (.pressure, "hPa", 1e2),
        .kilopascal: (.pressure, "kPa", 1e3),
        .megapascal: (.pressure, "MPa", 1e6),
        .degreeCelsius: (.temperature, "°C", 1),
        .degreeFahrenheit: (.temperature, "°F", 1),
        .kelvin: (.temperature, "K", 1),
        .millisecond: (.duration, "ms", 1e-3),
        .second: (.duration, "s", 1),
        .hour: (.duration, "h", 3600),
        .day: (.duration, "d", 86400),
    ]
}

public enum Units {
    /// Units offered per quantity, in the order of the v2 pickers.
    public static let quantityUnits: [QuantityKind: [UnitId]] = [
        .length: [.micrometer, .millimeter, .centimeter, .decimeter, .meter, .kilometer],
        .area: [.squareMillimeter, .squareCentimeter, .squareDecimeter, .squareMeter],
        .volume: [.cubicMicrometer, .cubicMillimeter, .cubicCentimeter, .cubicDecimeter, .milliliter, .liter],
        .mass: [.milligram, .gram, .decigram, .kilogram, .tonne],
        .speed: [
            .millimeterPerMinute, .centimeterPerMinute, .decimeterPerMinute, .meterPerMinute,
            .kilometerPerMinute, .millimeterPerSecond, .centimeterPerSecond, .decimeterPerSecond,
            .meterPerSecond, .kilometerPerSecond, .millimeterPerHour, .centimeterPerHour,
            .meterPerHour, .kilometerPerHour,
        ],
        .pressure: [
            .millibar, .bar, .kilobar, .megabar, .millipascal, .pascal, .kilopascal, .hectopascal,
            .megapascal,
        ],
        .temperature: [.degreeCelsius, .degreeFahrenheit, .kelvin],
        .duration: [.millisecond, .second, .hour, .day],
    ]

    public struct ConversionError: Error, Equatable, CustomStringConvertible {
        public let from: UnitId
        public let to: UnitId
        public var description: String {
            "Cannot convert \(from.quantity.rawValue) (\(from.rawValue)) to \(to.quantity.rawValue) (\(to.rawValue))"
        }
    }

    private static func toKelvin(_ value: Double, _ from: UnitId) -> Double {
        if from == .kelvin { return value }
        if from == .degreeFahrenheit { return ((value - 32) * 5) / 9 + 273.15 }
        return value + 273.15
    }

    private static func fromKelvin(_ value: Double, _ to: UnitId) -> Double {
        if to == .kelvin { return value }
        if to == .degreeFahrenheit { return ((value - 273.15) * 9) / 5 + 32 }
        return value - 273.15
    }

    /// Converts a value between two units of the same quantity.
    public static func convert(_ value: Double, from: UnitId, to: UnitId) throws -> Double {
        if from == to { return value }
        let a = from.definition
        let b = to.definition
        guard a.quantity == b.quantity else { throw ConversionError(from: from, to: to) }
        if a.quantity == .temperature { return fromKelvin(toKelvin(value, from), to) }
        return (value * a.factor) / b.factor
    }

    public struct RoundingOptions: Sendable, Equatable {
        public var roundingAccuracy: Int
        public var roundOnUnitChange: Bool
        public init(roundingAccuracy: Int = 2, roundOnUnitChange: Bool = true) {
            self.roundingAccuracy = roundingAccuracy
            self.roundOnUnitChange = roundOnUnitChange
        }
    }

    /// v2 `BaseEntityModel.ConvertValueForUnit`: converts and optionally rounds a nullable value.
    public static func convertValueForUnit(
        _ value: Double?, from: UnitId, to: UnitId, options: RoundingOptions
    ) throws -> Double? {
        guard let value else { return nil }
        let converted = try convert(value, from: from, to: to)
        return options.roundOnUnitChange ? Rounding.roundTo(converted, options.roundingAccuracy) : converted
    }
}

/// Base unit of every unit-bearing field (v2 `*BaseUnit` statics). Values are persisted in these.
public enum FieldBaseUnits {
    public static let vehicle: [String: UnitId] = ["gewicht": .kilogram]
    public static let motor: [String: UnitId] = [
        "bohrungD": .millimeter, "brennraumV": .cubicMillimeter, "deachsierungL": .millimeter,
        "hubL": .millimeter, "hubraumV": .cubicMillimeter, "kolbenG": .meterPerSecond,
        "kurbelgehaeuseV": .cubicMillimeter, "pleulL": .millimeter,
    ]
    public static let einlass: [String: UnitId] = [
        "breiteB": .millimeter, "durchmesserD": .millimeter, "flaecheA": .squareMillimeter,
        "hoeheH": .millimeter, "laengeL": .millimeter,
    ]
    public static let vergaser: [String: UnitId] = ["durchmesserD": .millimeter]
    public static let auslass: [String: UnitId] = einlass
    public static let auspuff: [String: UnitId] = [
        "abgasT": .degreeCelsius, "abgasV": .meterPerSecond, "diffusorD": .millimeter,
        "diffusorD1": .millimeter, "diffusorD2": .millimeter, "diffusorD3": .millimeter,
        "diffusorL": .millimeter, "diffusorL1": .millimeter, "diffusorL2": .millimeter,
        "diffusorL3": .millimeter, "endrohrD": .millimeter, "endrohrL": .millimeter,
        "gegenkonusD": .millimeter, "gegenkonusL": .millimeter, "gesamtL": .centimeter,
        "kruemmerD": .millimeter, "kruemmerL": .millimeter, "mittelteilD": .millimeter,
        "mittelteilL": .millimeter, "resonanzL": .millimeter,
    ]
    public static let ueberstroemer: [String: UnitId] = [
        "breiteB": .millimeter, "flaecheA": .squareMillimeter, "hoeheH": .millimeter,
    ]
    public static let environment: [String: UnitId] = [
        "luftdruckP": .hectopascal, "temperaturT": .degreeCelsius,
    ]
}
