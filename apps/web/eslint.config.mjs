import { nextJsConfig } from '@sim-tuning/eslint-config/next';

/** @type {import("eslint").Linter.Config[]} */
export default [
  {
    ignores: ['.next/**'],
  },
  ...nextJsConfig,
];
