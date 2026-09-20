import { afterEach, expect, it, vi } from 'vitest'

afterEach(() => { vi.unstubAllGlobals(); vi.resetModules() })

it('restaura a sessao apos recarga e limpa a persistencia no logout local', async () => {
  const storage = new Map<string, string>()
  vi.stubGlobal('sessionStorage', {
    getItem: (key: string) => storage.get(key) ?? null,
    setItem: (key: string, value: string) => storage.set(key, value),
    removeItem: (key: string) => storage.delete(key),
  })
  const initial = await import('./session')
  initial.setSession({ token: 'teste', expiraEm: new Date(Date.now() + 60_000).toISOString(),
    usuario: { id: '1', nome: 'Admin', email: 'admin@example.test', perfil: 'Admin' } })
  vi.resetModules()
  const restored = await import('./session')
  expect(restored.authenticated.value).toBe(true)
  expect(restored.accessToken()).toBe('teste')
  expect(restored.defaultAuthenticatedRoute()).toBe('/dashboard')
  restored.clearSession()
  expect(restored.authenticated.value).toBe(false)
  expect(storage.size).toBe(0)
})

it('descarta token expirado antes de enviar uma chamada protegida', async () => {
  const current = await import('./session')
  current.setSession({ token: 'expirado', expiraEm: new Date(Date.now() - 1000).toISOString(),
    usuario: { id: '1', nome: 'Admin', email: 'admin@example.test', perfil: 'Admin' } })
  expect(current.accessToken()).toBeNull()
  expect(current.authenticated.value).toBe(false)
})

it('distingue PlatformAdmin de Admin sem promover perfis por aproximacao', async () => {
  const current = await import('./session')
  current.setSession({ token: 'platform', expiraEm: new Date(Date.now() + 60_000).toISOString(),
    usuario: { id: '1', nome: 'Plataforma', email: 'platform@example.test', perfil: 'PlatformAdmin' } })
  expect(current.hasRole('PlatformAdmin')).toBe(true)
  expect(current.hasRole('Admin')).toBe(false)
  expect(current.defaultAuthenticatedRoute()).toBe('/admin/empresas')
})
