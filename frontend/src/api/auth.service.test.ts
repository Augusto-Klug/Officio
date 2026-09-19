import { afterEach, expect, it, vi } from 'vitest'
import { AxiosError, AxiosHeaders } from 'axios'
import { ativarConta, login, redefinirSenha, solicitarRedefinicaoSenha } from './auth.service'
import { http, onUnauthorized } from './http'
import { accessToken, clearSession, session, setSession } from '../stores/session'

afterEach(() => { clearSession(); onUnauthorized(() => {}); delete http.defaults.adapter })
const response = () => ({ token: 'jwt-teste', expiraEm: new Date(Date.now() + 60_000).toISOString(),
  usuario: { id: '1', nome: 'Admin', email: 'admin@example.test', perfil: 'Admin' } })

it('envia JSON ao login real sem Bearer, preserva senha e permite autenticar chamadas seguintes', async () => {
  const data = response()
  http.defaults.adapter = async (config) => {
    expect(config.url).toBe('/v1/autenticacao/Login')
    expect(JSON.parse(config.data)).toEqual({ email: 'admin@example.test', senha: ' senha-teste ' })
    expect(config.headers.get('Content-Type')).toBe('application/json')
    expect(config.headers.has('Authorization')).toBe(false)
    return { data, status: 200, statusText: 'OK', headers: new AxiosHeaders(), config }
  }
  setSession(await login('  admin@example.test  ', ' senha-teste '))
  expect(session.value).toEqual(data)
  expect(accessToken()).toBe(data.token)
  clearSession()
  expect(accessToken()).toBeNull()
  expect(session.value).toBeNull()
})

it.each([['', 'senha'], ['email@test.local', ''], ['   ', 'senha']])('rejeita campos obrigatorios vazios', async (email, senha) => {
  const adapter = vi.fn()
  http.defaults.adapter = adapter
  await expect(login(email, senha)).rejects.toThrow('Informe seu e-mail e sua senha.')
  expect(adapter).not.toHaveBeenCalled()
})

it.each([401, 400, 500])('nao expoe detalhes do backend no login com status %i', async (status) => {
  const redirect = vi.fn()
  onUnauthorized(redirect)
  http.defaults.adapter = async (config) => {
    throw new AxiosError('interno', undefined, config, undefined,
      { data: { error: 'usuario inexistente: detalhe interno' }, status, statusText: 'Error', headers: new AxiosHeaders(), config })
  }
  const result = login('email@test.local', 'senha')
  await expect(result).rejects.not.toThrow('detalhe interno')
  if (status === 401) await expect(result).rejects.toThrow('E-mail ou senha incorretos.')
  expect(redirect).not.toHaveBeenCalled()
  expect(session.value).toBeNull()
})

it('explica falha de conexao sem detalhes tecnicos', async () => {
  http.defaults.adapter = async () => { throw new AxiosError('ECONNREFUSED', 'ERR_NETWORK') }
  await expect(login('email@test.local', 'senha')).rejects.toThrow('Não foi possível conectar ao servidor.')
})

it.each([{}, { ...response(), expiraEm: 'invalido' }])('rejeita resposta inesperada sem expor contrato interno', async (data) => {
  http.defaults.adapter = async (config) => ({ data, status: 200, statusText: 'OK', headers: new AxiosHeaders(), config })
  await expect(login('email@test.local', 'senha')).rejects.toThrow('Não foi possível entrar. Tente novamente em instantes.')
})

it('envia os fluxos publicos de ativacao e recuperacao sem Bearer', async () => {
  const calls: Array<{ url?: string; data: unknown; authorization: boolean }> = []
  http.defaults.adapter = async (config) => {
    calls.push({ url: config.url, data: JSON.parse(config.data), authorization: config.headers.has('Authorization') })
    return { data: {}, status: 204, statusText: 'No Content', headers: new AxiosHeaders(), config }
  }
  await solicitarRedefinicaoSenha(' pessoa@empresa.test ')
  await ativarConta(' token-ativacao ', 'senha-segura')
  await redefinirSenha(' token-reset ', 'nova-senha')
  expect(calls).toEqual([
    { url: '/v1/autenticacao/EsqueciSenha', data: { email: 'pessoa@empresa.test' }, authorization: false },
    { url: '/v1/autenticacao/AceitarConvite', data: { token: 'token-ativacao', senha: 'senha-segura' }, authorization: false },
    { url: '/v1/autenticacao/RedefinirSenha', data: { token: 'token-reset', senha: 'nova-senha' }, authorization: false },
  ])
})

it.each([
  [() => ativarConta('', 'senha-segura'), 'link está incompleto'],
  [() => redefinirSenha('token', 'curta'), 'pelo menos 8 caracteres'],
  [() => solicitarRedefinicaoSenha('   '), 'Informe seu e-mail'],
])('valida dados dos fluxos publicos antes da API', async (action, message) => {
  const adapter = vi.fn()
  http.defaults.adapter = adapter
  await expect(action()).rejects.toThrow(message)
  expect(adapter).not.toHaveBeenCalled()
})
