import { test, expect, type Page } from '@playwright/test';
import { confirmAlertDialog } from './helpers/dialogs.js';
import { resetOfflineStore } from './helpers/offline-store.js';
import { navigateToTab } from './helpers/shell.js';

async function openVehicles(page: Page) {
  await navigateToTab(page, 'vehicles');
  await expect(page.getByRole('heading', { name: 'Vehicles', level: 1 })).toBeVisible();
  await expect(page.getByTestId('vehicle-list')).toBeVisible();
}

async function createVehicle(page: Page, name: string, motorName?: string) {
  await page.getByTestId('vehicle-create').click();
  const dialog = page.getByTestId('vehicle-create-dialog');
  await expect(dialog).toBeVisible();
  await dialog.getByLabel('Name', { exact: true }).fill(name);
  if (motorName) await dialog.getByLabel('Engine name').fill(motorName);
  await dialog.getByTestId('vehicle-create-submit').click();
  await expect(dialog).not.toBeVisible();
}

test.describe('Vehicles module (guest mode)', () => {
  test.beforeEach(async ({ page }) => {
    await resetOfflineStore(page);
    await openVehicles(page);
  });

  test('lists starter vehicles read-only and duplicates one to edit', async ({ page }) => {
    const starter = page.getByTestId('vehicle-item-sys-veh-s51b1-4');
    await expect(starter).toBeVisible();
    await starter.click();
    await expect(starter).toHaveAttribute('aria-current', 'true');
    await expect(page.getByTestId('read-only-banner')).toBeVisible();
    await expect(page.getByTestId('vehicle-field-name')).toHaveAttribute('readonly', '');
    await expect(page.getByTestId('vehicle-field-motor-hubL')).toHaveValue('44');
    await expect(page.getByTestId('vehicle-delete')).toHaveCount(0);

    await page.getByTestId('vehicle-duplicate').click();
    await expect(page.getByTestId('read-only-banner')).toHaveCount(0);
    const nameField = page.getByTestId('vehicle-field-name');
    await expect(nameField).toHaveValue(/\(copy\)$/);
    await expect(nameField).not.toHaveAttribute('readonly', '');
    await expect(page.getByTestId('vehicle-field-motor-hubL')).toHaveValue('44');
  });

  test('creates, edits, saves and deletes an own vehicle', async ({ page }) => {
    await createVehicle(page, 'E2E Racer', 'M 541');
    const nameField = page.getByTestId('vehicle-field-name');
    await expect(nameField).toHaveValue('E2E Racer');
    await expect(page.getByTestId('vehicle-field-motor-name')).toHaveValue('M 541');

    await page.getByTestId('vehicle-field-motor-hubL').fill('44');
    await page.getByTestId('vehicle-field-auslass-steuerzeitSZ').fill('172');
    await page.getByTestId('vehicle-field-ueberstroemer-anzahl').fill('4');
    await expect(page.getByTestId('vehicle-save-status')).toHaveText('Unsaved changes');
    await page.getByRole('button', { name: 'Save', exact: true }).click();
    await expect(page.getByTestId('vehicle-save-status')).toHaveText('Saved');

    // Persisted in IndexedDB: survives a reload.
    await page.reload();
    await openVehicles(page);
    await expect(page.getByTestId('vehicle-field-name')).toHaveValue('E2E Racer');
    await expect(page.getByTestId('vehicle-field-auslass-steuerzeitSZ')).toHaveValue('172');
    await expect(page.getByTestId('vehicle-field-ueberstroemer-anzahl')).toHaveValue('4');

    await page.getByTestId('vehicle-delete').click();
    await expect(page.getByTestId('vehicle-delete-dialog')).toContainText('E2E Racer');
    await confirmAlertDialog(page, 'Delete');
    await expect(page.getByTestId('vehicle-list')).not.toContainText('E2E Racer');
  });

  test('blocks saving without a name', async ({ page }) => {
    await createVehicle(page, 'Nameless soon');
    await page.getByTestId('vehicle-field-name').fill('');
    await expect(page.getByTestId('vehicle-save-status')).toHaveText('Please enter a name.');
    await expect(page.getByRole('button', { name: 'Save', exact: true })).toBeDisabled();
  });

  test('stays within a 375px viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    for (const tab of ['vehicles', 'environments']) {
      await page.getByTestId(`module-tab-${tab}`).click();
      const overflow = await page.evaluate(
        () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
      );
      expect(overflow).toBeLessThanOrEqual(0);
    }
  });

  test('manages own environments next to the read-only starters', async ({ page }) => {
    await page.getByTestId('module-tab-environments').click();
    const list = page.getByTestId('environment-list');
    await expect(list).toContainText('Sommer');

    await page.getByTestId('environment-create').click();
    const dialog = page.getByTestId('environment-dialog');
    await dialog.getByTestId('environment-name').fill('Track day');
    await dialog.getByTestId('environment-temperatur').fill('18');
    await dialog.getByTestId('environment-luftdruck').fill('995');
    await dialog.getByTestId('environment-submit').click();
    await expect(dialog).not.toBeVisible();
    await expect(list).toContainText('Track day');
    await expect(list).toContainText('18 °C');

    await page.getByRole('button', { name: 'Edit: Track day' }).click();
    await page.getByTestId('environment-name').fill('Track day 2');
    await page.getByTestId('environment-submit').click();
    await expect(list).toContainText('Track day 2');

    await page.getByRole('button', { name: 'Delete: Track day 2' }).click();
    await confirmAlertDialog(page, 'Delete');
    await expect(list).not.toContainText('Track day');
  });
});
