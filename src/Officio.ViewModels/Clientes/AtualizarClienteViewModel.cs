using Officio.ViewModels.Enumeradores;
using Officio.ViewModels.Prospectos;

namespace Officio.ViewModels.Clientes;

public record AtualizarClienteViewModel(
    string RazaoSocial,
    string? NomeFantasia,
    string? Cnpj,
    string? Cpf,
    RegimeTributarioViewModel? RegimeTributario,
    string? Email,
    string? Telefone,
    string? Whatsapp,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Observacoes,
    string? InscricaoEstadual = null,
    string? InscricaoMunicipal = null,
    string? TipoEmpresa = null,
    string? MunicipioCodigoIbge = null,
    List<CriarSocioViewModel>? Socios = null,
    bool ConfirmarCancelamentoContratos = false,
    List<Guid>? ContratosConfirmados = null,
    string? Complemento = null);
