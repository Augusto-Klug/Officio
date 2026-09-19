import axios from 'axios'

export class ApiError extends Error {
  constructor(message: string, public status?: number, public fields: Record<string, string[]> = {}) {
    super(message)
    this.name = 'ApiError'
  }
}

export function normalizeError(error: unknown): ApiError {
  if (error instanceof ApiError) return error
  if (!axios.isAxiosError(error)) return new ApiError(error instanceof Error ? error.message : 'Não foi possível concluir a solicitação.')
  const status = error.response?.status
  const body: unknown = error.response?.data
  if (!status) return new ApiError('Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.')
  if (status >= 500) return new ApiError('O servidor não conseguiu concluir a solicitação. Tente novamente em instantes.', status)
  const fallback = status === 401 ? 'Sua sessão expirou. Entre novamente.' : 'Não foi possível concluir a solicitação.'
  if (typeof body === 'string') return new ApiError(body || fallback, status)
  if (body && typeof body === 'object') {
    const data = body as Record<string, unknown>
    const fields: Record<string, string[]> = {}
    if (data.errors && typeof data.errors === 'object') {
      for (const [key, value] of Object.entries(data.errors)) {
        if (Array.isArray(value)) fields[key] = value.filter((item): item is string => typeof item === 'string')
      }
    }
    const message = [data.error, data.erro, data.message, data.detail, ...Object.values(fields).flat(), data.title].find((value) => typeof value === 'string' && value.trim())
    return new ApiError(typeof message === 'string' ? message : fallback, status, fields)
  }
  return new ApiError(fallback, status)
}
