import axios from 'axios'
import { accessToken, clearSession } from '../stores/session'
import { normalizeError } from './errors'

declare module 'axios' {
  export interface AxiosRequestConfig { skipAuth?: boolean }
}

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 20_000,
  headers: { Accept: 'application/json' },
})

let unauthorizedHandler: (() => void) | undefined
export function onUnauthorized(handler: () => void) { unauthorizedHandler = handler }

http.interceptors.request.use((config) => {
  const token = config.skipAuth ? null : accessToken()
  if (token) config.headers.set('Authorization', `Bearer ${token}`)
  // O navegador define o boundary multipart; nunca fixar Content-Type globalmente.
  if (config.data instanceof FormData) config.headers.delete('Content-Type')
  return config
})

http.interceptors.response.use((response) => response, (error: unknown) => {
  if (axios.isCancel(error)) return Promise.reject(error)
  if (axios.isAxiosError(error) && error.response?.status === 401 && !error.config?.skipAuth) {
    clearSession()
    unauthorizedHandler?.()
  }
  return Promise.reject(normalizeError(error))
})
