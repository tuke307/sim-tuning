import { test, expect } from '@playwright/test';
import {
  submitLoginPageForm,
  TEST_USER_EMAIL,
  TEST_USER_PASSWORD,
  TEST_USER_NAME,
} from './helpers/auth.js';
import {
  gotoProfileTab,
  mockAuthenticatedSession,
  resetOfflineStore,
} from './helpers/offline-store.js';
import { expectSyncBadge } from './helpers/shell.js';

test.describe('Authentication & Guest Flows', () => {
  test.beforeEach(async ({ page }) => {
    await resetOfflineStore(page);
  });

  test('invalid credentials on /login show error and stay on /login', async ({ page }) => {
    await page.route('**/api/auth/login', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({
          success: false,
          error: 'Invalid email or password',
        }),
      });
    });

    await submitLoginPageForm(page, 'wrong@simtuning.test', TEST_USER_PASSWORD);

    await expect(page.getByText(/Invalid email or password|Unable to sign in/i)).toBeVisible({
      timeout: 5_000,
    });
    await expect(page).toHaveURL(/\/login/);
  });

  test('continue offline as guest from /login returns to PWA shell in Guest mode', async ({
    page,
  }) => {
    await page.goto('/login');
    await page.getByRole('button', { name: /Continue in Guest/i }).click();

    await expect(page).toHaveURL(/\/$/);
    await expectSyncBadge(page, /Guest/i);
  });

  test('registration requiring email confirmation shows confirmation banner and resend button', async ({
    page,
  }) => {
    await page.route('**/api/auth/register', async (route) => {
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          data: {
            requiresEmailConfirmation: true,
            message:
              'Account created! Please check your email and click the confirmation link to activate your account.',
          },
        }),
      });
    });

    await page.goto('/login');
    await page.getByRole('tab', { name: /Create Account|Konto erstellen/i }).click();
    await page.locator('#name').fill(TEST_USER_NAME);
    await page.locator('#email').fill(TEST_USER_EMAIL);
    await page.locator('#password').fill(TEST_USER_PASSWORD);
    await page.getByRole('button', { name: /Register & Send Confirmation Link/i }).click();

    await expect(page.getByText(/check your email/i)).toBeVisible({ timeout: 5_000 });
    await expect(page.getByRole('button', { name: /Resend Confirmation/i })).toBeVisible();
  });

  test('authenticated user shows Synced badge and can sign out back to Guest mode', async ({
    page,
  }) => {
    await mockAuthenticatedSession(page, {
      email: TEST_USER_EMAIL,
      name: TEST_USER_NAME,
    });

    await page.route('**/api/auth/logout', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true }),
      });
    });

    await expectSyncBadge(page, /Synced|Saved Locally/i);
    await gotoProfileTab(page);

    await expect(page.getByText(TEST_USER_EMAIL)).toBeVisible();
    await expect(page.getByText('Signed in', { exact: true })).toBeVisible();

    await page.getByRole('button', { name: /^(Sign Out|Abmelden)$/i }).click();
    await expectSyncBadge(page, /Guest/i);
  });
});
