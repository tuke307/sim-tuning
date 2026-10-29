ARG NODE_VERSION=24
FROM node:${NODE_VERSION}-slim AS base

# Stage 1: Prune monorepo to only what @sim-tuning/web needs
FROM base AS prepare
WORKDIR /app
RUN yarn global add turbo
COPY . .

ARG NEXT_PUBLIC_SITE_URL
ENV NEXT_PUBLIC_SITE_URL=${NEXT_PUBLIC_SITE_URL}

RUN turbo prune @sim-tuning/web --docker

# Stage 2: Install dependencies and build
FROM base AS installer
WORKDIR /app

COPY --from=prepare /app/out/json/ .
RUN yarn install

COPY --from=prepare /app/out/full/ .

ARG NEXT_PUBLIC_SITE_URL
ENV NEXT_PUBLIC_SITE_URL=${NEXT_PUBLIC_SITE_URL}

RUN yarn turbo build

# Stage 3: Production server
FROM node:${NODE_VERSION}-slim AS runner
WORKDIR /app
ENV NODE_ENV=production
# Run in UTC (see api Dockerfile): German wall-clock times are stored/read in
# the UTC slot of JS Dates, so day/hour math is only correct when TZ is UTC.
ENV TZ=UTC

RUN groupadd --system --gid 1001 nodejs
RUN useradd --system --uid 1001 --gid nodejs nextjs
USER nextjs

# Leverage Next.js output file tracing to reduce image size
COPY --from=installer --chown=nextjs:nodejs /app/apps/web/.next/standalone ./
COPY --from=installer --chown=nextjs:nodejs /app/apps/web/.next/static ./apps/web/.next/static
COPY --from=installer --chown=nextjs:nodejs /app/apps/web/public ./apps/web/public

# Re-declare ARGs so they are available at runtime (Next.js reads these server-side)
ARG NEXT_PUBLIC_SITE_URL

ENV NEXT_PUBLIC_SITE_URL=${NEXT_PUBLIC_SITE_URL}

EXPOSE 3000

CMD ["node", "apps/web/server.js"]
