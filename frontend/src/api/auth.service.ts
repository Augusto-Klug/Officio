import { http } from './http'
import { field, record, requiredString } from './contracts'
import type { Session } from '../stores/session'
import { ApiError, normalizeError } from './errors'

export async function login(email: string, senha: string): Promise<Session> {
  email = email.trim()
  if (!email || !senha.trim()) throw new ApiError('Informe seu e-mail e sua senha.')
  try {
    const { data } = await http.post<unknown>('/v1/autenticacao/Login', { email, senha }, { skipAuth: true })
    const body = record(data)
    const usuario = record(field(body, 'usuario'))
    const expiraEm = requiredString(field(body, 'expiraEm'))
    if (!(Date.parse(expiraEm) > Date.now())) throw new Error('Sessão inválida.')
    return {
      token: requiredString(field(body, 'token')),
      expiraEm,
      usuario: {
        id: requiredString(field(usuario, 'id')),
        nome: requiredString(field(usuario, 'nome')),
        email: requiredString(field(usuario, 'email')),
        perfil: requiredString(field(usuario, 'perfil')),
      },
    }
  } catch (cause) {
    const error = normalizeError(cause)
    if (error.status === 401) throw new ApiError('E-mail ou senha incorretos.', 401)
    if (cause instanceof ApiError && (!error.status || error.status >= 500)) throw error
    throw new ApiError('Não foi possível entrar. Tente novamente em instantes.', error.status)
  }
}
