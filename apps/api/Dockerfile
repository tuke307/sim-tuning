ARG NODE_VERSION=24
FROM node:${NODE_VERSION}-slim AS base

# Stage 1: Prune monorepo to only what @sim-tuning/api needs
FROM base AS prepare
WORKDIR /app
RUN yarn global add turbo
COPY . .
RUN turbo prune @sim-tuning/api --docker

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
# Run in UTC: the app packs German wall-clock times into the UTC slot of JS
# Dates, so day/hour math (getUTCHours/Date.UTC) is only correct when the
# process TZ is UTC. node:-slim defaults to UTC; this makes it explicit.
ENV TZ=UTC

# Create uploads directory and non-root user
RUN mkdir -p /app/uploads
RUN groupadd --system --gid 1001 nodejs
RUN useradd --system --uid 1001 --gid nodejs nestjs
RUN chown -R nestjs:nodejs /app/uploads

USER nestjs

COPY --from=builder --chown=nestjs:nodejs /app .

# Important: Run the compiled NestJS output directly in production
CMD ["node", "apps/api/dist/src/main.js"]
