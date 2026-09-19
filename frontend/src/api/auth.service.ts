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

export async function solicitarRedefinicaoSenha(email: string): Promise<void> {
  email = email.trim()
  if (!email) throw new ApiError('Informe seu e-mail.')
  try { await http.post('/v1/autenticacao/EsqueciSenha', { email }, { skipAuth: true }) }
  catch (cause) {
    const error = normalizeError(cause)
    // A recuperação não confirma se uma conta existe para o e-mail informado.
    if (error.status && error.status >= 400 && error.status < 500) return
    throw error
  }
}

function validarTokenESenha(token: string, senha: string): { token: string; senha: string } {
  token = token.trim()
  if (!token) throw new ApiError('O link está incompleto ou inválido.')
  if (senha.length < 8) throw new ApiError('A senha deve ter pelo menos 8 caracteres.')
  return { token, senha }
}

export async function ativarConta(token: string, senha: string): Promise<void> {
  try { await http.post('/v1/autenticacao/AceitarConvite', validarTokenESenha(token, senha), { skipAuth: true }) }
  catch (cause) { throw normalizeError(cause) }
}

export async function redefinirSenha(token: string, senha: string): Promise<void> {
  try { await http.post('/v1/autenticacao/RedefinirSenha', validarTokenESenha(token, senha), { skipAuth: true }) }
  catch (cause) { throw normalizeError(cause) }
}
