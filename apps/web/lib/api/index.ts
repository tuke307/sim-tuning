import { apiFetch } from './client';
import type {
  ChangePasswordDto,
  ConfirmAccountDeletionDto,
  ForgotPasswordDto,
  LoginDto,
  RegisterDto,
  RegisterResponseDto,
  RequestAccountDeletionDto,
  ResendConfirmationDto,
  ResetPasswordDto,
  UpdatePreferencesDto,
  UserDto,
  VerifyEmailDto,
} from '@sim-tuning/common/schemas';

export const auth = {
  register: (data: RegisterDto) =>
    apiFetch<RegisterResponseDto>('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
      retry: false,
    }),
  verifyEmail: (data: VerifyEmailDto) =>
    apiFetch<UserDto>('/auth/verify-email', {
      method: 'POST',
      body: JSON.stringify(data),
      retry: false,
    }),
  resendConfirmation: (data: ResendConfirmationDto) =>
    apiFetch<{ success: boolean; message: string }>('/auth/resend-confirmation', {
      method: 'POST',
      body: JSON.stringify(data),
      retry: false,
    }),
  forgotPassword: (data: ForgotPasswordDto) =>
    apiFetch<{ success: boolean; message: string }>('/auth/forgot-password', {
      method: 'POST',
      body: JSON.stringify(data),
      retry: false,
    }),
  resetPassword: (data: ResetPasswordDto) =>
    apiFetch<{ success: boolean; message: string }>('/auth/reset-password', {
      method: 'POST',
      body: JSON.stringify(data),
      retry: false,
    }),
  login: (data: LoginDto) =>
    apiFetch<UserDto>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
      retry: false,
    }),
  me: () => apiFetch<UserDto>('/auth/me'),
  updatePreferences: (data: UpdatePreferencesDto) =>
    apiFetch<UserDto>('/auth/preferences', {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),
  changePassword: (data: ChangePasswordDto) =>
    apiFetch<{ success: boolean }>('/auth/change-password', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  requestAccountDeletion: (data: RequestAccountDeletionDto) =>
    apiFetch<{ success: boolean; message: string }>('/auth/request-delete-account', {
      method: 'POST',
      body: JSON.stringify(data),
      retry: false,
    }),
  confirmAccountDeletion: (data: ConfirmAccountDeletionDto) =>
    apiFetch<{ success: boolean; message: string }>('/auth/confirm-delete-account', {
      method: 'POST',
      body: JSON.stringify(data),
      retry: false,
    }),
  logout: () =>
    apiFetch<{ success: boolean }>('/auth/logout', {
      method: 'POST',
    }),
  refreshToken: () =>
    apiFetch<{ success: boolean }>('/auth/refresh-token', {
      method: 'POST',
      retry: false,
    }),
};

export * from './client';

export const api = {
  auth,
};
