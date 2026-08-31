using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Clientes.Comandos.AtualizarCliente;

public class AtualizarClienteHandler(IClienteServico clienteServico)
    : IRequestHandler<AtualizarClienteComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(AtualizarClienteComando request, CancellationToken ct)
    {
        var requisicao = new AtualizarClienteRequisicao(
            request.ClienteId,
            request.RazaoSocial,
            request.NomeFantasia,
            request.Cnpj,
            request.Cpf,
            request.RegimeTributario,
            request.Email,
            request.Telefone,
            request.Whatsapp,
            request.Cep,
            request.Logradouro,
            request.Numero,
            request.Bairro,
            request.Cidade,
            request.Uf,
            request.Observacoes);

        return await clienteServico.AtualizarAsync(requisicao, ct);
    }
}
