export function getProblemDetailsMessage(err: unknown): string {
  const typedErr = err as { data?: ProblemDetails; status?: number }
  const data = typedErr?.data
  const status = data?.status ?? typedErr?.status

  if (status === 500) {
    return 'The server encountered an unexpected error'
  }

  if (status === 429) {
    return "Slow down! You're sending requests faster than we can handle them"
  }

  if (data?.errors && data.errors.length > 0) {
    return data.errors[0]?.reason || 'Unknown error'
  }

  if (data?.detail) {
    return data.detail
  }

  return 'Unknown error'
}
