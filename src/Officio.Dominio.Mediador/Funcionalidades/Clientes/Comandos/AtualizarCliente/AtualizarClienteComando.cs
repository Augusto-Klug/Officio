using MediatR;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Clientes.Comandos.AtualizarCliente;

public record AtualizarClienteComando(
    Guid ClienteId,
    string RazaoSocial,
    string? NomeFantasia,
    string? Cnpj,
    string? Cpf,
    RegimeTributario? RegimeTributario,
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
    IReadOnlyCollection<CriarSocioRequisicao>? Socios = null,
    bool ConfirmarCancelamentoContratos = false,
    IReadOnlyCollection<Guid>? ContratosConfirmados = null,
    string? Complemento = null) : IRequest<Resultado<Guid>>;
