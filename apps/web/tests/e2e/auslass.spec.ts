import { test, expect, type Page } from '@playwright/test';
import { resetOfflineStore } from './helpers/offline-store.js';
import { navigateToTab } from './helpers/shell.js';

/** Opens the exhaust module and duplicates the (read-only) starter vehicle so it can be edited. */
async function openEditableAuslass(page: Page): Promise<void> {
  await navigateToTab(page, 'auslass');
  await expect(page.getByRole('heading', { name: 'Exhaust', exact: true })).toBeVisible();
  await expect(page.getByTestId('read-only-banner')).toBeVisible();
  await expect(page.getByTestId('auslass-theorie-save')).toHaveText('Save as my vehicle');
  await page.getByRole('button', { name: 'Duplicate to edit' }).click();
  await expect(page.getByTestId('read-only-banner')).toBeHidden();
}

async function fillAnwendung(page: Page, values: Record<string, string>): Promise<void> {
  for (const [key, value] of Object.entries(values)) {
    await page.getByTestId(`auslass-anwendung-${key}`).fill(value);
  }
}

test.describe('Auslass module (guest mode)', () => {
  test.beforeEach(async ({ page }) => {
    await resetOfflineStore(page);
  });

  test('theory: header, gas velocity and resonance length', async ({ page }) => {
    await openEditableAuslass(page);

    await page.getByTestId('auslass-theorie-kruemmerD').fill('30');
    await page.getByTestId('auslass-theorie-kruemmerF').fill('10');
    await expect(page.getByTestId('auslass-theorie-kruemmerF-value')).toHaveText('10.0');
    await expect(page.getByTestId('auslass-theorie-kruemmerL')).toHaveText(/^300\s*mm$/);

    await page.getByTestId('auslass-theorie-flaecheA').fill('200');
    // Flange area 1.1–1.5 × port area (Rechensheet-2Takt): 200 mm² → 16.7–19.5 mm.
    await expect(page.getByTestId('auslass-theorie-kruemmerDRange')).toHaveText(
      /16\.7 – 19\.5\s*mm/,
    );

    // Speed of sound in the exhaust gas, √(1.32·295·T) (Rechensheet-2Takt).
    await page.getByTestId('auslass-theorie-gasT').fill('550');
    await expect(page.getByTestId('auslass-theorie-gasV')).toHaveText(/566\.16/);

    await expect(page.getByTestId('auslass-theorie-resonanzU')).not.toHaveValue('');
    await page.getByTestId('auslass-theorie-resonanzU').fill('5000');
    await page.getByTestId('auslass-theorie-steuerzeitSZ').fill('190');
    // 500 °C → c = 548.69 m/s → l = c·φ / (12·n).
    await page.getByTestId('auslass-theorie-abgasT').fill('500');
    await expect(page.getByTestId('auslass-theorie-resonanzL')).toHaveText(/1,737\.52/);

    await page.getByTestId('auslass-theorie-save').click();
    await expect(page.getByText('Saved', { exact: true })).toBeVisible();
  });

  test('expansion chamber: calculate, draw, list and save', async ({ page }) => {
    await openEditableAuslass(page);
    await page.getByTestId('module-tab-anwendung').click();

    await expect(page.getByTestId('auslass-anwendung-calculate')).toBeDisabled();
    await expect(page.getByTestId('auslass-anwendung-missing')).toBeVisible();
    await expect(page.getByTestId('auslass-anwendung-stage-hint')).toBeVisible();

    await fillAnwendung(page, {
      abgasT: '500',
      resonanzU: '8000',
      auslassDurchmesserD: '28',
      auslassLaengeL: '60',
      auslassSteuerzeitSZ: '190',
      auslassFlaecheA: '400',
      kruemmerF: '10',
      kruemmerW: '2',
      mittelteilF: '2.9',
      gegenKonusW: '8',
      endrohrD: '22',
      endrohrL: '250',
      diffusorW1: '7.5',
    });
    await expect(page.getByTestId('auslass-anwendung-missing')).toBeHidden();
    await page.getByTestId('auslass-anwendung-calculate').click();

    const svg = page.getByTestId('auspuff-drawing');
    await expect(svg).toBeVisible();
    // The drawing is cropped to the chamber (plus padding) instead of the empty v2 canvas.
    const viewBox = (await svg.getAttribute('viewBox'))!.split(' ').map(Number);
    expect(viewBox).toHaveLength(4);
    expect(viewBox[2]).toBeLessThan(2000);
    expect(viewBox[3]).toBeLessThan(1000);
    expect(viewBox[3]).toBeGreaterThan(0);
    await expect(svg).toHaveAttribute('role', 'img');
    await expect(svg.locator('[data-kind="dimension"]').first()).toHaveAttribute(
      'marker-end',
      /url\(#/,
    );
    await expect(page.getByTestId('auspuff-part-kruemmer')).toHaveText('Header');
    await expect(page.getByTestId('auspuff-part-endrohr')).toHaveText('Stinger');
    await expect(page.getByTestId('auspuff-value-LK')).toBeVisible();

    // Stückliste: value and unit live in separate columns (Pos · Benennung · Wert · Einheit).
    await expect(page.getByTestId('auslass-anwendung-row-resonanzL')).toHaveText(/^1,085\.95$/);
    await expect(page.getByTestId('auslass-anwendung-row-resonanzL-unit')).toHaveText('mm');
    await expect(page.getByTestId('auslass-anwendung-row-kruemmerL')).toHaveText(/^220$/);
    await expect(page.getByTestId('auslass-anwendung-row-endrohrL')).toHaveText(/^250$/);
    await expect(page.getByTestId('auslass-anwendung-row-endrohrL-unit')).toHaveText('mm');

    await page.getByTestId('auslass-anwendung-save').click();
    await expect(page.getByText('Saved', { exact: true })).toBeVisible();

    // Changing an input makes the result stale until recalculated.
    await page.getByTestId('auslass-anwendung-diffusorStage').selectOption('2');
    await expect(page.getByTestId('auslass-anwendung-diffusorW1')).toBeHidden();
    await expect(page.getByTestId('auslass-anwendung-save')).toBeDisabled();
    await page.getByTestId('auslass-anwendung-calculate').click();
    await expect(page.getByTestId('auslass-anwendung-row-diffusorL1')).toHaveText(/^0$/);
    await expect(page.getByTestId('auslass-anwendung-row-diffusorL2')).toHaveText(/^0$/);

    // Saved values are prefilled after a reload.
    await page.reload();
    await navigateToTab(page, 'auslass');
    await page.getByTestId('module-tab-anwendung').click();
    await expect(page.getByTestId('auslass-anwendung-endrohrL')).toHaveValue('250');
    await expect(page.getByTestId('auslass-anwendung-kruemmerW')).toHaveValue('2');
  });

  test('drawing scales inside a 375px viewport without page overflow', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await openEditableAuslass(page);
    await page.getByTestId('module-tab-anwendung').click();
    await fillAnwendung(page, {
      abgasT: '500',
      resonanzU: '8000',
      auslassDurchmesserD: '28',
      auslassLaengeL: '60',
      auslassSteuerzeitSZ: '190',
      auslassFlaecheA: '400',
      kruemmerF: '10',
      kruemmerW: '2',
      mittelteilF: '2.9',
      gegenKonusW: '8',
      endrohrD: '22',
      endrohrL: '250',
      diffusorW1: '7.5',
    });
    await page.getByTestId('auslass-anwendung-calculate').click();
    const box = await page.getByTestId('auspuff-drawing').boundingBox();
    expect(box).not.toBeNull();
    expect(box!.width).toBeLessThanOrEqual(375);
    const overflow = await page.evaluate(
      () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
    );
    expect(overflow).toBeLessThanOrEqual(0);
  });
});
