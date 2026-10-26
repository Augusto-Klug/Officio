import { http } from '../../api/http'
import { record, field, optionalString, requiredString } from '../../api/contracts'
import type { DadosContabilidade, EnderecoCadastro } from './contabilidade-form'

export function normalizeContabilidade(value: unknown): DadosContabilidade {
  const d = record(value)
  const texto = (r: Record<string, unknown>, key: string) => optionalString(field(r, key)) || ''
  function endereco(value: unknown): EnderecoCadastro | null {
    if (value == null) return null
    const e = record(value)
    return { cep: texto(e, 'cep'), logradouro: requiredString(field(e, 'logradouro')), numero: texto(e, 'numero'),
      complemento: texto(e, 'complemento'), bairro: texto(e, 'bairro'), cidade: texto(e, 'cidade'),
      uf: texto(e, 'uf'), municipioCodigoIbge: texto(e, 'municipioCodigoIbge') }
  }
  const r = field(d, 'responsavelTecnico')
  const tecnico = r == null ? null : record(r)
  return { razaoSocial: requiredString(field(d, 'razaoSocial')), cnpj: requiredString(field(d, 'cnpj')),
    nomeFantasia: texto(d, 'nomeFantasia'), crc: texto(d, 'crc'), email: texto(d, 'email'), telefone: texto(d, 'telefone'),
    endereco: endereco(field(d, 'endereco')), responsavelTecnico: tecnico ? {
      nome: texto(tecnico, 'nome'), cpf: texto(tecnico, 'cpf'), profissao: texto(tecnico, 'profissao'),
      crc: texto(tecnico, 'crc'), endereco: endereco(field(tecnico, 'endereco')) } : null }
}
export async function obterDadosContabilidade(signal?: AbortSignal): Promise<DadosContabilidade> {
  const { data } = await http.get<unknown>('/v1/contabilidade', { signal })
  return normalizeContabilidade(data)
}
export async function atualizarDadosContabilidade(dados: DadosContabilidade): Promise<DadosContabilidade> {
  const { data } = await http.put<unknown>('/v1/contabilidade', dados)
  return normalizeContabilidade(data)
}
