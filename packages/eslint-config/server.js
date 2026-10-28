import globals from 'globals';
import { config as baseConfig } from './base.js';

/**
 * A custom ESLint configuration for api services.
 *
 * @type {import("eslint").Linter.Config[]} */
export const serverConfig = [
  ...baseConfig,
  {
    languageOptions: {
      globals: {
        ...globals.serviceworker,
        ...globals.browser,
      },
    },
  },
];
