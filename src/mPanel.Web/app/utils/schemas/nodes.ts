import * as v from 'valibot'

export const createNodeSchema = v.object({
    name: v.pipe(
        v.string('Name is required'),
        v.trim(),
        v.nonEmpty('Name is required'),
        v.minLength(3, 'Name is too short'),
        v.maxLength(128, 'Name is too long'),
    ),
    scheme: v.picklist(['Http', 'Https']),
    address: v.pipe(v.string('Address is required'), v.trim(), v.nonEmpty('Address is required')),
    port: v.optional(
        v.pipe(
            v.number('Port must be a number'),
            v.integer('Port must be an integer'),
            v.minValue(1, 'Port must be at least 1'),
            v.maxValue(65535, 'Port must be at most 65535'),
        ),
    ),
    alias: v.optional(
        v.pipe(
            v.string(),
            v.trim(),
            v.maxLength(128, 'Alias is too long'),
            v.transform((v) => v || undefined),
        ),
    ),
    sftpPort: v.optional(
        v.pipe(
            v.number('SFTP port must be a number'),
            v.integer('SFTP port must be an integer'),
            v.minValue(1, 'SFTP port must be at least 1'),
            v.maxValue(65535, 'SFTP port must be at most 65535'),
        ),
    ),
    sftpAlias: v.optional(
        v.pipe(
            v.string(),
            v.trim(),
            v.maxLength(128, 'SFTP alias is too long'),
            v.transform((v) => v || undefined),
        ),
    ),
    maxMemoryMb: v.optional(
        v.pipe(
            v.number('Max memory must be a number'),
            v.minValue(0, 'Max memory must be at least 0'),
            v.transform((v) => v || undefined),
        ),
    ),
    maxDiskMb: v.optional(
        v.pipe(
            v.number('Max disk must be a number'),
            v.minValue(0, 'Max disk must be at least 0'),
            v.transform((v) => v || undefined),
        ),
    ),
    isMaintenanceMode: v.boolean(),
    isActive: v.boolean(),
})

export const updateNodeSchema = v.object({
    name: v.pipe(
        v.string('Name is required'),
        v.trim(),
        v.nonEmpty('Name is required'),
        v.minLength(3, 'Name is too short'),
        v.maxLength(128, 'Name is too long'),
    ),
    scheme: v.picklist(['Http', 'Https']),
    address: v.pipe(v.string('Address is required'), v.trim(), v.nonEmpty('Address is required')),
    port: v.optional(
        v.pipe(
            v.number('Port must be a number'),
            v.integer('Port must be an integer'),
            v.minValue(1, 'Port must be at least 1'),
            v.maxValue(65535, 'Port must be at most 65535'),
        ),
    ),
    alias: v.optional(
        v.pipe(
            v.string(),
            v.trim(),
            v.maxLength(128, 'Alias is too long'),
            v.transform((v) => v || undefined),
        ),
    ),
    sftpPort: v.optional(
        v.pipe(
            v.number('SFTP port must be a number'),
            v.integer('SFTP port must be an integer'),
            v.minValue(1, 'SFTP port must be at least 1'),
            v.maxValue(65535, 'SFTP port must be at most 65535'),
        ),
    ),
    sftpAlias: v.optional(
        v.pipe(
            v.string(),
            v.trim(),
            v.maxLength(128, 'SFTP alias is too long'),
            v.transform((v) => v || undefined),
        ),
    ),
    maxMemoryMb: v.optional(
        v.pipe(
            v.number('Max memory must be a number'),
            v.minValue(0, 'Max memory must be at least 0'),
            v.transform((v) => v || undefined),
        ),
    ),
    maxDiskMb: v.optional(
        v.pipe(
            v.number('Max disk must be a number'),
            v.minValue(0, 'Max disk must be at least 0'),
            v.transform((v) => v || undefined),
        ),
    ),
    isMaintenanceMode: v.boolean(),
    isActive: v.boolean(),
})

export type CreateNodeSchema = v.InferInput<typeof createNodeSchema>
