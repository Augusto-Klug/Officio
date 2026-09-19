import { afterEach, describe, expect, it, vi } from 'vitest'
import { AxiosError, AxiosHeaders, type AxiosResponse } from 'axios'
import { http, onUnauthorized } from './http'
import { normalizeError } from './errors'
import { clearSession, session, setSession } from '../stores/session'

afterEach(() => { clearSession(); onUnauthorized(() => {}); delete http.defaults.adapter })
function authenticate() {
  setSession({ token: 'test-token', expiraEm: new Date(Date.now() + 60_000).toISOString(), usuario: { id: '1', nome: 'Ana', email: 'test@example.com', perfil: 'Admin' } })
}

describe('fronteira HTTP', () => {
  it('envia Bearer e permite que o navegador defina o boundary do FormData', async () => {
    authenticate()
    const data = new FormData()
    data.append('arquivo', new Blob(['teste']), 'teste.txt')
    http.defaults.adapter = async (config) => {
      expect(config.headers.get('Authorization')).toBe('Bearer test-token')
      expect(config.data).toBe(data)
      expect(String(config.headers.get('Content-Type'))).not.toContain('multipart/form-data')
      return { data: {}, status: 200, statusText: 'OK', headers: new AxiosHeaders(), config }
    }
    await http.post('/test', data)
  })
  it('limpa a sessão e notifica o router em um 401 protegido', async () => {
    authenticate()
    const redirect = vi.fn()
    onUnauthorized(redirect)
    http.defaults.adapter = async (config) => {
      throw new AxiosError('Unauthorized', 'ERR_BAD_REQUEST', config, undefined, { data: {}, status: 401, config, statusText: 'Unauthorized', headers: new AxiosHeaders() })
    }
    await expect(http.get('/test')).rejects.toMatchObject({ status: 401 })
    expect(session.value).toBeNull()
    expect(redirect).toHaveBeenCalledOnce()
  })
  it('não redireciona quando o login recebe credenciais inválidas', async () => {
    const redirect = vi.fn()
    onUnauthorized(redirect)
    http.defaults.adapter = async (config) => {
      expect(config.headers.has('Authorization')).toBe(false)
      throw new AxiosError('Unauthorized', 'ERR_BAD_REQUEST', config, undefined, { data: { error: 'E-mail ou senha incorretos.' }, status: 401, config, statusText: 'Unauthorized', headers: new AxiosHeaders() })
    }
    await expect(http.post('/test', {}, { skipAuth: true })).rejects.toThrow('E-mail ou senha incorretos.')
    expect(redirect).not.toHaveBeenCalled()
  })
  it('normaliza ValidationProblemDetails e não expõe detalhes internos de erros 500', () => {
    const response = { status: 400, data: { errors: { Email: ['E-mail inválido.'] } } } as AxiosResponse
    expect(normalizeError(new AxiosError('error', undefined, undefined, undefined, response))).toMatchObject({ message: 'E-mail inválido.', fields: { Email: ['E-mail inválido.'] } })
    response.status = 500
    response.data = { detail: 'Detalhe interno do servidor' }
    expect(normalizeError(new AxiosError('error', undefined, undefined, undefined, response)).message).not.toContain('Detalhe interno')
  })
})
