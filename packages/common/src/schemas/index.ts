import { z } from 'zod';
import { EmailStatus, EmailTemplateType, LANGUAGE_PREFERENCES, THEME_PREFERENCES } from '../enums';

export const PASSWORD_RULES = {
  minLength: 8,
  description: 'At least 8 characters, containing at least one letter and one number.',
};

export const PasswordSchema = z
  .string()
  .min(PASSWORD_RULES.minLength, 'Password must be at least 8 characters')
  .regex(/[a-zA-Z]/, 'Password must contain at least one letter')
  .regex(/[0-9]/, 'Password must contain at least one number');

// ─────────────────────────────────────────────────────────────────────────────
// Auth & Preferences Schemas
// ─────────────────────────────────────────────────────────────────────────────
export const RegisterSchema = z.object({
  email: z.email('Please enter a valid email address'),
  password: PasswordSchema,
  name: z.string().trim().min(1, 'Name is required').max(80).optional(),
});

export const LoginSchema = z.object({
  email: z.email('Please enter a valid email address'),
  password: z.string().min(1, 'Password is required'),
});

export const ChangePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Current password is required'),
    newPassword: PasswordSchema,
  })
  .refine((data) => data.newPassword !== data.currentPassword, {
    message: 'New password must be different from current password',
    path: ['newPassword'],
  });

export const VerifyEmailSchema = z.object({
  token: z.string().trim().min(10, 'Verification token is required'),
});

export const ResendConfirmationSchema = z.object({
  email: z.email('Please enter a valid email address'),
});

export const ForgotPasswordSchema = z.object({
  email: z.email('Please enter a valid email address'),
});

export const ResetPasswordSchema = z.object({
  token: z.string().trim().min(10, 'Password reset token is required'),
  newPassword: PasswordSchema,
});

export const RequestAccountDeletionSchema = z.object({
  password: z.string().min(1, 'Current password is required to request account deletion'),
});

export const ConfirmAccountDeletionSchema = z.object({
  token: z.string().trim().min(10, 'Account deletion token is required'),
});

export const CreateEmailTemplateSchema = z.object({
  name: z.string().min(1),
  subject: z.string().min(1),
  htmlFile: z.string().min(1),
  type: z.enum(EmailTemplateType),
});

export const UpdateEmailTemplateSchema = CreateEmailTemplateSchema.partial();

export const CreateEmailSchema = z.object({
  recipientEmail: z.email(),
  subject: z.string().min(1),
  content: z.string().min(1),
  status: z.enum(EmailStatus).default(EmailStatus.PENDING),
  scheduledAt: z.string(),
  emailTemplateId: z.number().int(),
  userId: z.string().nullable().optional(),
});

export const UpdateEmailSchema = z.object({
  status: z.enum(EmailStatus).optional(),
  sentAt: z.string().optional(),
  attempts: z.number().int().optional(),
  lastAttemptAt: z.string().optional(),
  errorMessage: z.string().nullable().optional(),
});

/** Decimal places shown after a unit conversion (v2 `UnitSettings.RoundingAccuracy`). */
export const ROUNDING_ACCURACY = { min: 0, max: 6, default: 2 } as const;

export const UpdatePreferencesSchema = z.object({
  name: z.string().trim().min(1).max(80).optional(),
  theme: z.enum(THEME_PREFERENCES).optional(),
  language: z.enum(LANGUAGE_PREFERENCES).optional(),
  roundingAccuracy: z
    .number()
    .int()
    .min(ROUNDING_ACCURACY.min)
    .max(ROUNDING_ACCURACY.max)
    .optional(),
  roundOnUnitChange: z.boolean().optional(),
});

export const UserPreferencesSchema = z.object({
  id: z.string(),
  userId: z.string(),
  theme: z.string(),
  language: z.enum(LANGUAGE_PREFERENCES).default('system').optional(),
  roundingAccuracy: z.number().int(),
  roundOnUnitChange: z.boolean(),
});

export const UserSchema = z.object({
  id: z.string(),
  email: z.string(),
  name: z.string().nullable(),
  emailVerified: z.boolean().optional(),
  lastLoginAt: z.string().nullable().optional(),
  createdAt: z.string().optional(),
  preferences: UserPreferencesSchema.nullable().optional(),
});

export const AuthTokensSchema = z.object({
  accessToken: z.string(),
  refreshToken: z.string(),
  expiresAt: z.string(),
});

export const RefreshTokenBodySchema = z.object({
  refreshToken: z.string().min(1).optional(),
});

export const UserPayloadSchema = z.object({
  id: z.string(),
  email: z.string(),
});

export type RegisterDto = z.infer<typeof RegisterSchema>;
export type LoginDto = z.infer<typeof LoginSchema>;
export type ChangePasswordDto = z.infer<typeof ChangePasswordSchema>;
export type VerifyEmailDto = z.infer<typeof VerifyEmailSchema>;
export type ResendConfirmationDto = z.infer<typeof ResendConfirmationSchema>;
export type ForgotPasswordDto = z.infer<typeof ForgotPasswordSchema>;
export type ResetPasswordDto = z.infer<typeof ResetPasswordSchema>;
export type RequestAccountDeletionDto = z.infer<typeof RequestAccountDeletionSchema>;
export type ConfirmAccountDeletionDto = z.infer<typeof ConfirmAccountDeletionSchema>;
export type CreateEmailTemplateDto = z.infer<typeof CreateEmailTemplateSchema>;
export type UpdateEmailTemplateDto = z.infer<typeof UpdateEmailTemplateSchema>;
export type CreateEmailDto = z.infer<typeof CreateEmailSchema>;
export type UpdateEmailDto = z.infer<typeof UpdateEmailSchema>;
export type UpdatePreferencesDto = z.infer<typeof UpdatePreferencesSchema>;
export type UserPreferencesDto = z.infer<typeof UserPreferencesSchema>;
export type AuthTokensDto = z.infer<typeof AuthTokensSchema>;
export type RefreshTokenBodyDto = z.infer<typeof RefreshTokenBodySchema>;
export type UserDto = z.infer<typeof UserSchema> & {
  tokens?: AuthTokensDto;
};
export type UserPayloadDto = z.infer<typeof UserPayloadSchema>;

export interface RegisterResponseDto extends UserDto {
  requiresEmailConfirmation?: boolean;
  message?: string;
}

// ─────────────────────────────────────────────────────────────────────────────
// Web Analytics (first-party Umami pageview collector)
// ─────────────────────────────────────────────────────────────────────────────
export const ANALYTICS_MAX_URL_LENGTH = 500;

/** Drops query string and fragment so tokens/PII in URLs can never be forwarded. */
export const stripUrlQuery = (value: string): string => value.split(/[?#]/, 1)[0] ?? '';

/**
 * Allow-list of custom event names. Event data must stay low-cardinality and anonymous:
 * no names, e-mail addresses, free text or vehicle/measurement values.
 */
export const ANALYTICS_EVENT_NAMES = [
  'signup',
  'login',
  'password_reset_request',
  'account_delete_request',
  'offline_sync',
  'data_export',
  'pwa_installed',
] as const;

export type AnalyticsEventName = (typeof ANALYTICS_EVENT_NAMES)[number];

const analyticsUrlSchema = z
  .string()
  .max(ANALYTICS_MAX_URL_LENGTH)
  .regex(/^\/(?!\/)/, 'url must be an absolute path')
  .transform(stripUrlQuery);

const analyticsEventDataSchema = z
  .record(
    z.string().regex(/^[a-z][a-z0-9_]{0,39}$/),
    z.union([z.string().max(100), z.number().finite(), z.boolean()]),
  )
  .refine((data) => Object.keys(data).length <= 10, 'at most 10 data keys');

export const AnalyticsCollectSchema = z.discriminatedUnion('type', [
  z.strictObject({
    type: z.literal('pageview'),
    url: analyticsUrlSchema,
    referrer: z.string().max(ANALYTICS_MAX_URL_LENGTH).transform(stripUrlQuery).optional(),
  }),
  z.strictObject({
    type: z.literal('event'),
    name: z.enum(ANALYTICS_EVENT_NAMES),
    url: analyticsUrlSchema,
    data: analyticsEventDataSchema.optional(),
  }),
]);

export type AnalyticsCollectDto = z.infer<typeof AnalyticsCollectSchema>;
export * from './sim';
