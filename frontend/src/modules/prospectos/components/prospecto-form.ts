import type { CriarProspectoPayload } from '../types/prospecto'
import { formatarCep, formatarCnpj, formatarCpf, formatarTelefone, somenteDigitos } from '../../../utils/inputMasks'

export { formatarCep, formatarCnpj, formatarCpf, formatarTelefone, somenteDigitos } from '../../../utils/inputMasks'

export type ProspectoFormErrors = Record<string, string>

export function novoProspecto(): CriarProspectoPayload {
  return {
    razaoSocial: '', nomeFantasia: null, cnpj: null, inscricaoEstadual: null, inscricaoMunicipal: null,
    tipoEmpresa: null, regimeTributario: null, porteEmpresa: null, cnaePrincipal: null, dataAbertura: null,
    email: null, telefone: null, whatsapp: null, nomeContato: null, cep: null, logradouro: null,
    numero: null, complemento: null, bairro: null, cidade: null, uf: null, municipioCodigoIbge: null,
    origem: null, socios: [],
  }
}

export function montarFormularioProspecto(dados: CriarProspectoPayload = novoProspecto()) {
  return {
    razaoSocial: dados.razaoSocial, nomeFantasia: dados.nomeFantasia || '', cnpj: formatarCnpj(dados.cnpj || ''),
    inscricaoEstadual: dados.inscricaoEstadual || '', inscricaoMunicipal: dados.inscricaoMunicipal || '',
    tipoEmpresa: dados.tipoEmpresa || '', regimeTributario: dados.regimeTributario,
    porteEmpresa: dados.porteEmpresa || '', cnaePrincipal: dados.cnaePrincipal || '',
    dataAbertura: dados.dataAbertura?.slice(0, 10) || '', email: dados.email || '',
    telefone: formatarTelefone(dados.telefone), whatsapp: formatarTelefone(dados.whatsapp), nomeContato: dados.nomeContato || '',
    cep: formatarCep(dados.cep || ''), logradouro: dados.logradouro || '', numero: dados.numero || '',
    complemento: dados.complemento || '', bairro: dados.bairro || '', cidade: dados.cidade || '',
    uf: dados.uf || '', municipioCodigoIbge: dados.municipioCodigoIbge || '', origem: dados.origem || '',
    socios: dados.socios.map((socio) => ({ ...socio, cpf: formatarCpf(socio.cpf || '') })),
  }
}

function vazio(value: string | null): boolean {
  return !value?.trim()
}

export function validarProspecto(payload: CriarProspectoPayload, cepInvalido = false): ProspectoFormErrors {
  const errors: ProspectoFormErrors = {}
  if (vazio(payload.razaoSocial)) errors.razaoSocial = 'Informe a razão social.'
  else if (payload.razaoSocial.trim().length > 300) errors.razaoSocial = 'Use no máximo 300 caracteres.'

  if (payload.cnpj && somenteDigitos(payload.cnpj).length !== 14) errors.cnpj = 'Informe um CNPJ com 14 dígitos.'

  if (payload.cnaePrincipal && somenteDigitos(payload.cnaePrincipal).length !== 7) errors.cnaePrincipal = 'Informe um CNAE com 7 digitos.'
  if (payload.cep && somenteDigitos(payload.cep).length !== 8) errors.cep = 'Informe um CEP com 8 digitos.'
  else if (payload.cep && cepInvalido) errors.cep = 'CEP inválido. Informe um CEP válido para continuar o cadastro.'
  if (payload.uf && payload.uf.trim().length !== 2) errors.uf = 'Informe a UF com 2 letras.'

  const possuiEndereco = Boolean(
    payload.cep || payload.logradouro || payload.numero || payload.complemento || payload.bairro || payload.uf || payload.municipioCodigoIbge,
  )
  if (possuiEndereco && !payload.uf) errors.uf = 'Selecione a UF.'
  if (possuiEndereco && !payload.municipioCodigoIbge) errors.municipioCodigoIbge = 'Selecione o municipio.'

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
  const documento = (value: string | null | undefined) => value ? somenteDigitos(value) || null : null
  const texto = (value: string | null | undefined) => value?.trim() || null
  return {
    ...payload,
    razaoSocial: payload.razaoSocial.trim(),
    nomeFantasia: texto(payload.nomeFantasia),
    cnpj: documento(payload.cnpj),
    inscricaoEstadual: texto(payload.inscricaoEstadual),
    inscricaoMunicipal: texto(payload.inscricaoMunicipal),
    tipoEmpresa: texto(payload.tipoEmpresa),
    porteEmpresa: texto(payload.porteEmpresa),
    cnaePrincipal: documento(payload.cnaePrincipal),
    dataAbertura: payload.dataAbertura || null,
    email: texto(payload.email),
    telefone: documento(payload.telefone),
    whatsapp: documento(payload.whatsapp),
    nomeContato: texto(payload.nomeContato),
    cep: documento(payload.cep),
    logradouro: texto(payload.logradouro),
    numero: texto(payload.numero),
    complemento: texto(payload.complemento),
    bairro: texto(payload.bairro),
    cidade: texto(payload.cidade),
    uf: texto(payload.uf)?.toUpperCase() || null,
    municipioCodigoIbge: texto(payload.municipioCodigoIbge),
    origem: texto(payload.origem),
    regimeTributario: payload.regimeTributario,
    socios: payload.socios.map((socio) => ({ ...socio, nome: socio.nome.trim(), cpf: documento(socio.cpf) })),
  }
}
