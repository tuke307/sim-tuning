/**
 * Mailpit HTTP API client for E2E email-link verification tests.
 *
 * Mailpit (`compose.dev.yml` service `mailpit`) listens on:
 *   - SMTP: `localhost:1030`
 *   - HTTP API & Web UI: `http://localhost:8030`
 */

export const MAILPIT_URL = process.env.MAILPIT_URL ?? 'http://localhost:8030';

export interface MailpitSummaryMessage {
  ID: string;
  Subject: string;
  Created: string;
  Snippet: string;
}

export interface MailpitMessageDetail {
  ID: string;
  Subject: string;
  HTML: string;
  Text: string;
}

interface MailpitSearchResponse {
  messages?: MailpitSummaryMessage[];
}

/**
 * Delete all messages in Mailpit matching a specific recipient email.
 */
export async function clearMailboxForRecipient(recipientEmail: string): Promise<void> {
  const searchUrl = `${MAILPIT_URL}/api/v1/search?query=${encodeURIComponent(`to:${recipientEmail}`)}`;
  const searchRes = await fetch(searchUrl);
  if (!searchRes.ok) return;

  const data = (await searchRes.json()) as MailpitSearchResponse;
  const ids = (data.messages ?? []).map((m) => m.ID);
  if (ids.length === 0) return;

  await fetch(`${MAILPIT_URL}/api/v1/messages`, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ IDs: ids }),
  });
}

/**
 * Poll Mailpit until an email sent to `recipientEmail` whose subject matches
 * `subjectPattern` arrives, then return its full HTML and text body.
 */
export async function waitForMailpitEmail(
  recipientEmail: string,
  subjectPattern: RegExp,
  timeoutMs = 20_000,
): Promise<MailpitMessageDetail> {
  const deadline = Date.now() + timeoutMs;
  const searchUrl = `${MAILPIT_URL}/api/v1/search?query=${encodeURIComponent(`to:${recipientEmail}`)}`;

  while (Date.now() < deadline) {
    const res = await fetch(searchUrl);
    if (res.ok) {
      const data = (await res.json()) as MailpitSearchResponse;
      const match = (data.messages ?? []).find((msg) => subjectPattern.test(msg.Subject));
      if (match) {
        const detailRes = await fetch(`${MAILPIT_URL}/api/v1/message/${match.ID}`);
        if (detailRes.ok) {
          return (await detailRes.json()) as MailpitMessageDetail;
        }
      }
    }
    await new Promise((resolve) => setTimeout(resolve, 400));
  }

  throw new Error(
    `Timed out after ${timeoutMs}ms waiting for Mailpit email to "${recipientEmail}" matching ${subjectPattern}`,
  );
}

/**
 * Extract the relative action URL (`/verify-email?token=...`,
 * `/reset-password?token=...`, or `/delete-account?token=...`) from the
 * rendered MJML email HTML body.
 */
export function extractAuthLinkFromEmail(
  email: MailpitMessageDetail,
  routeSegment: 'verify-email' | 'reset-password' | 'delete-account',
): string {
  const source = `${email.HTML}\n${email.Text}`;
  const regex = new RegExp(
    `https?:\\/\\/[^"'\\s<>]+\\/${routeSegment}\\?token=([a-fA-F0-9]+)`,
    'i',
  );
  const match = regex.exec(source);
  if (!match || !match[1]) {
    throw new Error(`Could not find /${routeSegment}?token=... link in email "${email.Subject}"`);
  }
  return `/${routeSegment}?token=${match[1]}`;
}
