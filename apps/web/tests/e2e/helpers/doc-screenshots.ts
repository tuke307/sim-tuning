/**
 * Shared infrastructure for the documentation screenshot specs.
 *
 * Mirrors `marienmuehle` and `kds-werkzeugbau`: waits for network idle,
 * hides the Next.js dev overlay, and captures clean viewport screenshots.
 */
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import type { Page } from '@playwright/test';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

/** Directory documentation screenshots are written to (`tests/e2e/doc-screenshots`). */
export const OUT = path.resolve(__dirname, '..', 'doc-screenshots');

/**
 * Capture a documentation screenshot after letting the page settle.
 * Hides the Next.js dev overlay so it never bleeds into the docs.
 */
export async function shot(page: Page, name: string): Promise<void> {
  fs.mkdirSync(OUT, { recursive: true });
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(800);
  await page.addStyleTag({
    content: `
      nextjs-portal,
      [data-nextjs-dialog-overlay],
      #__next-build-indicator,
      button[data-nextjs-toast-errors-hide-button],
      [data-nextjs-dev-tools-button] { display: none !important; }
    `,
  });
  await page.screenshot({ path: path.join(OUT, `${name}.png`), fullPage: false });
}
