using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public class ClienteServico(IClienteRepositorio clienteRepositorio) : IClienteServico
{
    public async Task<Resultado<List<ClienteDto>>> ListarAsync(
        string? busca = null,
        bool? ativo = null,
        CancellationToken ct = default)
    {
        var clientes = await clienteRepositorio.ListarAsync(busca, ativo, ct);
        return Resultado<List<ClienteDto>>.CriarSucesso(clientes.Select(Mapear).ToList());
    }

    public async Task<Resultado<ClienteDetalheDto>> ObterPorIdAsync(
        Guid clienteId,
        CancellationToken ct = default)
    {
        var cliente = await clienteRepositorio.ObterDetalheAsync(clienteId, ct);
        if (cliente is null)
            return Resultado<ClienteDetalheDto>.CriarFalha("Cliente nao encontrado.");

        return Resultado<ClienteDetalheDto>.CriarSucesso(MapearDetalhe(cliente));
    }

    public async Task<Resultado<Guid>> AtualizarAsync(
        AtualizarClienteRequisicao requisicao,
        CancellationToken ct = default)
    {
        var cliente = await clienteRepositorio.ObterPorIdAsync(requisicao.ClienteId, ct);
        if (cliente is null)
            return Resultado<Guid>.CriarFalha("Cliente nao encontrado.");

        try
        {
            cliente.AtualizarDados(
                requisicao.RazaoSocial,
                requisicao.NomeFantasia,
                requisicao.Cnpj,
                requisicao.Cpf,
                requisicao.RegimeTributario,
                requisicao.Email,
                requisicao.Telefone,
                requisicao.Whatsapp,
                requisicao.Cep,
                requisicao.Logradouro,
                requisicao.Numero,
                requisicao.Bairro,
                requisicao.Cidade,
                requisicao.Uf,
                requisicao.Observacoes);
        }
        catch (ArgumentException exception)
        {
            return Resultado<Guid>.CriarFalha(exception.Message);
        }

        await clienteRepositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(cliente.Id);
    }

    public async Task<Resultado<bool>> VincularCodigoSciAsync(
        VincularCodigoSciRequisicao requisicao,
        CancellationToken ct = default)
    {
        var cliente = await clienteRepositorio.ObterPorIdAsync(requisicao.ClienteId, ct);
        if (cliente is null)
            return Resultado<bool>.CriarFalha("Cliente nao encontrado.");

        try
        {
            cliente.VincularSciCodigo(requisicao.SciCodigo);
        }
        catch (ArgumentException exception)
        {
            return Resultado<bool>.CriarFalha(exception.Message);
        }

        await clienteRepositorio.SalvarAlteracoesAsync(ct);

        return Resultado<bool>.CriarSucesso(true);
    }

    private static ClienteDto Mapear(Cliente cliente)
        => new(
            cliente.Id,
            cliente.RazaoSocial,
            cliente.NomeFantasia,
            cliente.Cnpj,
            cliente.Cpf,
            cliente.RegimeTributario?.ToString(),
            cliente.SciCodigo,
            cliente.Ativo,
            cliente.Email,
            cliente.Telefone,
            cliente.Whatsapp,
            cliente.Cep,
            cliente.Logradouro,
            cliente.Numero,
            cliente.Bairro,
            cliente.Cidade,
            cliente.Uf,
            cliente.Observacoes,
            cliente.ProspectOrigemId,
            cliente.Socios.Count,
            cliente.Contratos.Count,
            cliente.Documentos.Count);

    private static ClienteDetalheDto MapearDetalhe(Cliente cliente)
        => new(
            cliente.Id,
            cliente.RazaoSocial,
            cliente.NomeFantasia,
            cliente.Cnpj,
            cliente.Cpf,
            cliente.RegimeTributario?.ToString(),
            cliente.SciCodigo,
            cliente.Ativo,
            cliente.Email,
            cliente.Telefone,
            cliente.Whatsapp,
            cliente.Cep,
            cliente.Logradouro,
            cliente.Numero,
            cliente.Bairro,
            cliente.Cidade,
            cliente.Uf,
            cliente.Observacoes,
            cliente.ProspectOrigemId,
            cliente.InicioRelacionamentoEm,
            cliente.Socios
                .Select(socio => new ClienteSocioDto(socio.Id, socio.Nome, socio.Cpf, socio.Participacao))
                .ToList(),
            cliente.Contratos
                .Select(contrato => new ClienteContratoDto(
                    contrato.Id,
                    contrato.Numero,
                    contrato.Status.ToString(),
                    contrato.ValorTotal,
                    contrato.DataInicio,
                    contrato.DataFim))
                .ToList(),
            cliente.Documentos
                .Select(documento => new ClienteDocumentoDto(
                    documento.Id,
                    documento.NomeArquivo,
                    documento.Tipo.ToString(),
                    documento.Legivel,
                    documento.QualidadeEstimada))
                .ToList());
}
