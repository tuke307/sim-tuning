// Client-safe utility exports - These utilities can be safely imported in Next.js client components

// Date and time utilities (client-safe)
export * from './time.util';
export * from './date.utils';

// Locale resolution (client-safe)
export * from './locale.util';

// Zod utilities (client-safe)
export * from './zod-utils';
export * from './round.util';

// Note: Pipes and decorators are NOT exported here as they contain NestJS server-side code
// Server-side utilities can still be imported via '@sim-tuning/common/utils' for server components and API routes
