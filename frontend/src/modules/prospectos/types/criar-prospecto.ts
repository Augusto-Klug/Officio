export interface CriarProspecto {
  razaoSocial: string
  cnpj: string | null
  regimeTributario: 0 | 1 | 2 | 3 | null
  socios: { nome: string; cpf: string | null; participacao: number }[]
}

export interface ProspectoFormulario {
  razaoSocial: string
  cnpj: string
  regimeTributario: string | null
  socios: { id: number; nome: string; cpf: string; participacao: string }[]
}

export const regimesTributarios = [
  { label: 'Simples Nacional', value: '0' },
  { label: 'Lucro Presumido', value: '1' },
  { label: 'Lucro Real', value: '2' },
  { label: 'MEI', value: '3' },
]

export function validarProspecto(form: ProspectoFormulario): Record<string, string> {
  const erros: Record<string, string> = {}
  if (!form.razaoSocial.trim()) erros.razaoSocial = 'Informe a razão social.'
  else if (form.razaoSocial.trim().length > 300) erros.razaoSocial = 'Use até 300 caracteres.'
  if (form.cnpj.trim() && !/^(?:\d{14}|\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2})$/.test(form.cnpj.trim())) {
    erros.cnpj = 'Informe os 14 dígitos do CNPJ, com ou sem pontuação.'
  }
  if (form.regimeTributario !== null && !regimesTributarios.some(item => item.value === form.regimeTributario)) {
    erros.regimeTributario = 'Selecione um regime tributário válido.'
  }
  for (const socio of form.socios) {
    if (!socio.nome.trim()) erros[`socio-${socio.id}-nome`] = 'Informe o nome do sócio.'
    if (socio.cpf.trim() && !/^(?:\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})$/.test(socio.cpf.trim())) {
      erros[`socio-${socio.id}-cpf`] = 'Informe os 11 dígitos do CPF, com ou sem pontuação.'
    }
    const participacao = socio.participacao.trim().replace(',', '.')
    if (!/^\d+(?:\.\d{1,2})?$/.test(participacao) || Number(participacao) > 100) {
      erros[`socio-${socio.id}-participacao`] = 'Informe uma participação de 0 a 100%, com até duas casas decimais.'
    }
  }
  return erros
}

export function payloadProspecto(form: ProspectoFormulario): CriarProspecto {
  return {
    razaoSocial: form.razaoSocial.trim(),
    cnpj: form.cnpj.trim() ? form.cnpj.replace(/\D/g, '').replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5') : null,
    regimeTributario: form.regimeTributario === null ? null : Number(form.regimeTributario) as 0 | 1 | 2 | 3,
    socios: form.socios.map(socio => ({
      nome: socio.nome.trim(),
      cpf: socio.cpf.trim() ? socio.cpf.replace(/\D/g, '').replace(/^(\d{3})(\d{3})(\d{3})(\d{2})$/, '$1.$2.$3-$4') : null,
      participacao: Number(socio.participacao.trim().replace(',', '.')),
    })),
  }
}
