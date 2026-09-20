import { describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { carregarDashboard, normalizarAlertas, normalizarProjecao, normalizarVisaoGeral } from './dashboard.service'

describe('dashboard.service', () => {
  it('normaliza a visao geral', () => {
    expect(normalizarVisaoGeral({
      prospectosEmAberto: 2,
      clientesAtivos: 3,
      contratosAtivos: 4,
      admissoesPendentes: 5,
      orcamentosAguardandoRetorno: 6,
      receitaMensalContratada: 1500.5,
    }).receitaMensalContratada).toBe(1500.5)
  })

  it('rejeita payloads inconsistentes', () => {
    expect(() => normalizarVisaoGeral({})).toThrow()
    expect(() => normalizarAlertas([{ severidade: 'desconhecida' }])).toThrow()
    expect(() => normalizarProjecao([{ mes: 'invalido', receitaContratada: 1 }])).toThrow()
  })

  it('carrega os tres recortes em paralelo', async () => {
    vi.spyOn(http, 'get')
      .mockResolvedValueOnce({ data: { prospectosEmAberto: 0, clientesAtivos: 0, contratosAtivos: 0, admissoesPendentes: 0, orcamentosAguardandoRetorno: 0, receitaMensalContratada: 0 } } as never)
      .mockResolvedValueOnce({ data: [] } as never)
      .mockResolvedValueOnce({ data: [] } as never)

    const resultado = await carregarDashboard()

    expect(resultado.alertas).toEqual([])
    expect(resultado.projecao).toEqual([])
    expect(http.get).toHaveBeenCalledTimes(3)
  })
})
