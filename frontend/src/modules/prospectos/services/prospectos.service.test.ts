import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { prospectoNaEtapaInicial, RegimeTributario, type CriarProspectoPayload } from '../types/prospecto'
import { atualizarProspecto, criarProspecto, excluirProspecto, normalizeProspectoCriado, normalizeProspectos, obterProspecto } from './prospectos.service'

afterEach(() => vi.restoreAllMocks())

describe('manutencao de prospectos', () => {
  it.each(['Convertido', 'PropostaEnviada', 'Perdido', 'Inativo', null])('oculta a manutencao no status %s', (status) => {
    expect(prospectoNaEtapaInicial({ status })).toBe(false)
  })

  it('habilita a manutencao apenas na etapa inicial', () => {
    expect(prospectoNaEtapaInicial({ status: 'EmNegociacao' })).toBe(true)
  })

  it('consulta o cadastro completo e usa os endpoints de atualizacao e exclusao', async () => {
    const payload: CriarProspectoPayload = { razaoSocial: 'Revisada', cnpj: null, regimeTributario: null, socios: [] }
    const detail = { id: 'p1', status: 'EmNegociacao', dados: payload }
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: detail })
    const put = vi.spyOn(http, 'put').mockResolvedValue({ status: 204 })
    const remove = vi.spyOn(http, 'delete').mockResolvedValue({ status: 204 })
    const controller = new AbortController()

    await expect(obterProspecto('p1', controller.signal)).resolves.toEqual(detail)
    await atualizarProspecto('p1', payload)
    await excluirProspecto('p1')
    expect(get).toHaveBeenCalledWith('/v1/prospectos/ObterPorId/p1', { signal: controller.signal })
    expect(put).toHaveBeenCalledWith('/v1/prospectos/Atualizar/p1', payload)
    expect(remove).toHaveBeenCalledWith('/v1/prospectos/Excluir/p1')
  })
})

describe('normalização do contrato de prospectos', () => {
  it('interpreta o PagedResult efetivamente retornado pelo endpoint', () => {
    expect(normalizeProspectos({ data: [{ id: '1', razaoSocial: 'Escritório Exemplo', cnpj: null, status: 'EmNegociacao', responsavel: 'Ana', criadoEm: null }], totalCount: 21, page: 2, pageSize: 20, totalPages: 2 })).toEqual({
      dados: [{ id: '1', razaoSocial: 'Escritório Exemplo', cnpj: null, status: 'EmNegociacao', responsavel: 'Ana', criadoEm: null }], totalRegistros: 21, pagina: 2, tamanhoPagina: 20, totalPaginas: 2,
    })
  })
  it('aceita o contrato português e PascalCase sem vazar a diferença para a tela', () => {
    const value = normalizeProspectos({ Dados: [{ Id: '2', RazaoSocial: 'Empresa', ResponsavelNome: 'Bia', Status: 'NovoStatus' }], TotalRegistros: 1, Pagina: 1, TamanhoPagina: 20, TotalPaginas: 1 })
    expect(value.dados[0]).toMatchObject({ responsavel: 'Bia', status: 'NovoStatus', cnpj: null })
  })
  it('diferencia lista vazia de resposta inválida', () => {
    expect(normalizeProspectos({ data: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 }).dados).toEqual([])
    expect(() => normalizeProspectos({ error: 'Falha' })).toThrow()
    expect(() => normalizeProspectos({ data: [], totalCount: 0, page: 0, pageSize: 20, totalPages: 0 })).toThrow()
  })
})

describe('criação de prospecto', () => {
  it('normaliza o retorno efetivo do endpoint', () => {
    expect(normalizeProspectoCriado({ Id: 'prospecto-1', RazaoSocial: 'Empresa Exemplo' })).toEqual({
      id: 'prospecto-1',
      razaoSocial: 'Empresa Exemplo',
    })
  })

  it('envia somente os dados efetivamente persistidos ao endpoint real', async () => {
    const payload: CriarProspectoPayload = {
      razaoSocial: 'Empresa Exemplo Ltda.', cnpj: '12345678000190',
      regimeTributario: RegimeTributario.SimplesNacional,
      socios: [{ nome: 'Maria', cpf: '123.456.789-09', participacao: 100 }],
    }
    const post = vi.spyOn(http, 'post').mockResolvedValue({ data: { id: 'prospecto-1', razaoSocial: payload.razaoSocial } })

    await expect(criarProspecto(payload)).resolves.toEqual({ id: 'prospecto-1', razaoSocial: payload.razaoSocial })
    expect(post).toHaveBeenCalledWith('/v1/prospectos/Criar', payload)
  })

  it('rejeita uma resposta de criação sem identificador', () => {
    expect(() => normalizeProspectoCriado({ razaoSocial: 'Empresa' })).toThrow()
  })
})
