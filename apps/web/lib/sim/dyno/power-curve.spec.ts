import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { getLeistungskurve, wattToPs } from '@sim-tuning/common/logic';
import { DynoExportSchema } from '@sim-tuning/common/schemas';
import {
  airDensity,
  computePowerCurve,
  DEFAULT_DIAGNOSIS_PARAMS,
  peakPower,
  toDrehzahlSeries,
} from './power-curve';
import { pickRecorderMimeType, safeFileName, validateAudioFile } from './audio-input';
import { fillTemplate } from './format';

describe('power curve (v2 range logic, v3 physics)', () => {
  const drehzahl = [
    { zeit: 0, drehzahl: 3000 },
    { zeit: 500, drehzahl: 4000 },
    { zeit: 1000, drehzahl: 5000 },
    { zeit: 1500, drehzahl: 6000 },
    { zeit: 2000, drehzahl: 5500 },
  ];

  it('uses the range before the RPM maximum without the first point', () => {
    const curve = computePowerCurve(drehzahl, DEFAULT_DIAGNOSIS_PARAMS);
    assert.deepEqual(
      curve.map((p) => p.drehzahl),
      [4000, 5000],
    );
    const [first] = getLeistungskurve(drehzahl.slice(0, 4), 14, 0.277, {
      masse: 170,
      luftdichte: 1.2,
      cw: 0.8,
      frontA: 0.75,
      rollwiderstand: 0.005,
      steigung: 0,
    });
    assert.equal(curve[0]!.ps, wattToPs(first!.watt));
    // Physically plausible for a moped (v2 formula produced ~1e14 PS).
    assert.ok(curve.every((p) => p.ps > 0 && p.ps < 50));
    assert.equal(peakPower(curve)!.drehzahl, 5000);
  });

  it('returns nothing for too short curves', () => {
    assert.deepEqual(computePowerCurve(drehzahl.slice(0, 2), DEFAULT_DIAGNOSIS_PARAMS), []);
    assert.equal(peakPower([]), null);
  });

  it('derives air density from the environment (hPa, °C) or falls back to 1.2', () => {
    const rho = airDensity({ luftdruckP: 1013.25, temperaturT: 15 });
    assert.ok(Math.abs(rho - 1.225) < 0.001);
    assert.equal(airDensity(null), 1.2);
    assert.equal(airDensity({ luftdruckP: null, temperaturT: 20 }), 1.2);
  });

  it('maps fitted points to the stored drehzahl series', () => {
    assert.deepEqual(
      toDrehzahlSeries([
        { x: 0, y: 3000 },
        { x: 5, y: Number.NaN },
        { x: 10, y: 3100 },
      ]),
      [
        { zeit: 0, drehzahl: 3000 },
        { zeit: 10, drehzahl: 3100 },
      ],
    );
  });

  it('rejects invalid import files via DynoExportSchema', () => {
    assert.equal(DynoExportSchema.safeParse({ format: 'other' }).success, false);
  });
});

describe('audio input helpers', () => {
  it('validates uploaded files', () => {
    assert.equal(validateAudioFile({ name: 'a.wav', size: 10, type: 'audio/wav' }), null);
    assert.equal(validateAudioFile({ name: 'a.m4a', size: 10, type: '' }), null);
    assert.equal(validateAudioFile({ name: 'a.exe', size: 10, type: '' }), 'unsupportedType');
    assert.equal(validateAudioFile({ name: 'a.wav', size: 0, type: 'audio/wav' }), 'empty');
    assert.equal(
      validateAudioFile({ name: 'a.wav', size: 60 * 1024 * 1024, type: 'audio/wav' }),
      'tooLarge',
    );
  });

  it('prefers webm/opus and falls back to mp4 for Safari', () => {
    assert.equal(
      pickRecorderMimeType(() => true),
      'audio/webm;codecs=opus',
    );
    assert.equal(
      pickRecorderMimeType((type) => type.startsWith('audio/mp4')),
      'audio/mp4;codecs=mp4a.40.2',
    );
    assert.equal(
      pickRecorderMimeType(() => false),
      undefined,
    );
    assert.equal(
      pickRecorderMimeType(() => {
        throw new Error('nope');
      }),
      undefined,
    );
  });

  it('builds safe download names and fills templates', () => {
    assert.equal(safeFileName('../../etc/Sommer Lauf!', 'json'), 'etcSommer-Lauf.json');
    assert.equal(safeFileName('', 'wav'), 'dyno.wav');
    assert.equal(fillTemplate('{a} of {b} {c}', { a: 1, b: 2 }), '1 of 2 {c}');
  });
});
