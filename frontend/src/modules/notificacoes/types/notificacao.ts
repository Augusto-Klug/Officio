export type FiltroNotificacoes = 'todas' | 'nao-lidas'

export interface Notificacao {
  id: string
  tipo: string
  titulo: string
  mensagem: string | null
  referenciaId: string | null
  lida: boolean
  lidaEm: string | null
  criadoEm: string
}

export interface QuantidadeNotificacoesLidas {
  quantidade: number
}
