import { afterEach, expect, it } from 'vitest'
import { AxiosHeaders } from 'axios'
import { alterarStatusEmpresa, criarEmpresa, listarEmpresas, reenviarConviteAdministrador } from './empresas.service'
import { http } from '../../../api/http'

afterEach(() => { delete http.defaults.adapter })

it('normaliza empresas e aceita resposta envelopada', async () => {
  http.defaults.adapter = async (config) => ({
    data: { empresas: [{ Id: 'e1', RazaoSocial: 'Contábil Alfa', Cnpj: '12345678000190', Ativo: true, QuantidadeUsuarios: 3 }] },
    status: 200, statusText: 'OK', headers: new AxiosHeaders(), config,
  })
  await expect(listarEmpresas()).resolves.toEqual([expect.objectContaining({ id: 'e1', ativa: true, quantidadeUsuarios: 3 })])
})

it('limpa documentos e usa endpoints administrativos', async () => {
  const calls: Array<{ method?: string; url?: string; data?: unknown }> = []
  http.defaults.adapter = async (config) => {
    calls.push({ method: config.method, url: config.url, data: config.data ? JSON.parse(config.data) : undefined })
    return { data: {}, status: 204, statusText: 'No Content', headers: new AxiosHeaders(), config }
  }
  await criarEmpresa({ razaoSocial: ' Alfa ', nomeFantasia: null, cnpj: '12.345.678/0001-90', administrador: { nome: ' Ana ', email: ' ana@test.local ' } })
  await alterarStatusEmpresa('e1', false)
  await reenviarConviteAdministrador('e1')
  expect(calls[0]).toEqual({
    method: 'post',
    url: '/v1/admin/empresas',
    data: {
      razaoSocial: 'Alfa',
      nomeFantasia: null,
      cnpj: '12345678000190',
      emailEmpresa: null,
      nomeAdministrador: 'Ana',
      emailAdministrador: 'ana@test.local',
    },
  })
  expect(calls.slice(1).map(({ method, url }) => ({ method, url }))).toEqual([
    { method: 'patch', url: '/v1/admin/empresas/e1/status' },
    { method: 'post', url: '/v1/admin/empresas/e1/administrador/reenviar-convite' },
  ])
})
