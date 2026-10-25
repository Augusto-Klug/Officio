import type { ApiError } from '../../../api/errors'

export const codigoErroAceiteOrcamento = {
  confirmacaoSubstituicaoNecessaria: 'ORCAMENTO_ACEITE_REQUER_CONFIRMAR_SUBSTITUICAO',
  concorrencia: 'ORCAMENTO_ACEITE_CONCORRENTE',
  bloqueioContrato: 'ORCAMENTO_SUBSTITUICAO_BLOQUEADA_CONTRATO',
} as const

export function deveAbrirConfirmacaoSubstituicao(erro: ApiError, jaConfirmou: boolean): boolean {
  return !jaConfirmou
    && erro.status === 409
    && erro.code === codigoErroAceiteOrcamento.confirmacaoSubstituicaoNecessaria
}
