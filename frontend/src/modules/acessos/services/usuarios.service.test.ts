import { afterEach, expect, it } from 'vitest'
import { AxiosHeaders } from 'axios'
import { alterarPerfil, alterarStatus, cancelarConvite, convidarUsuario, listarUsuarios, reenviarConvite } from './usuarios.service'
import { http } from '../../../api/http'

afterEach(() => { delete http.defaults.adapter })

it('combina usuarios e convites pendentes em uma unica listagem', async () => {
  http.defaults.adapter = async (config) => ({
    data: config.url?.endsWith('/convites')
      ? [{ Id: 'c1', Nome: 'Bia', Email: 'bia@empresa.test', Perfil: 'Comercial', Status: 'Pendente', CriadoEm: '2026-09-19T10:00:00Z' }]
      : [{ Id: 'u1', Nome: 'Ana', Email: 'ana@empresa.test', Perfil: 'admin', Ativo: true, CriadoEm: '2026-09-18T10:00:00Z' }],
    status: 200, statusText: 'OK', headers: new AxiosHeaders(), config,
  })
  await expect(listarUsuarios()).resolves.toEqual([
    expect.objectContaining({ id: 'u1', perfil: 'Admin', situacao: 'ATIVO' }),
    expect.objectContaining({ id: 'c1', perfil: 'Comercial', situacao: 'CONVITE_PENDENTE' }),
  ])
})

it('usa os contratos REST de gestao de acessos', async () => {
  const calls: Array<{ method?: string; url?: string; data?: unknown }> = []
  http.defaults.adapter = async (config) => {
    calls.push({ method: config.method, url: config.url, data: config.data ? JSON.parse(config.data) : undefined })
    return { data: {}, status: 204, statusText: 'No Content', headers: new AxiosHeaders(), config }
  }
  await convidarUsuario({ nome: ' Ana ', email: ' ana@empresa.test ', perfil: 'Comercial' })
  await alterarPerfil('u1', 'Financeiro')
  await alterarStatus('u1', false)
  await reenviarConvite('u1')
  await cancelarConvite('u1')
  expect(calls).toEqual([
    { method: 'post', url: '/v1/usuarios/convites', data: { nome: 'Ana', email: 'ana@empresa.test', perfil: 'Comercial' } },
    { method: 'patch', url: '/v1/usuarios/u1/perfil', data: { perfil: 'Financeiro' } },
    { method: 'patch', url: '/v1/usuarios/u1/status', data: { ativo: false } },
    { method: 'post', url: '/v1/usuarios/convites/u1/reenviar', data: undefined },
    { method: 'post', url: '/v1/usuarios/convites/u1/cancelar', data: undefined },
  ])
})
