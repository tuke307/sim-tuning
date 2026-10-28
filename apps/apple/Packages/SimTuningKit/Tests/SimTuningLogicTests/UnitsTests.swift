import Testing
@testable import SimTuningLogic

/// Swift mirror of `packages/common/src/units/units.spec.ts`.
@Suite("units")
struct UnitsTests {
    @Test func convertsLinearQuantitiesThroughReferenceUnit() throws {
        #expect(Rounding.roundTo(try Units.convert(47600, from: .cubicMillimeter, to: .cubicCentimeter), 9) == 47.6)
        #expect(Rounding.roundTo(try Units.convert(1, from: .kilometerPerHour, to: .meterPerSecond), 6) == 0.277778)
        #expect(Rounding.roundTo(try Units.convert(1010, from: .hectopascal, to: .bar), 9) == 1.01)
    }

    @Test func convertsTemperaturesAffinely() throws {
        #expect(Rounding.roundTo(try Units.convert(100, from: .degreeCelsius, to: .degreeFahrenheit), 6) == 212)
        #expect(Rounding.roundTo(try Units.convert(0, from: .degreeCelsius, to: .kelvin), 6) == 273.15)
    }

    @Test func rejectsMixedQuantitiesAndRoundsLikeV2() throws {
        #expect(throws: Units.ConversionError.self) { try Units.convert(1, from: .meter, to: .kilogram) }
        let opts = Units.RoundingOptions(roundingAccuracy: 2, roundOnUnitChange: true)
        #expect(try Units.convertValueForUnit(38.125, from: .millimeter, to: .centimeter, options: opts) == 3.81)
        #expect(try Units.convertValueForUnit(nil, from: .millimeter, to: .centimeter, options: opts) == nil)
        var noRound = opts
        noRound.roundOnUnitChange = false
        #expect(try Units.convertValueForUnit(38.125, from: .millimeter, to: .centimeter, options: noRound) == 3.8125)
    }

    @Test func listsOnlyUnitsOfMatchingQuantity() {
        for (quantity, ids) in Units.quantityUnits {
            for id in ids { #expect(id.quantity == quantity) }
        }
        #expect(Set(Units.quantityUnits.values.flatMap { $0 }) == Set(UnitId.allCases))
    }

    @Test func usesBankerRoundingLikeDotNet() {
        #expect(Rounding.roundTo(2.5, 0) == 2)
        #expect(Rounding.roundTo(3.5, 0) == 4)
        #expect(Rounding.roundTo(-2.5, 0) == -2)
        #expect(Rounding.roundTo(0.125, 2) == 0.12)
    }
}
