import { field, record, requiredString } from '../../../api/contracts'
import { http } from '../../../api/http'
import type { DashboardAlerta, DashboardAlertaSeveridade, DashboardProjecaoReceita, DashboardVisaoGeral } from '../types/dashboard'

function numeroNaoNegativo(value: unknown): number {
  if (typeof value !== 'number' || !Number.isFinite(value) || value < 0) {
    throw new Error('A API retornou um indicador de dashboard invalido.')
  }
  return value
}

function inteiroNaoNegativo(value: unknown): number {
  const numero = numeroNaoNegativo(value)
  if (!Number.isInteger(numero)) throw new Error('A API retornou uma quantidade de dashboard invalida.')
  return numero
}

function severidade(value: unknown): DashboardAlertaSeveridade {
  if (value !== 'erro' && value !== 'aviso' && value !== 'info') {
    throw new Error('A API retornou uma severidade de alerta invalida.')
  }
  return value
}

function data(value: unknown): string {
  const resultado = requiredString(value)
  if (Number.isNaN(Date.parse(resultado))) throw new Error('A API retornou uma data de dashboard invalida.')
  return resultado
}

export function normalizarVisaoGeral(value: unknown): DashboardVisaoGeral {
  const item = record(value)
  return {
    prospectosEmAberto: inteiroNaoNegativo(field(item, 'prospectosEmAberto')),
    clientesAtivos: inteiroNaoNegativo(field(item, 'clientesAtivos')),
    contratosAtivos: inteiroNaoNegativo(field(item, 'contratosAtivos')),
    admissoesPendentes: inteiroNaoNegativo(field(item, 'admissoesPendentes')),
    orcamentosAguardandoRetorno: inteiroNaoNegativo(field(item, 'orcamentosAguardandoRetorno')),
    receitaMensalContratada: numeroNaoNegativo(field(item, 'receitaMensalContratada')),
  }
}

export function normalizarAlertas(value: unknown): DashboardAlerta[] {
  if (!Array.isArray(value)) throw new Error('A API retornou uma lista de alertas invalida.')
  return value.map((value) => {
    const item = record(value)
    return {
      codigo: requiredString(field(item, 'codigo')),
      severidade: severidade(field(item, 'severidade')),
      titulo: requiredString(field(item, 'titulo')),
      descricao: requiredString(field(item, 'descricao')),
      referenciaId: requiredString(field(item, 'referenciaId')),
      rota: requiredString(field(item, 'rota')),
      dataReferencia: data(field(item, 'dataReferencia')),
    }
  })
}

export function normalizarProjecao(value: unknown): DashboardProjecaoReceita[] {
  if (!Array.isArray(value)) throw new Error('A API retornou uma projecao de receita invalida.')
  return value.map((value) => {
    const item = record(value)
    return {
      mes: data(field(item, 'mes')),
      receitaContratada: numeroNaoNegativo(field(item, 'receitaContratada')),
    }
  })
}

export async function carregarDashboard(signal?: AbortSignal) {
  const [visaoGeral, alertas, projecao] = await Promise.all([
    http.get<unknown>('/v1/dashboard/VisaoGeral', { signal }),
    http.get<unknown>('/v1/dashboard/Alertas', { signal }),
    http.get<unknown>('/v1/dashboard/ProjecaoReceita', { signal }),
  ])

  return {
    visaoGeral: normalizarVisaoGeral(visaoGeral.data),
    alertas: normalizarAlertas(alertas.data),
    projecao: normalizarProjecao(projecao.data),
  }
}
