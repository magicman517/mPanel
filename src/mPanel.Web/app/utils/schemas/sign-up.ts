import * as v from 'valibot'

export const signUpSchema = v.object({
  email: v.pipe(
    v.string('Email is required'),
    v.trim(),
    v.nonEmpty('Email is required'),
    v.email('Invalid email format'),
  ),
  username: v.pipe(
    v.string('Username is required'),
    v.trim(),
    v.nonEmpty('Username is required'),
    v.minLength(3, 'Username is too short'),
    v.maxLength(64, 'Username is too long'),
  ),
  password: v.pipe(
    v.string('Password is required'),
    v.trim(),
    v.nonEmpty('Password is required'),
    v.minLength(6, 'Password is too short'),
    v.maxLength(128, 'Password is too long'),
  ),
})

export type SignUpSchema = v.InferInput<typeof signUpSchema>
