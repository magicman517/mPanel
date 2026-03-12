import * as v from 'valibot'

export const signInSchema = v.object({
  identity: v.pipe(
    v.string('Email or Username is required'),
    v.trim(),
    v.nonEmpty('Email or Username is required'),
  ),
  password: v.pipe(v.string('Password is required'), v.trim(), v.nonEmpty('Password is required')),
})

export type SignInSchema = v.InferInput<typeof signInSchema>
