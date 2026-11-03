import { naturezaCobrancaPorNome, type NovoItemOrcamento } from '../types/orcamento'

const naturezasObrigatorias = [
  { valor: naturezaCobrancaPorNome.HonorarioMensal, nome: 'Honorário mensal' },
  { valor: naturezaCobrancaPorNome.PorFolha, nome: 'Preço por folha' },
] as const

export function obterItensObrigatoriosAusentes(itens: NovoItemOrcamento[]): string[] {
  const naturezasSelecionadas = new Set(
    itens
      .filter((item) => item.itemCatalogoOrcamentoId || item.itemOrcamentoId)
      .map((item) => item.naturezaCobranca),
  )

  return naturezasObrigatorias
    .filter((natureza) => !naturezasSelecionadas.has(natureza.valor))
    .map((natureza) => natureza.nome)
}
