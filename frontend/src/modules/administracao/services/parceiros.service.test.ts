import { afterEach, expect, it } from 'vitest'
import { AxiosHeaders } from 'axios'
import { alterarStatusParceiro, criarParceiro, listarParceiros, reenviarConviteAdministrador } from './parceiros.service'
import { http } from '../../../api/http'

afterEach(() => { delete http.defaults.adapter })

it('normaliza parceiros e aceita resposta envelopada', async () => {
  http.defaults.adapter = async (config) => ({
    data: { parceiros: [{ Id: 'p1', RazaoSocial: 'Contabil Alfa', Cnpj: '12345678000190', Ativo: true, QuantidadeUsuarios: 3 }] },
    status: 200, statusText: 'OK', headers: new AxiosHeaders(), config,
  })
  await expect(listarParceiros()).resolves.toEqual([expect.objectContaining({ id: 'p1', ativo: true, quantidadeUsuarios: 3 })])
})

it('limpa documentos e usa endpoints administrativos', async () => {
  const calls: Array<{ method?: string; url?: string; data?: unknown }> = []
  http.defaults.adapter = async (config) => {
    calls.push({ method: config.method, url: config.url, data: config.data ? JSON.parse(config.data) : undefined })
    return { data: {}, status: 204, statusText: 'No Content', headers: new AxiosHeaders(), config }
  }
  await criarParceiro({ razaoSocial: ' Alfa ', nomeFantasia: null, cnpj: '12.345.678/0001-90', administrador: { nome: ' Ana ', email: ' ana@test.local ' } })
  await alterarStatusParceiro('p1', false)
  await reenviarConviteAdministrador('p1')
  expect(calls[0]).toEqual({
    method: 'post',
    url: '/v1/admin/parceiros',
    data: {
      razaoSocial: 'Alfa',
      nomeFantasia: null,
      cnpj: '12345678000190',
      emailParceiro: null,
      nomeAdministrador: 'Ana',
      emailAdministrador: 'ana@test.local',
    },
  })
  expect(calls.slice(1).map(({ method, url }) => ({ method, url }))).toEqual([
    { method: 'patch', url: '/v1/admin/parceiros/p1/status' },
    { method: 'post', url: '/v1/admin/parceiros/p1/administrador/reenviar-convite' },
  ])
})
