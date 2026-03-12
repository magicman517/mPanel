import * as v from 'valibot'

export const currentUserSchema = v.object({
  email: v.pipe(
    v.string('Email is required'),
    v.trim(),
    v.nonEmpty('Email is required'),
    v.email('Invalid email'),
  ),
  username: v.pipe(
    v.string('Username is required'),
    v.trim(),
    v.nonEmpty('Username is required'),
    v.minLength(3, 'Username is too short'),
    v.maxLength(64, 'Username is too long'),
  ),
})

export const updatePasswordSchema = v.pipe(
  v.object({
    currentPassword: v.pipe(
      v.string('Current password is required'),
      v.trim(),
      v.nonEmpty('Current password is required'),
    ),
    newPassword: v.pipe(
      v.string('New password is required'),
      v.trim(),
      v.nonEmpty('New password is required'),
      v.minLength(6, 'Password is too short'),
      v.maxLength(128, 'Password is too long'),
    ),
    confirmNewPassword: v.pipe(
      v.string('Field cannot be empty'),
      v.trim(),
      v.nonEmpty('Field cannot be empty'),
    ),
  }),
  v.forward(
    v.partialCheck(
      [['newPassword'], ['confirmNewPassword']],
      (input) => input.newPassword === input.confirmNewPassword,
      'Passwords do not match',
    ),
    ['confirmNewPassword'],
  ),
)

export type CurrentUserSchema = v.InferInput<typeof currentUserSchema>
export type UpdatePasswordSchema = v.InferInput<typeof updatePasswordSchema>
