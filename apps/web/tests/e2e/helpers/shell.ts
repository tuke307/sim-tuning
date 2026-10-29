/**
 * Shared navigation and PWA shell helpers for desktop (`chromium`) and mobile
 * (`mobile-safari`) viewports.
 */
import { expect, type Page } from '@playwright/test';

export type ShellTab = 'home' | 'vehicles' | 'motor' | 'einlass' | 'auslass' | 'dyno' | 'profile';

/**
 * Navigate to a main PWA tab, automatically picking the desktop horizontal nav
 * button or the mobile bottom tab bar button depending on viewport visibility.
 */
export async function navigateToTab(page: Page, tab: ShellTab): Promise<void> {
  // Profile lives in the header avatar on every viewport, so it uses a single shared test id.
  // Home is reached through the brand button in the header.
  if (tab === 'home') {
    await page.getByRole('button', { name: /SimTuning (Home|Start)/ }).click();
    return;
  }
  if (tab === 'profile') {
    const sharedBtn = page.getByTestId(`nav-${tab}`);
    await expect(sharedBtn).toBeVisible({ timeout: 10_000 });
    await sharedBtn.click();
    return;
  }

  const desktopBtn = page.getByTestId(`desktop-nav-${tab}`);
  const mobileBtn = page.getByTestId(`mobile-nav-${tab}`);

  if (await desktopBtn.isVisible()) {
    await desktopBtn.click();
  } else {
    await expect(mobileBtn).toBeVisible({ timeout: 10_000 });
    await mobileBtn.click();
  }
}

/**
 * Assert the header sync/connection status contains the expected text. The status is always
 * announced to assistive tech, but only shown visually when the user can act on it.
 */
export async function expectSyncBadge(
  page: Page,
  expectedText: string | RegExp,
  timeout = 10_000,
): Promise<void> {
  const badge = page.getByTestId('header-sync-status');
  await expect(badge).toBeAttached({ timeout });
  await expect(badge).toContainText(expectedText, { timeout });
}
