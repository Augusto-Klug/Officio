using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Contratos;

public sealed record EmpresaPortalClienteDto(Guid Id, string RazaoSocial, string? Cnpj);

public sealed record CriarAdmissaoPortalRequisicao(
    Guid EmpresaId,
    string EmailFuncionario);

public sealed record AdmissaoPortalCriadaDto(Guid Id, string EmailFuncionario, bool LinkFuncionarioEnviado);

public sealed record VisaoPortalClienteDto(
    IReadOnlyList<EmpresaPortalClienteDto> Empresas,
    IReadOnlyList<AdmissaoResumoDto> Admissoes,
    IReadOnlyList<ContratoDto> Contratos);
