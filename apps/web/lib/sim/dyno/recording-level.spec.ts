import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { classifyLevel, formatClock, measureLevel, meterFraction } from './recording-level';

describe('recording level', () => {
  it('measures RMS and peak of a sine wave', () => {
    const samples = Array.from({ length: 1000 }, (_, i) => 0.5 * Math.sin((2 * Math.PI * i) / 100));
    const level = measureLevel(samples);
    assert.ok(Math.abs(level.rms - 0.5 / Math.SQRT2) < 1e-3);
    assert.ok(Math.abs(level.peak - 0.5) < 1e-3);
  });

  it('handles an empty frame', () => {
    assert.deepEqual(measureLevel([]), { rms: 0, peak: 0 });
  });

  it('classifies low, ok and clipping input', () => {
    assert.equal(classifyLevel({ rms: 0.001, peak: 0.01 }), 'low');
    assert.equal(classifyLevel({ rms: 0.2, peak: 0.6 }), 'ok');
    assert.equal(classifyLevel({ rms: 0.5, peak: 1 }), 'clipping');
  });

  it('maps RMS to a logarithmic meter fraction', () => {
    assert.equal(meterFraction(0), 0);
    assert.equal(meterFraction(1), 1);
    assert.ok(Math.abs(meterFraction(0.001) - 0) < 1e-9);
    assert.ok(Math.abs(meterFraction(Math.pow(10, -30 / 20)) - 0.5) < 1e-9);
  });

  it('formats elapsed time as mm:ss', () => {
    assert.equal(formatClock(0), '00:00');
    assert.equal(formatClock(65_400), '01:05');
    assert.equal(formatClock(-5), '00:00');
  });
});
