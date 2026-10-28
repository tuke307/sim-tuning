import { test, expect, type Page } from '@playwright/test';
import { gotoProfileTab, resetOfflineStore } from './helpers/offline-store.js';
import { navigateToTab } from './helpers/shell.js';

const KR51 = 'sys-veh-kr51-1';

async function openMotorWith(page: Page, vehicleId: string) {
  await navigateToTab(page, 'motor');
  await expect(page.getByRole('heading', { name: 'Engine', exact: true })).toBeVisible();
  await page.getByTestId('vehicle-select').selectOption(vehicleId);
}

test.describe('Motor module (guest mode)', () => {
  test.beforeEach(async ({ page }) => {
    await resetOfflineStore(page);
  });

  test('Hubraum derives bore, piston and oversize from the vehicle', async ({ page }) => {
    await openMotorWith(page, KR51);
    await expect(page.getByTestId('motor-hubraum-hub')).toHaveValue('39.5');
    await expect(page.getByTestId('motor-hubraum-bohrung')).toContainText('39.99');
    await expect(page.getByTestId('motor-hubraum-kolben')).toContainText('39.96');
    await expect(page.getByTestId('motor-hubraum-grinding-1')).toContainText('40.19');
    // Starter vehicles are read-only.
    await expect(page.getByTestId('motor-hubraum-apply')).toBeDisabled();

    await page.getByTestId('motor-hubraum-hub').fill('');
    await expect(page.getByTestId('motor-hubraum-bohrung')).toHaveText('–');
  });

  test('Verdichtung computes compression and machining length', async ({ page }) => {
    await openMotorWith(page, KR51);
    await page.getByTestId('module-tab-verdichtung').click();
    await page.getByTestId('motor-verdichtung-brennraum').fill('5000');
    await expect(page.getByTestId('motor-verdichtung-current')).toContainText('10.9');
    await page.getByTestId('motor-verdichtung-ziel').selectOption('12');
    await expect(page.getByTestId('motor-verdichtung-abdrehen')).toContainText('0.39');
  });

  test('Steuerdiagramm renders the timing diagram with angles', async ({ page }) => {
    await openMotorWith(page, KR51);
    await page.getByTestId('module-tab-steuerdiagramm').click();
    const svg = page.getByRole('img', { name: /Port timing diagram/ });
    await expect(svg).toBeVisible();
    await expect(page.getByTestId('motor-steuerdiagramm-line-einlass-oeffnet')).toBeAttached();
    await expect(page.getByTestId('motor-angles-einlass')).toContainText('292');
    await expect(page.getByTestId('motor-angles-auslass')).toContainText('106');
    await expect(page.getByTestId('motor-steuerdiagramm-vorauslass')).toContainText('13');

    await page.getByTestId('motor-steuerdiagramm-preset').selectOption('mittel');
    await expect(page.getByTestId('motor-steuerdiagramm-auslass')).toHaveValue('165');
    await expect(page.getByTestId('motor-steuerdiagramm-vorauslass')).toContainText('22.5');
  });

  test('Umrechnung converts port timings between degrees and millimetres', async ({ page }) => {
    await openMotorWith(page, KR51);
    await page.getByTestId('module-tab-umrechnung').click();
    await page.getByTestId('motor-umrechnung-pleuel').fill('85');
    await expect(page.getByTestId('motor-umrechnung-hubradius')).toContainText('19.75');
    await page.getByTestId('motor-umrechnung-degree').fill('90');
    await expect(page.getByTestId('motor-umrechnung-distance')).not.toHaveText('–');

    await page.getByTestId('motor-umrechnung-port').selectOption('auslass');
    await expect(page.getByTestId('motor-umrechnung-vorher')).toHaveValue('148');
    await page.getByTestId('motor-umrechnung-nachher').fill('158');
    await expect(page.getByTestId('motor-umrechnung-oeffnet-nachher')).toContainText('101');
    await expect(page.getByTestId('motor-umrechnung-diff-degree')).toContainText('10');
    await expect(page.getByTestId('motor-umrechnung-diff-length')).not.toHaveText('–');
  });

  test('applies results to an own vehicle', async ({ page }) => {
    await navigateToTab(page, 'vehicles');
    await page.getByTestId(`vehicle-item-${KR51}`).click();
    await page.getByTestId('vehicle-duplicate').click();
    await expect(page.getByTestId('read-only-banner')).toHaveCount(0);

    await navigateToTab(page, 'motor');
    await page.getByTestId('module-tab-steuerdiagramm').click();
    await page.getByTestId('motor-steuerdiagramm-preset').selectOption('lang');
    await page.getByTestId('motor-steuerdiagramm-apply').click();
    await expect(page.getByText('Saved to vehicle')).toBeVisible();

    await navigateToTab(page, 'vehicles');
    await expect(page.getByTestId('vehicle-field-auslass-steuerzeitSZ')).toHaveValue('185');
    await expect(page.getByTestId('vehicle-field-einlass-steuerzeitSZ')).toHaveValue('160');
  });

  test('stays within a 375px viewport in English and German', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    const expectNoOverflow = async () => {
      for (const tab of ['hubraum', 'verdichtung', 'steuerdiagramm', 'umrechnung']) {
        await page.getByTestId(`module-tab-${tab}`).click();
        await expect(page.getByTestId(`motor-panel-${tab}`)).toBeVisible();
        const overflow = await page.evaluate(
          () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
        );
        expect(overflow).toBeLessThanOrEqual(0);
      }
    };
    await openMotorWith(page, KR51);
    await expectNoOverflow();

    await gotoProfileTab(page);
    await page.getByRole('tab', { name: 'Deutsch' }).click();
    await navigateToTab(page, 'motor');
    await expect(page.getByRole('heading', { name: 'Motor', exact: true })).toBeVisible();
    await expectNoOverflow();
  });
});
