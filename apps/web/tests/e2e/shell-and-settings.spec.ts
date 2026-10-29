import { test, expect } from '@playwright/test';
import { gotoProfileTab, resetOfflineStore } from './helpers/offline-store.js';
import { expectSyncBadge, navigateToTab } from './helpers/shell.js';

test.describe('PWA Shell, Navigation & Preferences', () => {
  test.beforeEach(async ({ page }) => {
    await resetOfflineStore(page);
  });

  test('boots in guest mode, lists the modules and navigates between tabs', async ({ page }) => {
    await expectSyncBadge(page, /Guest/i);
    await expect(page.getByRole('heading', { name: 'Modules' })).toBeVisible();
    for (const key of ['vehicles', 'motor', 'einlass', 'auslass', 'dyno']) {
      await expect(page.getByTestId(`home-module-${key}`)).toBeVisible();
    }

    await page.getByTestId('home-module-motor').click();
    await expect(page.getByRole('heading', { name: 'Engine', exact: true })).toBeVisible();

    for (const tab of ['vehicles', 'einlass', 'auslass', 'dyno'] as const) {
      await navigateToTab(page, tab);
    }

    await navigateToTab(page, 'profile');
    await expect(page.getByRole('heading', { name: 'Profile & Preferences' })).toBeVisible();

    await page.getByRole('button', { name: 'SimTuning Home' }).click();
    await expect(page.getByRole('heading', { name: 'Modules' })).toBeVisible();
  });

  test('can switch language between English and German across views', async ({ page }) => {
    await gotoProfileTab(page);

    await page.getByRole('tab', { name: 'Deutsch' }).click();
    await expect(page.getByRole('heading', { name: /Profil & Einstellungen/i })).toBeVisible();

    await navigateToTab(page, 'home');
    await expect(page.getByRole('heading', { name: 'Module' })).toBeVisible();

    await navigateToTab(page, 'profile');
    await page.getByRole('tab', { name: 'English' }).click();
    await expect(page.getByRole('heading', { name: 'Profile & Preferences' })).toBeVisible();
  });

  test('can change theme and unit rounding preferences', async ({ page }) => {
    await gotoProfileTab(page);

    await page.getByRole('tab', { name: 'Light' }).click();
    await expect(page.getByRole('tab', { name: 'Light' })).toHaveAttribute('data-state', 'active');
    await page.getByRole('tab', { name: 'Dark' }).click();

    const rounding = page.getByTestId('profile-rounding-accuracy');
    await expect(rounding).toHaveValue('2');
    await rounding.selectOption('4');
    await expect(rounding).toHaveValue('4');

    const roundTabs = page.getByTestId('profile-round-on-unit-change-tabs');
    await roundTabs.getByRole('tab', { name: 'Off' }).click();
    await expect(roundTabs.getByRole('tab', { name: 'Off' })).toHaveAttribute(
      'data-state',
      'active',
    );
  });
});
