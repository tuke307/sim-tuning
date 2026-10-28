/** Shared test-user configuration for all e2e specs. */

export const TEST_USER_EMAIL = process.env.TEST_USER_EMAIL ?? 'e2e.tuner@simtuning.test';
export const TEST_USER_PASSWORD =
  process.env.TEST_USER_PASSWORD ?? 'FAKE-PASSWORD-FOR-TESTING-123!';
export const TEST_USER_NAME = process.env.TEST_USER_NAME ?? 'E2E Tuner';
