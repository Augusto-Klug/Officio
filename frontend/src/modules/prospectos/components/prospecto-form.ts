import type { CriarProspectoPayload } from '../types/prospecto'

export type ProspectoFormErrors = Record<string, string>

export function somenteDigitos(value: string): string {
  return value.replace(/\D/g, '')
}

export function formatarCnpj(value: string): string {
  return somenteDigitos(value).slice(0, 14)
    .replace(/^(\d{2})(\d)/, '$1.$2')
    .replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/\.(\d{3})(\d)/, '.$1/$2')
    .replace(/(\d{4})(\d)/, '$1-$2')
}

export function formatarCpf(value: string): string {
  return somenteDigitos(value).slice(0, 11)
    .replace(/^(\d{3})(\d)/, '$1.$2')
    .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/(\d{3})(\d)/, '$1-$2')
}

export function novoProspecto(): CriarProspectoPayload {
  return { razaoSocial: '', cnpj: null, regimeTributario: null, socios: [] }
}

function vazio(value: string | null): boolean {
  return !value?.trim()
}

export function validarProspecto(payload: CriarProspectoPayload): ProspectoFormErrors {
  const errors: ProspectoFormErrors = {}
  if (vazio(payload.razaoSocial)) errors.razaoSocial = 'Informe a razão social.'
  else if (payload.razaoSocial.trim().length > 300) errors.razaoSocial = 'Use no máximo 300 caracteres.'

  if (payload.cnpj && somenteDigitos(payload.cnpj).length !== 14) errors.cnpj = 'Informe um CNPJ com 14 dígitos.'

  let participacaoTotal = 0
  payload.socios.forEach((socio, index) => {
    if (vazio(socio.nome)) errors[`socios.${index}.nome`] = 'Informe o nome do sócio.'
    if (socio.cpf && somenteDigitos(socio.cpf).length !== 11) errors[`socios.${index}.cpf`] = 'Informe um CPF com 11 dígitos.'
    if (!Number.isFinite(socio.participacao) || socio.participacao < 0 || socio.participacao > 100) {
      errors[`socios.${index}.participacao`] = 'Use um percentual entre 0 e 100.'
    } else participacaoTotal += socio.participacao
  })
  if (participacaoTotal > 100) errors.socios = 'A participação total dos sócios não pode ultrapassar 100%.'
  return errors
}

export function limparPayload(payload: CriarProspectoPayload): CriarProspectoPayload {
  const documento = (value: string | null) => value ? somenteDigitos(value) || null : null
  return {
    razaoSocial: payload.razaoSocial.trim(),
    cnpj: documento(payload.cnpj),
    regimeTributario: payload.regimeTributario,
    socios: payload.socios.map((socio) => ({ ...socio, nome: socio.nome.trim(), cpf: documento(socio.cpf) })),
  }
}
