# `@sim-tuning/database`

Prisma 7 PostgreSQL schema, migrations, and seed data for SimTuning.

## Local Setup

```bash
cp .env.sample .env.local
yarn db:push
yarn db:seed
```

## Migrations

```bash
# Create a new migration in development
yarn db:migrate:local -- <migration-name>

# Apply pending migrations
yarn db:deploy:local

# Regenerate Prisma Client
yarn db:generate
```
