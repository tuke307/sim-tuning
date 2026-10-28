/**
 * Documentation screenshot capture script.
 *
 * Run once to regenerate all screenshots:
 *   yarn workspace @sim-tuning/web run docs:screenshots
 *
 * Screenshots land in: apps/web/tests/e2e/doc-screenshots/
 */
import { test } from '@playwright/test';
import { shot } from './helpers/doc-screenshots.js';
import { resetOfflineStore } from './helpers/offline-store.js';
import { navigateToTab } from './helpers/shell.js';

test.describe('doc: sim-tuning pwa views', () => {
  test.describe.configure({ mode: 'serial' });

  test('01 - home', async ({ page }) => {
    await resetOfflineStore(page);
    await shot(page, '01-home');
  });

  test('02 - profile and preferences', async ({ page }) => {
    await resetOfflineStore(page);
    await navigateToTab(page, 'profile');
    await shot(page, '02-profile');
  });
});
