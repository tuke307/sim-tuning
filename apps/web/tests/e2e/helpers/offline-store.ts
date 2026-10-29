/**
 * Helpers for resetting and seeding the IndexedDB offline store
 * (`sim_tuning_offline_v1`) and localStorage state between E2E tests.
 */
import { expect, type Page } from '@playwright/test';
import { navigateToTab } from './shell.js';

/**
 * Clear IndexedDB (`sim_tuning_offline_v1`) and `localStorage`, then reload the
 * page so the app boots with a clean local state.
 */
export async function resetOfflineStore(page: Page): Promise<void> {
  await page.addInitScript(() => {
    const injectHideDevOverlay = () => {
      if (!document.head || document.getElementById('e2e-hide-nextjs-overlay')) return;
      const style = document.createElement('style');
      style.id = 'e2e-hide-nextjs-overlay';
      style.textContent = `
        nextjs-portal,
        [data-nextjs-dialog-overlay],
        #__next-build-indicator,
        [data-nextjs-dev-tools-button] {
          display: none !important;
          pointer-events: none !important;
        }
      `;
      document.head.appendChild(style);
    };
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', injectHideDevOverlay);
    } else {
      injectHideDevOverlay();
    }
  });
  await page.goto('/');
  await page.evaluate(async () => {
    window.localStorage.clear();
    window.sessionStorage.clear();
    await new Promise<void>((resolve) => {
      const req = window.indexedDB.deleteDatabase('sim_tuning_offline_v1');
      req.onsuccess = () => resolve();
      req.onerror = () => resolve();
      req.onblocked = () => resolve();
    });
  });
  await page.reload();
  await expect(page.getByRole('button', { name: 'SimTuning Home' })).toBeVisible({
    timeout: 15_000,
  });
}

/**
 * Mock an authenticated user session via API route interception and reload the
 * page so `AuthProvider` enters `AUTHENTICATED` mode deterministically (without
 * requiring an external SMTP server for email confirmation).
 */
export async function mockAuthenticatedSession(
  page: Page,
  overrides?: { email?: string; name?: string },
): Promise<void> {
  const mockUser = {
    id: 'user-e2e-1',
    email: overrides?.email ?? 'e2e.tuner@simtuning.test',
    name: overrides?.name ?? 'E2E Tuner',
    createdAt: new Date().toISOString(),
    lastLoginAt: new Date().toISOString(),
    preferences: {
      id: 'pref-e2e-1',
      userId: 'user-e2e-1',
      theme: 'dark',
      language: 'en',
      roundingAccuracy: 2,
      roundOnUnitChange: true,
    },
  };

  await page.route('**/api/auth/me', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true, data: mockUser }),
    });
  });

  await page.route('**/api/auth/preferences', async (route) => {
    const body = route.request().postDataJSON() ?? {};
    Object.assign(mockUser.preferences, body);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true, data: mockUser }),
    });
  });

  await page.goto('/');
  await expect(page.getByRole('button', { name: 'SimTuning Home' })).toBeVisible({
    timeout: 15_000,
  });
}

/**
 * Navigate to the Profile tab and verify whether the user is in Guest or
 * Authenticated mode.
 */
export async function gotoProfileTab(page: Page): Promise<void> {
  await navigateToTab(page, 'profile');
  await expect(
    page.getByRole('heading', { name: /Profile & Preferences|Profil & Einstellungen/i }),
  ).toBeVisible({ timeout: 10_000 });
}
