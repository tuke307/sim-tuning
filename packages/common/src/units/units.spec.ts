import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { convertUnit, convertValueForUnit, QUANTITY_UNITS, UNITS } from './index';
import { roundTo } from '../utils/round.util';

describe('units', () => {
  it('converts linear quantities through the reference unit', () => {
    assert.equal(roundTo(convertUnit(47600, 'cubicMillimeter', 'cubicCentimeter'), 9), 47.6);
    assert.equal(roundTo(convertUnit(1, 'kilometerPerHour', 'meterPerSecond'), 6), 0.277778);
    assert.equal(roundTo(convertUnit(1010, 'hectopascal', 'bar'), 9), 1.01);
  });

  it('converts temperatures affinely', () => {
    assert.equal(roundTo(convertUnit(100, 'degreeCelsius', 'degreeFahrenheit'), 6), 212);
    assert.equal(roundTo(convertUnit(0, 'degreeCelsius', 'kelvin'), 6), 273.15);
  });

  it('rejects mixed quantities and rounds like v2 ConvertValueForUnit', () => {
    assert.throws(() => convertUnit(1, 'meter', 'kilogram'));
    const opts = { roundingAccuracy: 2, roundOnUnitChange: true };
    assert.equal(convertValueForUnit(38.125, 'millimeter', 'centimeter', opts), 3.81);
    assert.equal(convertValueForUnit(null, 'millimeter', 'centimeter', opts), null);
    assert.equal(
      convertValueForUnit(38.125, 'millimeter', 'centimeter', {
        ...opts,
        roundOnUnitChange: false,
      }),
      3.8125,
    );
  });

  it('lists only units of the matching quantity', () => {
    for (const [quantity, ids] of Object.entries(QUANTITY_UNITS)) {
      for (const id of ids) assert.equal(UNITS[id].quantity, quantity);
    }
  });

  it('uses banker rounding like .NET Math.Round', () => {
    assert.equal(roundTo(2.5, 0), 2);
    assert.equal(roundTo(3.5, 0), 4);
    assert.equal(roundTo(-2.5, 0), -2);
    assert.equal(roundTo(0.125, 2), 0.12);
  });
});
