import { defineConfig, devices } from '@playwright/test';
import { config } from 'dotenv';
import { resolve, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
config({ path: resolve(__dirname, '.env.local') });

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1,
  reporter: 'html',
  webServer: {
    // Bring up the full dev stack (api + web + workers) via turbo from the
    // monorepo root. Locally reuses an already-running `yarn dev`; CI starts
    // fresh. Raise timeout if the cold start (turbo + next dev + api + prisma)
    // needs longer.
    command: 'yarn run dev',
    cwd: resolve(__dirname, '../..'),
    url: 'http://localhost:3000',
    reuseExistingServer: !process.env.CI,
    timeout: 180_000,
  },
  use: {
    baseURL: process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    viewport: { width: 1920, height: 1080 },
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'], viewport: { width: 1920, height: 1080 } },
    },
    {
      name: 'mobile',
      testIgnore: /doc-screenshots\.spec\.ts/,
      use: {
        ...devices['iPhone 15 Pro'],
        defaultBrowserType: 'chromium',
      },
    },
  ],
});
