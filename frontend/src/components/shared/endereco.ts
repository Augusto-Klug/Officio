export interface EnderecoCadastro {
  cep: string; logradouro: string; numero: string; complemento: string; bairro: string
  cidade: string; uf: string; municipioCodigoIbge: string
}
export function novoEndereco(): EnderecoCadastro {
  return { cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidade: '', uf: '', municipioCodigoIbge: '' }
}
