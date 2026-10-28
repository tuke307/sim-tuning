/**
 * Shared dialog & alert-dialog helpers modeled after `helpers/table.ts` in
 * `marienmuehle` and `kds-werkzeugbau`.
 */
import { expect, type Page } from '@playwright/test';

/**
 * Click a button inside the open `dialog` (by name) and wait for the dialog to
 * close.
 */
export async function clickDialogButton(
  page: Page,
  name: RegExp | string,
  timeout = 10_000,
): Promise<void> {
  const dialog = page.getByRole('dialog');
  await expect(dialog).toBeVisible({ timeout });
  await dialog.getByRole('button', { name }).click();
  await expect(dialog).not.toBeVisible({ timeout });
}

/**
 * Click a confirmation button inside an open `alertdialog` (or `dialog`) and
 * wait for it to close.
 */
export async function confirmAlertDialog(
  page: Page,
  name: RegExp | string = /^(Delete|Löschen|Discard|Verwerfen)$/,
  timeout = 10_000,
): Promise<void> {
  const modal = page.getByRole('alertdialog').or(page.getByRole('dialog'));
  await expect(modal).toBeVisible({ timeout });
  await modal.getByRole('button', { name }).click();
  await expect(modal).not.toBeVisible({ timeout });
}
