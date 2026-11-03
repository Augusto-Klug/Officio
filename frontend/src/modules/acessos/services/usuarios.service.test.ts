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

it('normaliza usuarios Cliente com multiplas empresas e preserva perfis ainda nao mapeados', async () => {
  http.defaults.adapter = async (config) => ({
    data: config.url?.endsWith('/convites') ? [] : [
      {
        Id: 'u-client', Nome: 'Conta cliente', Email: 'cliente@empresa.test', Perfil: 'Cliente', Ativo: true,
        Empresas: [
          { Id: 'e1', RazaoSocial: 'Empresa Um Ltda', Cnpj: '11111111000111', EmpresaAtiva: true, VinculoAtivo: true },
          { Id: 'e2', RazaoSocial: 'Empresa Dois Ltda', Cnpj: '22222222000122', EmpresaAtiva: false, VinculoAtivo: false },
        ],
      },
      { Id: 'u-future', Nome: 'Perfil futuro', Email: 'futuro@empresa.test', Perfil: 'Auditor', Ativo: true },
    ],
    status: 200, statusText: 'OK', headers: new AxiosHeaders(), config,
  })

  await expect(listarUsuarios()).resolves.toEqual([
    expect.objectContaining({
      id: 'u-client', perfil: 'Cliente',
      empresas: [
        expect.objectContaining({ id: 'e1', razaoSocial: 'Empresa Um Ltda', vinculoAtivo: true }),
        expect.objectContaining({ id: 'e2', razaoSocial: 'Empresa Dois Ltda', empresaAtiva: false, vinculoAtivo: false }),
      ],
    }),
    expect.objectContaining({ id: 'u-future', perfil: 'Auditor', empresas: [] }),
  ])
})

it('normaliza convite de Cliente com sua empresa relacionada', async () => {
  http.defaults.adapter = async (config) => ({
    data: config.url?.endsWith('/convites')
      ? [{ Id: 'c-client', Nome: 'Convite cliente', Email: 'cliente@empresa.test', Perfil: 'Cliente', Status: 'Pendente', EmpresaId: 'e1', EmpresaNome: 'Empresa Um Ltda' }]
      : [],
    status: 200, statusText: 'OK', headers: new AxiosHeaders(), config,
  })

  await expect(listarUsuarios()).resolves.toEqual([
    expect.objectContaining({
      id: 'c-client', perfil: 'Cliente', situacao: 'CONVITE_PENDENTE',
      empresas: [expect.objectContaining({ id: 'e1', razaoSocial: 'Empresa Um Ltda' })],
    }),
  ])
})

it('usa os contratos REST de gestao de acessos', async () => {
  const calls: Array<{ method?: string; url?: string; data?: unknown }> = []
  http.defaults.adapter = async (config) => {
    calls.push({ method: config.method, url: config.url, data: config.data ? JSON.parse(config.data) : undefined })
    return { data: {}, status: 204, statusText: 'No Content', headers: new AxiosHeaders(), config }
  }
  await convidarUsuario({ nome: ' Ana ', email: ' ana@empresa.test ', perfil: 'Operacional' })
  await alterarPerfil('u1', 'Consulta')
  await alterarStatus('u1', false)
  await reenviarConvite('u1')
  await cancelarConvite('u1')
  expect(calls).toEqual([
    { method: 'post', url: '/v1/usuarios/convites', data: { nome: 'Ana', email: 'ana@empresa.test', perfil: 'Operacional' } },
    { method: 'patch', url: '/v1/usuarios/u1/perfil', data: { perfil: 'Consulta' } },
    { method: 'patch', url: '/v1/usuarios/u1/status', data: { ativo: false } },
    { method: 'post', url: '/v1/usuarios/convites/u1/reenviar', data: undefined },
    { method: 'post', url: '/v1/usuarios/convites/u1/cancelar', data: undefined },
  ])
})
