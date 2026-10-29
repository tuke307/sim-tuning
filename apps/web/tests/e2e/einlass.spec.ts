import { test, expect, type Page } from '@playwright/test';
import { resetOfflineStore } from './helpers/offline-store.js';
import { navigateToTab } from './helpers/shell.js';

/** Opens the intake module and duplicates the (read-only) starter vehicle so it can be edited. */
async function openEditableEinlass(page: Page): Promise<void> {
  await navigateToTab(page, 'einlass');
  await expect(page.getByRole('heading', { name: 'Intake', exact: true })).toBeVisible();
  await expect(page.getByTestId('read-only-banner')).toBeVisible();
  await expect(page.getByTestId('einlass-kanal-save')).toHaveText('Save as my vehicle');
  await page.getByRole('button', { name: 'Duplicate to edit' }).click();
  await expect(page.getByTestId('read-only-banner')).toBeHidden();
}

test.describe('Einlass module (guest mode)', () => {
  test.beforeEach(async ({ page }) => {
    await resetOfflineStore(page);
  });

  test('calculates the intake resonance length and saves the inputs', async ({ page }) => {
    await openEditableEinlass(page);

    // Prefilled from the starter vehicle (values in base units: U/min, mm³).
    await expect(page.getByTestId('einlass-kanal-resonanzU')).not.toHaveValue('');
    await expect(page.getByTestId('einlass-kanal-kurbelgehaeuseV')).not.toHaveValue('');
    await expect(page.getByTestId('einlass-kanal-resonanzL')).toHaveText('–');

    await page.getByTestId('einlass-kanal-resonanzU').fill('5000');
    await page.getByTestId('einlass-kanal-kurbelgehaeuseV').fill('142800');

    await page.getByTestId('einlass-kanal-flaecheA').fill('300');
    await page.getByTestId('einlass-kanal-steuerzeitSZ').fill('130');
    // l = 3 062 500·φ²·F / (n²·V) with F = 3 cm², φ = 130°, n = 5 000, V = 142.8 cm³.
    await expect(page.getByTestId('einlass-kanal-resonanzL')).toHaveText(/434\.93/);

    await page.getByTestId('einlass-kanal-save').click();
    await expect(page.getByText('Saved', { exact: true })).toBeVisible();

    await page.reload();
    await navigateToTab(page, 'einlass');
    await expect(page.getByTestId('einlass-kanal-flaecheA')).toHaveValue('300');
  });

  test('sizes the carburettor and main jet', async ({ page }) => {
    await openEditableEinlass(page);
    await page.getByTestId('module-tab-vergaser').click();

    await expect(page.getByTestId('einlass-vergaser-hubraumV')).not.toHaveValue('');
    await page.getByTestId('einlass-vergaser-hubraumV').fill('47600');
    await page.getByTestId('einlass-vergaser-resonanzU').fill('5000');
    await expect(page.getByTestId('einlass-vergaser-durchmesserD')).toHaveText(/13\.88/);
    await expect(page.getByTestId('einlass-vergaser-hauptdueseD')).toHaveText(/66/);

    // Custom resistance factor changes the bore.
    await page.getByTestId('einlass-vergaser-faktor').fill('1');
    await expect(page.getByTestId('einlass-vergaser-durchmesserD')).toHaveText(/15\.43/);

    await page.getByTestId('einlass-vergaser-save').click();
    await expect(page.getByText('Saved', { exact: true })).toBeVisible();
  });

  test('is keyboard reachable and fits a 375px viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await openEditableEinlass(page);
    const flaeche = page.getByLabel('Intake port area', { exact: true });
    await flaeche.focus();
    // NumberInput selects its content one animation frame after focus.
    await page.evaluate(() => new Promise((resolve) => requestAnimationFrame(resolve)));
    await page.keyboard.type('250');
    await expect(flaeche).toHaveValue('250');
    const overflow = await page.evaluate(
      () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
    );
    expect(overflow).toBeLessThanOrEqual(0);
  });
});
