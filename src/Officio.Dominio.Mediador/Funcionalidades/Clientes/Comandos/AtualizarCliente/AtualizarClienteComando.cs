using MediatR;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Contratos.Response;

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
    string? Observacoes) : IRequest<Resultado<Guid>>;
