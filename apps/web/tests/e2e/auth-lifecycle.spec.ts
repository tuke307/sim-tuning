import { test, expect } from '@playwright/test';
import {
  clearMailboxForRecipient,
  extractAuthLinkFromEmail,
  MAILPIT_URL,
  waitForMailpitEmail,
} from './helpers/mailpit.js';
import { gotoProfileTab, resetOfflineStore } from './helpers/offline-store.js';
import { expectSyncBadge } from './helpers/shell.js';

test.describe('Full Account Lifecycle via Mailpit Emails', () => {
  test.describe.configure({ mode: 'serial' });

  test('registers account, confirms email via Mailpit link, resets password via email, and deletes account via email', async ({
    page,
  }) => {
    await resetOfflineStore(page);

    const uniqueId = `${Date.now()}-${Math.floor(Math.random() * 10_000)}`;
    const email = `e2e.lifecycle.${uniqueId}@simtuning.test`;
    const initialPassword = 'InitialPass123!';
    const updatedPassword = 'UpdatedPass456!';

    await clearMailboxForRecipient(email);

    // ── 1. Register new account on /login ──
    await page.goto('/login');
    await page.getByRole('tab', { name: /Create Account|Konto erstellen/i }).click();
    await page.locator('#name').fill('E2E Mailpit Tuner');
    await page.locator('#email').fill(email);
    await page.locator('#password').fill(initialPassword);
    await page.getByRole('button', { name: /Register & Send Confirmation Link/i }).click();

    await expect(page.getByText(/check your email/i)).toBeVisible({ timeout: 10_000 });

    // ── 2. Attempting to sign in before clicking the email link is blocked ──
    await page.getByRole('tab', { name: /^Sign In$/i }).click();
    await page.locator('#email').fill(email);
    await page.locator('#password').fill(initialPassword);
    await page.getByRole('button', { name: /^Sign In$/i }).click();
    await expect(page.getByText(/confirm your email/i)).toBeVisible({ timeout: 10_000 });

    // ── 3. Read Account Confirmation email from Mailpit & open verification link ──
    const confirmEmail = await waitForMailpitEmail(email, /Confirm your account/i);
    const verifyPath = extractAuthLinkFromEmail(confirmEmail, 'verify-email');

    // Also verify Mailpit Web UI renders the email cleanly
    await page.goto(MAILPIT_URL);
    await expect(page.getByText(email).first()).toBeVisible({ timeout: 10_000 });

    // Click/visit the confirmation URL from the email
    await page.goto(verifyPath);
    await expect(page.getByRole('heading', { name: 'Email Confirmed!' })).toBeVisible({
      timeout: 15_000,
    });
    await page.getByRole('button', { name: /Open SimTuning Dashboard/i }).click();

    await expect(page).toHaveURL(/\/$/);
    await expectSyncBadge(page, /Synced|Saved Locally/i);

    // ── 4. Sign out and request a password reset email (/forgot-password) ──
    await gotoProfileTab(page);
    await expect(page.getByText('Signed in', { exact: true })).toBeVisible();
    await page.getByRole('button', { name: /^Sign Out$/i }).click();
    await expectSyncBadge(page, /Guest/i);

    await clearMailboxForRecipient(email);
    await page.goto('/forgot-password');
    await page.locator('#reset-email').fill(email);
    await page.getByRole('button', { name: /Send Password Reset Link/i }).click();
    await expect(page.getByText(/password reset link/i)).toBeVisible({ timeout: 10_000 });

    // ── 5. Read Password Reset email from Mailpit & set new password (/reset-password) ──
    const resetEmail = await waitForMailpitEmail(email, /Reset your password/i);
    const resetPath = extractAuthLinkFromEmail(resetEmail, 'reset-password');

    await page.goto(resetPath);
    await expect(page.getByRole('heading', { name: 'Reset Your Password' })).toBeVisible();
    await page.locator('#new-password').fill(updatedPassword);
    await page.locator('#confirm-password').fill(updatedPassword);
    await page.getByRole('button', { name: /Save New Password/i }).click();

    await expect(page.getByText(/Password has been reset/i)).toBeVisible({ timeout: 10_000 });

    // ── 6. Verify old password fails and new password succeeds ──
    await page.goto('/login');
    await page.locator('#email').fill(email);
    await page.locator('#password').fill(initialPassword);
    await page.getByRole('button', { name: /^Sign In$/i }).click();
    await expect(page.getByText(/Invalid email or password/i)).toBeVisible({ timeout: 10_000 });

    await page.locator('#password').fill(updatedPassword);
    await page.getByRole('button', { name: /^Sign In$/i }).click();
    await expect(page).toHaveURL(/\/$/, { timeout: 10_000 });
    await expectSyncBadge(page, /Synced|Saved Locally/i);

    // ── 7. Request Account Deletion from Profile tab ──
    await clearMailboxForRecipient(email);
    await gotoProfileTab(page);
    await page.getByRole('button', { name: 'Delete Account' }).click();

    const deleteModal = page.getByRole('dialog');
    await expect(deleteModal).toBeVisible();
    await deleteModal.locator('#delete-account-password').fill(updatedPassword);
    await deleteModal.getByRole('button', { name: /Send Deletion Confirmation Email/i }).click();
    await expect(deleteModal).not.toBeVisible({ timeout: 10_000 });

    // ── 8. Read Account Deletion email from Mailpit & permanently delete account ──
    const deleteEmail = await waitForMailpitEmail(email, /Confirm account deletion/i);
    const deletePath = extractAuthLinkFromEmail(deleteEmail, 'delete-account');

    await page.goto(deletePath);
    await expect(
      page.getByRole('heading', { name: 'Confirm Permanent Account Deletion' }),
    ).toBeVisible();
    await page.getByRole('button', { name: /Yes, Permanently Delete My Account/i }).click();

    await expect(page.getByRole('heading', { name: 'Account Permanently Deleted' })).toBeVisible({
      timeout: 10_000,
    });
    await page.getByRole('button', { name: /Return to SimTuning/i }).click();
    await expectSyncBadge(page, /Guest/i);

    // Deleted account can no longer sign in
    await page.goto('/login');
    await page.locator('#email').fill(email);
    await page.locator('#password').fill(updatedPassword);
    await page.getByRole('button', { name: /^Sign In$/i }).click();
    await expect(page.getByText(/Invalid email or password/i)).toBeVisible({ timeout: 10_000 });

    await clearMailboxForRecipient(email);
  });
});
