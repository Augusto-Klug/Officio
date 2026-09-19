import { afterEach, describe, expect, it, vi } from 'vitest'
import { AxiosHeaders } from 'axios'
import { http } from '../../../api/http'
import {
  listarNotificacoes,
  marcarNotificacaoComoLida,
  marcarTodasNotificacoesComoLidas,
  normalizarNotificacoes,
} from './notificacoes.service'

afterEach(() => { delete http.defaults.adapter })

const notificacao = {
  id: 'ef68210b-9223-4c29-86a3-3886d808ae4e',
  tipo: 'CONTRATO_ASSINADO',
  titulo: 'Contrato assinado',
  mensagem: 'O contrato CTR-2026-001 foi assinado.',
  referenciaId: '69b80268-74c9-451f-ae90-e137084f5c38',
  lida: false,
  lidaEm: null,
  criadoEm: '2026-09-19T14:30:00Z',
}

describe('servico de notificacoes', () => {
  it('lista todas as notificacoes pela rota real do backend', async () => {
    http.defaults.adapter = async (config) => {
      expect(config.method).toBe('get')
      expect(config.url).toBe('/v1/notificacoes/Listar')
      expect(config.params).toBeUndefined()
      return { data: [notificacao], status: 200, statusText: 'OK', headers: new AxiosHeaders(), config }
    }

    await expect(listarNotificacoes()).resolves.toEqual([notificacao])
  })

  it('envia o filtro de nao lidas como lida=false', async () => {
    http.defaults.adapter = async (config) => {
      expect(config.params).toEqual({ lida: false })
      return { data: [], status: 200, statusText: 'OK', headers: new AxiosHeaders(), config }
    }

    await expect(listarNotificacoes(false)).resolves.toEqual([])
  })

  it('marca uma notificacao como lida usando PATCH e o identificador na rota', async () => {
    const adapter = vi.fn(async (config) => {
      expect(config.method).toBe('patch')
      expect(config.url).toBe(`/v1/notificacoes/MarcarComoLida/${notificacao.id}`)
      return { data: undefined, status: 204, statusText: 'No Content', headers: new AxiosHeaders(), config }
    })
    http.defaults.adapter = adapter

    await expect(marcarNotificacaoComoLida(notificacao.id)).resolves.toBeUndefined()
    expect(adapter).toHaveBeenCalledOnce()
  })

  it('marca todas como lidas e normaliza a quantidade alterada', async () => {
    http.defaults.adapter = async (config) => {
      expect(config.method).toBe('patch')
      expect(config.url).toBe('/v1/notificacoes/MarcarTodasComoLidas')
      return { data: { Quantidade: 3 }, status: 200, statusText: 'OK', headers: new AxiosHeaders(), config }
    }

    await expect(marcarTodasNotificacoesComoLidas()).resolves.toEqual({ quantidade: 3 })
  })

  it('aceita propriedades PascalCase sem vazar essa variacao para o componente', () => {
    expect(normalizarNotificacoes([{ ...notificacao, id: undefined, Id: notificacao.id }])[0]?.id).toBe(notificacao.id)
  })

  it.each([
    null,
    {},
    [{ ...notificacao, lida: 'false' }],
    [{ ...notificacao, criadoEm: 'invalida' }],
  ])('rejeita uma resposta invalida: %j', (resposta) => {
    expect(() => normalizarNotificacoes(resposta)).toThrow()
  })
})
