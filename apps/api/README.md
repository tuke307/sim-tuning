# `@sim-tuning/api`

NestJS 12 REST API for SimTuning.

## Features

- Dual authentication: `HttpOnly` cookies + constant-time CSRF protection for web browsers, and `Authorization: Bearer` + refresh token rotation for native iOS clients.
- Swagger / OpenAPI documentation at `/api-docs`.

## Development

```bash
cp .env.sample .env.local
yarn dev
yarn test
```
