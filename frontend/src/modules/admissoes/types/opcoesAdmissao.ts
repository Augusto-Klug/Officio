export const sexosAdmissao = ['Masculino', 'Feminino']

export const estadosCivisAdmissao = [
  'Solteiro(a)',
  'Casado(a)',
  'União estável',
  'Divorciado(a)',
  'Viúvo(a)',
]

export const escolaridadesAdmissao = [
  'Não alfabetizado',
  'Ensino fundamental incompleto',
  'Ensino fundamental completo',
  'Ensino médio incompleto',
  'Ensino médio completo',
  'Ensino técnico incompleto',
  'Ensino técnico completo',
  'Ensino superior incompleto',
  'Ensino superior completo',
  'Pós-graduação incompleta',
  'Pós-graduação completa',
]

export const tiposContaAdmissao = ['Corrente', 'Poupança', 'Salário']

export function opcoesComValorAtual(valor: string | null | undefined, opcoes: string[]): string[] {
  const atual = valor?.trim()
  return atual && !opcoes.some((opcao) => opcao.toLocaleLowerCase() === atual.toLocaleLowerCase())
    ? [...opcoes, atual]
    : opcoes
}
