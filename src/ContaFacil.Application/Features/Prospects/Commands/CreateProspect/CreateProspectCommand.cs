using ContaFacil.Application.Common;
using ContaFacil.Domain.Enums;
using MediatR;

namespace ContaFacil.Application.Features.Prospects.Commands.CreateProspect;

public record CreateProspectCommand(
    string RazaoSocial,
    string? NomeFantasia,
    string? Cnpj,
    string? Cpf,
    RegimeTributario? RegimeTributario,
    string? PorteEmpresa,
    string? CnaePrincipal,
    DateTime? DataAbertura,
    string? Email,
    string? Telefone,
    string? Whatsapp,
    string? NomeContato,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Origem,
    Guid? ResponsavelId,
    List<CreateSocioDto>? Socios) : IRequest<Result<ProspectCreatedDto>>;
