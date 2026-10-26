import { describe, expect, it } from 'vitest'
import { ApiError } from '../../../api/errors'
import { codigoErroAceiteOrcamento, deveAbrirConfirmacaoSubstituicao } from './aceite-orcamento'

describe('confirmação de substituição de orçamento', () => {
  it('abre a confirmação apenas para o conflito estruturado de aceite', () => {
    expect(deveAbrirConfirmacaoSubstituicao(new ApiError('Mensagem humana', 409, {},
      codigoErroAceiteOrcamento.confirmacaoSubstituicaoNecessaria), false)).toBe(true)
  })

  it('não abre para outros 409 ou depois que a confirmação já foi enviada', () => {
    expect(deveAbrirConfirmacaoSubstituicao(new ApiError('Outro conflito', 409), false)).toBe(false)
    expect(deveAbrirConfirmacaoSubstituicao(new ApiError('Aceite concorrente', 409, {},
      codigoErroAceiteOrcamento.concorrencia), false)).toBe(false)
    expect(deveAbrirConfirmacaoSubstituicao(new ApiError('Confirmado', 409, {},
      codigoErroAceiteOrcamento.confirmacaoSubstituicaoNecessaria), true)).toBe(false)
  })
})
