import * as v from 'valibot'

export const updatePanelSettingsSchema = v.object({
    name: v.pipe(
        v.string(),
        v.trim(),
        v.nonEmpty('Name is required'),
        v.maxLength(32, 'Name is too long'),
    ),
    url: v.optional(v.pipe(v.string(), v.trim(), v.url('Invalid url'))),
    allowRegistration: v.boolean(),
    allowAccountSelfDeletion: v.boolean(),
    smtp: v.object({
        host: v.optional(v.string()),
        port: v.pipe(
            v.number(),
            v.integer(),
            v.minValue(1, 'Port must be greater than 0'),
            v.maxValue(65535, 'Port must be less than 65536'),
        ),
        username: v.optional(v.string()),
        password: v.optional(v.string()),
        from: v.optional(v.pipe(v.string(), v.email('Invalid email'))),
    }),
})

export type UpdatePanelSettingsSchema = v.InferInput<typeof updatePanelSettingsSchema>
