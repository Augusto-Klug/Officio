export function somenteDigitos(value: string | null | undefined): string {
  return (value ?? '').replace(/\D/g, '')
}

export function formatarCpf(value: string | null | undefined): string {
  return somenteDigitos(value).slice(0, 11)
    .replace(/^(\d{3})(\d)/, '$1.$2')
    .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/(\d{3})(\d{1,2})$/, '$1-$2')
}

export function formatarCnpj(value: string | null | undefined): string {
  return somenteDigitos(value).slice(0, 14)
    .replace(/^(\d{2})(\d)/, '$1.$2')
    .replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/\.(\d{3})(\d)/, '.$1/$2')
    .replace(/(\d{4})(\d)/, '$1-$2')
}

export function formatarCep(value: string | null | undefined): string {
  return somenteDigitos(value).slice(0, 8).replace(/^(\d{5})(\d)/, '$1-$2')
}

export function formatarTelefone(value: string | null | undefined): string {
  const digits = somenteDigitos(value).slice(0, 11)
  if (digits.length <= 2) return digits.length === 2 ? `(${digits})` : digits

  const ddd = `(${digits.slice(0, 2)}) `
  const numero = digits.slice(2)
  const tamanhoPrefixo = digits.length > 10 ? 5 : 4
  if (numero.length <= tamanhoPrefixo) return `${ddd}${numero}`
  return `${ddd}${numero.slice(0, tamanhoPrefixo)}-${numero.slice(tamanhoPrefixo)}`
}

export function formatarPis(value: string | null | undefined): string {
  const digits = somenteDigitos(value).slice(0, 11)
  let resultado = digits.slice(0, 3)
  if (digits.length > 3) resultado += `.${digits.slice(3, 8)}`
  if (digits.length > 8) resultado += `.${digits.slice(8, 10)}`
  if (digits.length > 10) resultado += `-${digits[10]}`
  return resultado
}

export function formatarCrc(value: string | null | undefined): string {
  const digits = somenteDigitos(value).slice(0, 8)
  const uf = (value?.toUpperCase().match(/[A-Z]/g) ?? []).slice(0, 2).join('')
  const sequencial = digits.slice(1, 7)
  let resultado = digits.slice(0, 1) + uf + sequencial

  if (sequencial.length === 6) resultado += '/S-'
  if (digits.length === 8) resultado += digits[7]

  return resultado
}

export function formatarDocumentoFiscal(value: string | null | undefined): string {
  const digits = somenteDigitos(value)
  if (digits.length === 11) return formatarCpf(digits)
  if (digits.length === 14) return formatarCnpj(digits)
  return value?.trim() ?? ''
}
