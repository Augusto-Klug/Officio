import type { AdmissaoDetalhe, AdmissaoResumo, AtualizarFichaEmpresaPayload } from '../../admissoes/types/admissao'
import type { Contrato } from '../../contratos/types/contrato'

export interface EmpresaPortalCliente {
  id: string
  razaoSocial: string
  cnpj: string | null
}

export interface VisaoPortalCliente {
  empresas: EmpresaPortalCliente[]
  admissoes: AdmissaoResumo[]
  contratos: Contrato[]
}

export interface CriarAdmissaoPortalPayload {
  empresaId: string
  emailFuncionario: string
}

export interface AdmissaoPortalCriada {
  id: string
  emailFuncionario: string
  linkFuncionarioEnviado: boolean
}

export type DadosEmpregadorPortal = AtualizarFichaEmpresaPayload
export type DetalheAdmissaoPortal = AdmissaoDetalhe
