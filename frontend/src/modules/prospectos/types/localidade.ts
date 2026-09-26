export interface EstadoLocalidade {
  codigoIbge: string
  sigla: string
  nome: string
}

export interface MunicipioLocalidade {
  codigoIbge: string
  nome: string
  uf: string
}

export interface EnderecoCepLocalidade {
  cep: string
  logradouro: string | null
  complemento: string | null
  bairro: string | null
  cidade: string
  uf: string
  municipioCodigoIbge: string
}
