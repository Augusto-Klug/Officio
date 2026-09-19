import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { listarClientes, normalizeClienteDetalhe, normalizeClientes, vincularCodigoSci } from './clientes.service'

afterEach(() => vi.restoreAllMocks())

describe('contrato HTTP de clientes', () => {
  it('normaliza listagem e aceita propriedades PascalCase', () => {
    expect(normalizeClientes([{ Id: 'c1', RazaoSocial: 'Empresa Ltda.', Ativo: true, QuantidadeSocios: 2, QuantidadeContratos: 1, QuantidadeDocumentos: 3 }])).toEqual([
      expect.objectContaining({ id: 'c1', razaoSocial: 'Empresa Ltda.', ativo: true, quantidadeSocios: 2, quantidadeContratos: 1, quantidadeDocumentos: 3 }),
    ])
  })

  it('normaliza detalhe e coleções relacionadas', () => {
    const result = normalizeClienteDetalhe({
      id: 'c1', razaoSocial: 'Empresa', ativo: true,
      socios: [{ id: 's1', nome: 'Ana', participacao: 100 }],
      contratos: [{ id: 'ct1', numero: 'CTR-1', status: 'Ativo', valorTotal: 500, dataInicio: '2026-09-01' }],
      documentos: [{ id: 'd1', nomeArquivo: 'contrato.pdf', tipo: 'Contrato', legivel: true, qualidadeEstimada: 0.98 }],
    })
    expect(result.socios[0]?.nome).toBe('Ana')
    expect(result.contratos[0]?.numero).toBe('CTR-1')
    expect(result.documentos[0]?.legivel).toBe(true)
  })

  it('usa filtros e rota pública do controller', async () => {
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: [] })
    await listarClientes({ busca: 'Empresa', ativo: true })
    expect(get).toHaveBeenCalledWith('/v1/clientes/Listar', { params: { busca: 'Empresa', ativo: true }, signal: undefined })
  })

  it('vincula código SCI no endpoint correto', async () => {
    const patch = vi.spyOn(http, 'patch').mockResolvedValue({ data: undefined })
    await vincularCodigoSci('c1', 'SCI-42')
    expect(patch).toHaveBeenCalledWith('/v1/clientes/VincularCodigoSci/c1', { sciCodigo: 'SCI-42' })
  })
})
