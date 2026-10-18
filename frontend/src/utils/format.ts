import { formatarCnpj, somenteDigitos } from './inputMasks'

const dateFormat = new Intl.DateTimeFormat('pt-BR')
const saoPauloDateTimeFormat = new Intl.DateTimeFormat('pt-BR', {
  dateStyle: 'short',
  timeStyle: 'short',
  timeZone: 'America/Sao_Paulo',
})
const currencyFormat = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
export function formatDate(value: string | null): string {
  if (!value || Number.isNaN(Date.parse(value))) return 'Não informado'
  const dataSomente = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value)
  if (dataSomente) return dateFormat.format(new Date(Number(dataSomente[1]), Number(dataSomente[2]) - 1, Number(dataSomente[3])))
  return dateFormat.format(new Date(value))
}
export function formatCnpj(value: string | null): string {
  if (!value) return 'CNPJ não informado'
  return somenteDigitos(value).length === 14 ? formatarCnpj(value) : value
}
export function formatCurrency(value: number): string {
  return currencyFormat.format(value)
}

export function formatSaoPauloDateTime(value: string): string {
  if (!value || Number.isNaN(Date.parse(value))) return 'Não informado'
  const possuiFuso = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(value)
  return saoPauloDateTimeFormat.format(new Date(possuiFuso ? value : `${value}Z`))
}
