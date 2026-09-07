import { describe, expect, it } from 'vitest'
import { normalizeProspectos } from './prospectos.service'

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
