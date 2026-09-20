import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { cancelarFaturamento, fecharCompetencia, fecharFaturamento, gerarCompetencia, listarFaturamentos, normalizarFaturamentos, obterResumoFaturamento, recalcularCompetencia } from './faturamentos.service'

afterEach(() => vi.restoreAllMocks())

const faturamento = {
  id: 'fat-1', contratoId: 'ctr-1', numeroContrato: 'CTR-2026-0001', cliente: 'Empresa Teste',
  competencia: '2026-09-01', status: 'ABERTO', quantidadeFuncionarios: 4, quantidadeProLabores: 1,
  quantidadeFolhas: 5, valorHonorarioBase: 800, valorPorFolha: 20, valorTotal: 900,
  calculadoEm: '2026-09-20T10:00:00Z',
}

describe('contrato HTTP de faturamentos', () => {
  it('normaliza snapshots mensais', () => {
    expect(normalizarFaturamentos([faturamento])).toEqual([faturamento])
    expect(() => normalizarFaturamentos([{ ...faturamento, status: 'PAGO' }])).toThrow('status')
  })

  it('lista, resume, gera, recalcula, fecha e cancela pelos endpoints esperados', async () => {
    const resumo = {
      competencia: '2026-09-01', totalRegistros: 1, abertos: 1, fechados: 0, cancelados: 0,
      quantidadeFolhas: 5, valorAberto: 900, valorFechado: 0, valorTotalAtivo: 900,
    }
    const get = vi.spyOn(http, 'get')
      .mockResolvedValueOnce({ data: [faturamento] })
      .mockResolvedValueOnce({ data: resumo })
    const post = vi.spyOn(http, 'post')
      .mockResolvedValueOnce({ data: { competencia: '2026-09-01', gerados: 1, ignorados: 0, valorTotal: 900 } })
      .mockResolvedValueOnce({ data: { competencia: '2026-09-01', recalculados: 1, ignorados: 0, valorTotal: 900 } })
      .mockResolvedValueOnce({ data: { competencia: '2026-09-01', fechados: 1, ignorados: 0, valorTotal: 900 } })
      .mockResolvedValue({ data: undefined })

    await expect(listarFaturamentos('2026-09', 'ABERTO')).resolves.toHaveLength(1)
    await expect(obterResumoFaturamento('2026-09')).resolves.toEqual(resumo)
    await expect(gerarCompetencia('2026-09')).resolves.toMatchObject({ gerados: 1, valorTotal: 900 })
    await expect(recalcularCompetencia('2026-09')).resolves.toMatchObject({ recalculados: 1, valorTotal: 900 })
    await expect(fecharCompetencia('2026-09')).resolves.toMatchObject({ fechados: 1, valorTotal: 900 })
    await fecharFaturamento('fat-1')
    await cancelarFaturamento('fat-2')

    expect(get).toHaveBeenCalledWith('/v1/faturamentos/Listar', { params: { competencia: '2026-09-01', status: 'ABERTO' }, signal: undefined })
    expect(get).toHaveBeenCalledWith('/v1/faturamentos/Resumo', { params: { competencia: '2026-09-01' }, signal: undefined })
    expect(post).toHaveBeenNthCalledWith(1, '/v1/faturamentos/GerarCompetencia', { competencia: '2026-09-01' })
    expect(post).toHaveBeenNthCalledWith(2, '/v1/faturamentos/RecalcularCompetencia', { competencia: '2026-09-01' })
    expect(post).toHaveBeenNthCalledWith(3, '/v1/faturamentos/FecharCompetencia', { competencia: '2026-09-01' })
    expect(post).toHaveBeenNthCalledWith(4, '/v1/faturamentos/Fechar/fat-1')
    expect(post).toHaveBeenNthCalledWith(5, '/v1/faturamentos/Cancelar/fat-2')
  })
})
