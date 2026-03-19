export type PagedResponse<T> = {
    items: T[]
    page: number
    pageSize: number
    totalCount: number
    totalPages: number
}

export type HealthResponse = {
    status: string
    totalDurationMs: number
    checks: HealthCheck[]
}

export type HealthCheck = {
    name: string
    status: string
    description: string | null
    durationMs: number
    exception: string | null
}

export type PanelPublicSettings = {
    name: string
    allowRegistration: boolean
    allowAccountSelfDeletion: boolean
}

export type PanelSettings = PanelPublicSettings & {
    url: string | null
    smtp: {
        host: string | null
        port: number
        username: string | null
        password: string | null
        from: string | null
    }
}

export type ProblemDetailsError = {
    name: string
    reason: string
    code: string | null
    severity: string | null
}

export type ProblemDetails = {
    type?: string
    title?: string
    status?: number
    instance?: string
    traceId?: string
    detail?: string | null
    errors?: ProblemDetailsError[]
}

export type Session = {
    id: string
    email: string
    username: string
    roles: string[]
}

export type CurrentUser = {
    id: string
    email: string
    username: string
    avatarUrl: string
    roles: string[]
    hasPassword: boolean
    emailConfirmed: boolean
    twoFactorEnabled: boolean
    createdAt: string
}

export type ApiKey = {
    id: string
    prefix: string
    name: string | null
    expiresAt: string | null
}

export type Node = {
    id: string
    name: string
    tokenPrefix: string
    scheme: 'Http' | 'Https' | string
    address: string
    port: number
    alias: string | null
    sftpPort: number
    sftpAlias: string | null
    maxMemoryMb: number | null
    maxDiskMb: number | null
    osName: string | null
    architecture: string | null
    cpuCores: number | null
    totalMemoryMb: number | null
    totalDiskMb: number | null
    isMaintenanceMode: boolean
    isActive: boolean
    handshakeError: string | null
    createdAt: string
    updatedAt: string
    lastHeartbeat: string | null
    lastHeartbeatCpuUsage: number | null
    lastHeartbeatMemoryMb: number | null
    lastHeartbeatDiskMb: number | null
}
