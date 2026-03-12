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
