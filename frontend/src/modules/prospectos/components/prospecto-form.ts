import { novoEndereco } from '../../../components/shared/endereco'
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
    inscricaoEstadual: somenteDigitos(dados.inscricaoEstadual), inscricaoMunicipal: somenteDigitos(dados.inscricaoMunicipal),
    tipoEmpresa: dados.tipoEmpresa || '', regimeTributario: dados.regimeTributario,
    porteEmpresa: dados.porteEmpresa || '', cnaePrincipal: somenteDigitos(dados.cnaePrincipal).slice(0, 7),
    dataAbertura: dados.dataAbertura?.slice(0, 10) || '', email: dados.email || '',
    telefone: formatarTelefone(dados.telefone), whatsapp: formatarTelefone(dados.whatsapp), nomeContato: dados.nomeContato || '',
    cep: formatarCep(dados.cep || ''), logradouro: dados.logradouro || '', numero: somenteDigitos(dados.numero),
    complemento: dados.complemento || '', bairro: dados.bairro || '', cidade: dados.cidade || '',
    uf: dados.uf || '', municipioCodigoIbge: dados.municipioCodigoIbge || '', origem: dados.origem || '',
    socios: dados.socios.map((socio, index) => ({ ...socio, chave: socio.id || `existente-${index}`, nome: socio.nome || '', cpf: formatarCpf(socio.cpf || ''), profissao: socio.profissao || '', tipoRelacaoId: socio.tipoRelacaoId || null, endereco: { ...novoEndereco(), ...Object.fromEntries(Object.entries(socio.endereco || {}).map(([k,v]) => [k,v || ''])) } })),
  }
}

function vazio(value: string | null | undefined): boolean {
  return !value?.trim()
}

export function validarProspecto(payload: CriarProspectoPayload, cepInvalido = false): ProspectoFormErrors {
  const errors: ProspectoFormErrors = {}
  if (vazio(payload.nomeContato)) errors.nomeContato = 'Informe o nome do contato.'
  else if ((payload.nomeContato?.length || 0) > 250) errors.nomeContato = 'Use no máximo 250 caracteres.'
  if (vazio(payload.email)) errors.email = 'Informe o e-mail do contato.'
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(payload.email!.trim())) errors.email = 'Informe um e-mail válido.'
  if (vazio(payload.telefone)) errors.telefone = 'Informe o telefone com DDD.'
  else if (![10, 11].includes(somenteDigitos(payload.telefone).length)) errors.telefone = 'Informe um telefone com DDD e 10 ou 11 dígitos.'
  if (vazio(payload.razaoSocial)) errors.razaoSocial = 'Informe a razão social.'
  if (vazio(payload.numero)) errors.numero = 'Informe o número do endereço.'
  else if ((payload.numero?.length || 0) > 30) errors.numero = 'Use no máximo 30 caracteres.'
  if (payload.razaoSocial.trim().length > 300) errors.razaoSocial = 'Use no máximo 300 caracteres.'

  if (payload.cnpj && somenteDigitos(payload.cnpj).length !== 14) errors.cnpj = 'Informe um CNPJ com 14 dígitos.'

  if (payload.cnaePrincipal && somenteDigitos(payload.cnaePrincipal).length !== 7) errors.cnaePrincipal = 'Informe um CNAE com 7 digitos.'
  if (vazio(payload.cep)) errors.cep = 'Informe o CEP.'
  else if (somenteDigitos(payload.cep).length !== 8) errors.cep = 'Informe um CEP com 8 dígitos.'
  else if (cepInvalido) errors.cep = 'CEP inválido. Informe um CEP válido para continuar o cadastro.'
  if (vazio(payload.logradouro)) errors.logradouro = 'Informe o logradouro.'
  else if ((payload.logradouro?.length || 0) > 250) errors.logradouro = 'Use no máximo 250 caracteres.'
  if (vazio(payload.bairro)) errors.bairro = 'Informe o bairro.'
  else if ((payload.bairro?.length || 0) > 150) errors.bairro = 'Use no máximo 150 caracteres.'
  if (vazio(payload.uf)) errors.uf = 'Selecione a UF.'
  else if (payload.uf!.trim().length !== 2) errors.uf = 'Informe a UF com 2 letras.'
  if (vazio(payload.cidade) || vazio(payload.municipioCodigoIbge)) errors.municipioCodigoIbge = 'Selecione o município.'

  let participacaoTotal = 0
  payload.socios.forEach((socio, index) => {
    if (socio.cpf && somenteDigitos(socio.cpf).length !== 11) errors[`socios.${index}.cpf`] = 'Informe um CPF com 11 dígitos.'
    if (socio.participacao != null && (!Number.isFinite(socio.participacao) || socio.participacao < 0 || socio.participacao > 100)) {
      errors[`socios.${index}.participacao`] = 'Use um percentual entre 0 e 100.'
    } else participacaoTotal += socio.participacao || 0
    if (socio.nome.length > 250) errors[`socios.${index}.nome`] = 'Use no máximo 250 caracteres.'
    if ((socio.profissao?.length || 0) > 250) errors[`socios.${index}.profissao`] = 'Use no máximo 250 caracteres.'
    if (socio.endereco?.cep && somenteDigitos(socio.endereco.cep).length !== 8) errors[`socios.${index}.endereco`] = 'Informe um CEP com 8 dígitos.'
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
    inscricaoEstadual: documento(payload.inscricaoEstadual),
    inscricaoMunicipal: documento(payload.inscricaoMunicipal),
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
    numero: documento(payload.numero),
    complemento: texto(payload.complemento),
    bairro: texto(payload.bairro),
    cidade: texto(payload.cidade),
    uf: texto(payload.uf)?.toUpperCase() || null,
    municipioCodigoIbge: texto(payload.municipioCodigoIbge),
    origem: texto(payload.origem),
    regimeTributario: payload.regimeTributario,
    socios: payload.socios.map((socio) => {
      const enderecoInformado = socio.endereco && Object.values(socio.endereco).some(value => Boolean(value?.trim()))
      return { ...socio, nome: socio.nome.trim(), cpf: documento(socio.cpf), profissao: texto(socio.profissao), tipoRelacaoId: socio.tipoRelacaoId || null,
        endereco: enderecoInformado ? { ...socio.endereco!, cep: somenteDigitos(socio.endereco!.cep), numero: somenteDigitos(socio.endereco!.numero) } : null }
    }),
  }
}
