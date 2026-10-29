/**
 * Authentication helpers for `/login` and Profile tab auth flows.
 */
import { expect, type Page } from '@playwright/test';
import { TEST_USER_EMAIL, TEST_USER_PASSWORD, TEST_USER_NAME } from './test-users.js';

export { TEST_USER_EMAIL, TEST_USER_PASSWORD, TEST_USER_NAME };

/**
 * Fill and submit the standalone `/login` form.
 */
export async function submitLoginPageForm(
  page: Page,
  email: string,
  password: string,
): Promise<void> {
  await page.goto('/login');
  await expect(page.locator('#email')).toBeVisible({ timeout: 10_000 });
  await page.locator('#email').fill(email);
  await page.locator('#password').fill(password);
  await page.getByRole('button', { name: /Sign In|Anmelden/i }).click();
}
