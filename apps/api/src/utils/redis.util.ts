import IORedis from 'ioredis';
import { REDIS_URL } from '@/config/env.config';

// Process-wide lazy Redis singleton for features that need it outside BullMQ
// (e.g. the access-token denylist). Created on first use so processes that
// never need it (and the test/build environments) don't open a connection.
let redis: IORedis | null = null;

export function getRedis(): IORedis {
  if (!redis) redis = new IORedis(REDIS_URL, { maxRetriesPerRequest: null });
  return redis;
}
