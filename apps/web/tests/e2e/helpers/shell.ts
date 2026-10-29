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

/** URL of a shell view: the active view lives in `?m=` (Home is the bare root). */
export function shellViewUrl(tab: ShellTab): string {
  return tab === 'home' ? '/' : `/?m=${tab}`;
}

/** Open a view directly by URL (deep link / reload behaviour). */
export async function gotoView(page: Page, tab: ShellTab): Promise<void> {
  await page.goto(shellViewUrl(tab));
}

/** Assert the URL reflects the given view, e.g. after back/forward navigation. */
export async function expectView(page: Page, tab: ShellTab, timeout = 10_000): Promise<void> {
  if (tab === 'home') {
    await expect(page).not.toHaveURL(/[?&]m=/, { timeout });
    return;
  }
  await expect(page).toHaveURL(new RegExp(`[?&]m=${tab}(&|$)`), { timeout });
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
