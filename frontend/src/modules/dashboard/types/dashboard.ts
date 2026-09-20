export interface DashboardVisaoGeral {
  prospectosEmAberto: number
  clientesAtivos: number
  contratosAtivos: number
  admissoesPendentes: number
  orcamentosAguardandoRetorno: number
  receitaMensalContratada: number
}

export type DashboardAlertaSeveridade = 'erro' | 'aviso' | 'info'

export interface DashboardAlerta {
  codigo: string
  severidade: DashboardAlertaSeveridade
  titulo: string
  descricao: string
  referenciaId: string
  rota: string
  dataReferencia: string
}

export interface DashboardProjecaoReceita {
  mes: string
  receitaContratada: number
}
