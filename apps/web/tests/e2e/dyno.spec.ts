import { readFileSync } from 'node:fs';
import { test, expect, type Page } from '@playwright/test';
import { resetOfflineStore } from './helpers/offline-store.js';
import { navigateToTab } from './helpers/shell.js';

/** Guest mode: duplicate a starter vehicle and create a dyno on it. */
async function createDyno(page: Page, name: string) {
  await navigateToTab(page, 'dyno');
  await expect(page.getByRole('heading', { name: 'Dyno', exact: true })).toBeVisible();
  const banner = page.getByTestId('read-only-banner');
  await expect(banner).toBeVisible({ timeout: 15_000 });
  await banner.getByRole('button', { name: 'Duplicate to edit' }).click();
  await expect(banner).toBeHidden();

  await page.getByTestId('dyno-name').fill(name);
  await page.getByTestId('dyno-description').fill('E2E acceleration run');
  await page.getByTestId('dyno-environment').selectOption({ index: 1 });
  await page.getByTestId('dyno-save').click();
  await expect(page.getByTestId('dyno-data-notice')).toHaveText('Dyno saved');
  await expect(page.getByTestId('dyno-count-drehzahl')).toHaveText('0');
}

test.describe('Dyno module', () => {
  test.beforeEach(async ({ page }) => {
    await resetOfflineStore(page);
  });

  test('starter vehicles are read-only for dynos', async ({ page }) => {
    await navigateToTab(page, 'dyno');
    await expect(page.getByTestId('read-only-banner')).toBeVisible({ timeout: 15_000 });
    await expect(page.getByTestId('dyno-name')).toBeDisabled();
    await expect(page.getByTestId('dyno-save')).toBeDisabled();
  });

  test('audio dyno: demo recording → analyze → filter → pick curve → save → power → export', async ({
    page,
  }, testInfo) => {
    test.setTimeout(180_000);
    await createDyno(page, 'E2E Dyno');

    // Audio: demo recording, analysis in the Web Worker, DBSCAN filter, curve pick.
    await page.getByTestId('module-tab-audio').click();
    await page.getByTestId('dyno-audio-demo').click();
    await expect(page.getByTestId('dyno-audio-loaded')).toContainText('Demo recording');
    await page.getByTestId('dyno-audio-analyze').click();
    await expect(page.getByTestId('dyno-audio-points')).toHaveText(/8[.,\s]?336/, {
      timeout: 90_000,
    });
    await expect(page.getByTestId('dyno-audio-chart')).toBeVisible();

    await page.getByTestId('dyno-audio-filter').click();
    await expect(page.getByTestId('dyno-audio-clusters')).toHaveText('15');
    await expect(page.getByTestId('dyno-audio-chart')).toContainText('Graph 15');

    // "Graph 2" is the main engine-speed harmonic of the v2 demo recording (2863 points).
    await page.getByTestId('dyno-audio-cluster').selectOption('1');
    await expect(page.getByTestId('dyno-audio-chart')).toContainText('Fitted curve');
    await page.getByTestId('dyno-audio-save').click();
    await expect(page.getByTestId('dyno-audio-notice')).toHaveText('RPM curve saved');
    await expect(page.getByTestId('dyno-audio-stored')).toContainText('Stored RPM curve');

    // Diagnose: power curve from the saved RPM curve.
    await page.getByTestId('module-tab-diagnosis').click();
    await page.getByTestId('dyno-diag-calculate').click();
    await expect(page.getByTestId('dyno-diag-chart')).toBeVisible();
    await expect(page.getByTestId('dyno-diag-peak')).not.toHaveText('–');
    await page.getByTestId('dyno-diag-save').click();
    await expect(page.getByTestId('dyno-diag-notice')).toHaveText('Power curve saved');

    // Data: counts + JSON export.
    await page.getByTestId('module-tab-data').click();
    await expect(page.getByTestId('dyno-count-drehzahl')).not.toHaveText('0');
    await expect(page.getByTestId('dyno-count-ps')).not.toHaveText('0');
    const downloadPromise = page.waitForEvent('download');
    await page.getByTestId('dyno-export').click();
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toMatch(/\.json$/);
    const file = testInfo.outputPath('dyno-export.json');
    await download.saveAs(file);
    const exported = JSON.parse(readFileSync(file, 'utf8')) as {
      format: string;
      version: number;
      dyno: { name: string; drehzahl: unknown[]; dynoPs: unknown[] };
    };
    expect(exported.format).toBe('sim-tuning-dyno');
    expect(exported.version).toBe(3);
    expect(exported.dyno.name).toBe('E2E Dyno');
    expect(exported.dyno.drehzahl.length).toBeGreaterThan(100);
    expect(exported.dyno.dynoPs.length).toBeGreaterThan(0);

    // Import the export again → new vehicle with the same dyno data.
    const vehicleOptions = page.getByTestId('vehicle-select').locator('option');
    const before = await vehicleOptions.count();
    await page.getByTestId('dyno-import-input').setInputFiles(file);
    await expect(vehicleOptions).toHaveCount(before + 1);
    await expect(page.getByTestId('dyno-name')).toHaveValue('E2E Dyno');
    await expect(page.getByTestId('dyno-count-drehzahl')).toHaveText(
      String(exported.dyno.drehzahl.length).replace(/\B(?=(\d{3})+(?!\d))/g, ','),
    );
  });

  test('rejects invalid import files', async ({ page }) => {
    await navigateToTab(page, 'dyno');
    await page.getByTestId('dyno-import-input').setInputFiles({
      name: 'broken.json',
      mimeType: 'application/json',
      buffer: Buffer.from('{"format":"something-else"}'),
    });
    await expect(page.getByTestId('dyno-data-notice')).toHaveText(
      'This file is not a valid SimTuning dyno export.',
    );
  });

  test('GPS run: start shows the countdown and reset returns to pull-away', async ({
    page,
    context,
  }) => {
    await context.grantPermissions(['geolocation']);
    await context.setGeolocation({ latitude: 51.05, longitude: 13.74 });
    await createDyno(page, 'GPS Dyno');
    await page.getByTestId('module-tab-runtime').click();
    const state = page.getByTestId('dyno-run-state');
    await expect(state).toHaveAttribute('data-phase', 'pre');
    await page.getByTestId('dyno-run-start').click();
    await expect(state).toHaveAttribute('data-phase', 'countdownAcceleration');
    await expect(page.getByTestId('dyno-run-stop-acceleration')).toBeVisible();
    await page.getByTestId('dyno-run-stop-acceleration').click();
    await expect(state).toHaveAttribute('data-phase', 'countdownRollout');
    await page.getByTestId('dyno-run-stop-rollout').click();
    await expect(state).toHaveAttribute('data-phase', 'done');
    await page.getByTestId('dyno-run-reset').click();
    await expect(state).toHaveAttribute('data-phase', 'pre');
    await expect(page.getByTestId('dyno-run-acceleration-empty')).toBeVisible();
  });
});
