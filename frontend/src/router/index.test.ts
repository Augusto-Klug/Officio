// @vitest-environment jsdom
import { afterEach, expect, it } from 'vitest'
import { router, safeRedirect } from './index'
import { clearSession, setSession } from '../stores/session'

afterEach(() => clearSession())
it('protege a nova rota e permite retorno seguro após login', async () => {
  clearSession()
  await router.push('/prospectos/novo')
  expect(router.currentRoute.value.name).toBe('login')
  expect(router.currentRoute.value.query.redirect).toBe('/prospectos/novo')
  setSession({ token: 'token', expiraEm: new Date(Date.now() + 60_000).toISOString(), usuario: { id: '1', nome: 'Maria', email: 'maria@test.local', perfil: 'Admin' } })
  await router.replace(safeRedirect(router.currentRoute.value.query.redirect))
  expect(router.currentRoute.value.name).toBe('novo-prospecto')
})
it.each(['/prospectos', '/prospectos?pagina=2', '/prospectos/novo'])('aceita retorno interno %s', value => expect(safeRedirect(value)).toBe(value))
it.each(['https://example.com', '//example.com', '/prospectos/novo/extra', '/prospectos-malicioso', '/clientes'])('rejeita retorno não autorizado %s', value => expect(safeRedirect(value)).toBe('/prospectos'))
