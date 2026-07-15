using Officio.Dominio.Enumeradores;
using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

public record CriarProspectoComando(
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
    List<CriarSocioDto>? Socios) : IRequest<Resultado<ProspectoCriadoDto>>;

public record CriarSocioDto(string Nome, string? Cpf, decimal Participacao);
public record ProspectoCriadoDto(Guid Id, string RazaoSocial);





