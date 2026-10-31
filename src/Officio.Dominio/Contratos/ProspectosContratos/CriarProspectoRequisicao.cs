using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Contratos;

public record CriarProspectoRequisicao(
    string RazaoSocial,
    string? Cnpj,
    RegimeTributario? RegimeTributario,
    IReadOnlyCollection<CriarSocioRequisicao>? Socios,
    string? NomeFantasia = null,
    string? Cpf = null,
    string? PorteEmpresa = null,
    string? CnaePrincipal = null,
    DateTime? DataAbertura = null,
    string? Email = null,
    string? Telefone = null,
    string? Whatsapp = null,
    string? NomeContato = null,
    string? Cep = null,
    string? Logradouro = null,
    string? Numero = null,
    string? Complemento = null,
    string? Bairro = null,
    string? Cidade = null,
    string? Uf = null,
    string? MunicipioCodigoIbge = null,
    string? Origem = null,
    Guid? ResponsavelId = null,
    string? InscricaoEstadual = null,
    string? InscricaoMunicipal = null,
    string? TipoEmpresa = null,
    bool ConfirmarCancelamentoContratos = false,
    IReadOnlyCollection<Guid>? ContratosConfirmados = null);
