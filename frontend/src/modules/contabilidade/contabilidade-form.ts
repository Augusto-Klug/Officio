import { somenteDigitos } from '../../utils/inputMasks'

import { novoEndereco, type EnderecoCadastro } from '../../components/shared/endereco'
export { novoEndereco, type EnderecoCadastro } from '../../components/shared/endereco'
export interface ResponsavelTecnico {
  nome: string; cpf: string; profissao: string; crc: string; endereco: EnderecoCadastro | null
}
export interface DadosContabilidade {
  razaoSocial: string; nomeFantasia: string; cnpj: string; crc: string; email: string; telefone: string
  endereco: EnderecoCadastro | null; responsavelTecnico: ResponsavelTecnico | null
}
export function temEndereco(e: EnderecoCadastro): boolean { return Object.values(e).some(v => v.trim()) }
export function validarEndereco(e: EnderecoCadastro): string {
  if (!temEndereco(e)) return ''
  if (e.cep && somenteDigitos(e.cep).length !== 8) return 'Informe um CEP com 8 dígitos.'
  if (!e.logradouro.trim() || !e.uf || !e.municipioCodigoIbge) return 'Complete o logradouro, a UF e a cidade do endereço.'
  return ''
}
export function validarDados(d: DadosContabilidade): string {
  if (!d.razaoSocial.trim()) return 'Informe a razão social.'
  if (somenteDigitos(d.cnpj).length !== 14) return 'Informe um CNPJ com 14 dígitos.'
  if (d.email && !/^\S+@\S+\.\S+$/.test(d.email)) return 'Informe um e-mail válido.'
  if (d.endereco) { const erro = validarEndereco(d.endereco); if (erro) return `Contabilidade: ${erro}` }
  const r = d.responsavelTecnico
  if (r) {
    if (!r.nome.trim() || !r.profissao.trim() || !r.crc.trim()) return 'Complete nome, profissão e CRC do responsável técnico.'
    if (somenteDigitos(r.cpf).length !== 11) return 'Informe um CPF com 11 dígitos para o responsável técnico.'
    if (r.endereco) { const erro = validarEndereco(r.endereco); if (erro) return `Responsável técnico: ${erro}` }
  }
  return ''
}
export function prepararDados(d: DadosContabilidade): DadosContabilidade {
  const endereco = (e: EnderecoCadastro | null): EnderecoCadastro | null => e && temEndereco(e)
    ? { cep: somenteDigitos(e.cep), logradouro: e.logradouro.trim(), numero: e.numero.trim(), complemento: e.complemento.trim(),
      bairro: e.bairro.trim(), cidade: e.cidade.trim(), uf: e.uf, municipioCodigoIbge: e.municipioCodigoIbge } : null
  return { ...d, razaoSocial: d.razaoSocial.trim(), nomeFantasia: d.nomeFantasia.trim(), cnpj: somenteDigitos(d.cnpj),
    crc: d.crc.trim(), email: d.email.trim(), telefone: somenteDigitos(d.telefone), endereco: endereco(d.endereco),
    responsavelTecnico: d.responsavelTecnico ? { ...d.responsavelTecnico, nome: d.responsavelTecnico.nome.trim(),
      cpf: somenteDigitos(d.responsavelTecnico.cpf), profissao: d.responsavelTecnico.profissao.trim(),
      crc: d.responsavelTecnico.crc.trim(), endereco: endereco(d.responsavelTecnico.endereco) } : null }
}
