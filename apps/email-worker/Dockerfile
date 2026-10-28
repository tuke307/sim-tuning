ARG NODE_VERSION=24
FROM node:${NODE_VERSION}-slim AS base

# Stage 1: Prune monorepo to only what @sim-tuning/email-worker needs
FROM base AS prepare
WORKDIR /app
RUN yarn global add turbo
COPY . .
RUN turbo prune @sim-tuning/email-worker --docker

# Stage 2: Install dependencies and build
FROM base AS builder
WORKDIR /app

COPY --from=prepare /app/out/json/ .
RUN yarn install

COPY --from=prepare /app/out/full/ .

RUN yarn workspace @sim-tuning/database db:generate
RUN yarn turbo build

# Stage 3: Production runner
FROM node:${NODE_VERSION}-slim AS runner
WORKDIR /app
ENV NODE_ENV=production
ENV TZ=UTC

RUN groupadd --system --gid 1001 nodejs
RUN useradd --system --uid 1001 --gid nodejs nestjs

USER nestjs

COPY --from=builder --chown=nestjs:nodejs /app .

CMD ["node", "apps/email-worker/dist/src/main.js"]
