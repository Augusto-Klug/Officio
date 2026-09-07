import { http } from './http'
import { field, record, requiredString } from './contracts'
import type { Session } from '../stores/session'

export async function login(email: string, senha: string): Promise<Session> {
  const { data } = await http.post<unknown>('/v1/autenticacao/Login', { email, senha }, { skipAuth: true })
  const body = record(data)
  const usuario = record(field(body, 'usuario'))
  const expiraEm = requiredString(field(body, 'expiraEm'))
  if (!(Date.parse(expiraEm) > Date.now())) throw new Error('O servidor retornou uma sessão expirada ou inválida.')
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
}
