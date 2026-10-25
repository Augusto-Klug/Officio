using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public class ClienteServico(
    IClienteRepositorio clienteRepositorio,
    IServicoEnderecoComercial servicoEnderecoComercial,
    IPublicadorAtualizacaoTempoReal publicadorAtualizacaoTempoReal) : IClienteServico
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
            cliente.Empresa.AtualizarDadosCadastrais(
                requisicao.RazaoSocial,
                requisicao.NomeFantasia,
                requisicao.Cnpj,
                requisicao.Email,
                requisicao.Telefone,
                requisicao.Whatsapp,
                requisicao.RegimeTributario?.ToString());

            var endereco = await servicoEnderecoComercial.CriarAsync(
                new EnderecoComercialRequisicao(
                    requisicao.Cep,
                    requisicao.Logradouro,
                    requisicao.Numero,
                    null,
                    requisicao.Bairro,
                    requisicao.Cidade,
                    requisicao.Uf),
                ct);

            if (!endereco.Sucesso)
                return Resultado<Guid>.CriarFalha(endereco.Erro ?? "Nao foi possivel atualizar o endereco.");

            if (endereco.Valor is not null)
            {
                cliente.Empresa.VincularEndereco(endereco.Valor.Id);
                cliente.VincularEndereco(endereco.Valor.Id);
            }
        }
        catch (ArgumentException exception)
        {
            return Resultado<Guid>.CriarFalha(exception.Message);
        }

        await clienteRepositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(cliente, ct);

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
        await PublicarAtualizacaoAsync(cliente, ct);

        return Resultado<bool>.CriarSucesso(true);
    }

    private Task PublicarAtualizacaoAsync(Cliente cliente, CancellationToken ct)
        => publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(cliente.TenantId, EntidadesAtualizacaoTempoReal.Cliente, cliente.Id),
            ct);

    private static ClienteDto Mapear(Cliente cliente)
        => new(
            cliente.Id,
            cliente.EmpresaId,
            cliente.Empresa.RazaoSocial,
            cliente.Empresa.NomeFantasia,
            cliente.Empresa.Cnpj,
            null,
            cliente.Empresa.RegimeTributario,
            cliente.SciCodigo,
            cliente.Ativo,
            cliente.Empresa.Email,
            cliente.Empresa.Telefone,
            cliente.Empresa.Whatsapp,
            ObterCep(cliente),
            ObterLogradouro(cliente),
            ObterNumero(cliente),
            ObterBairro(cliente),
            ObterCidade(cliente),
            ObterUf(cliente),
            cliente.Observacoes,
            cliente.ProspectOrigemId,
            cliente.Socios.Count,
            cliente.Contratos.Count,
            cliente.Documentos.Count);

    private static ClienteDetalheDto MapearDetalhe(Cliente cliente)
        => new(
            cliente.Id,
            cliente.EmpresaId,
            cliente.Empresa.RazaoSocial,
            cliente.Empresa.NomeFantasia,
            cliente.Empresa.Cnpj,
            null,
            cliente.Empresa.RegimeTributario,
            cliente.SciCodigo,
            cliente.Ativo,
            cliente.Empresa.Email,
            cliente.Empresa.Telefone,
            cliente.Empresa.Whatsapp,
            ObterCep(cliente),
            ObterLogradouro(cliente),
            ObterNumero(cliente),
            ObterBairro(cliente),
            ObterCidade(cliente),
            ObterUf(cliente),
            cliente.Observacoes,
            cliente.ProspectOrigemId,
            cliente.InicioRelacionamentoEm,
            cliente.Socios
                .Select(socio => new ClienteSocioDto(socio.Id, socio.Nome, socio.Cpf, socio.Participacao ?? 0))
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

    private static string? ObterCep(Cliente cliente)
        => cliente.Empresa.Endereco?.Cep ?? cliente.Cep;

    private static string? ObterLogradouro(Cliente cliente)
        => cliente.Empresa.Endereco?.Logradouro ?? cliente.Logradouro;

    private static string? ObterNumero(Cliente cliente)
        => cliente.Empresa.Endereco?.Numero ?? cliente.Numero;

    private static string? ObterBairro(Cliente cliente)
        => cliente.Empresa.Endereco?.Bairro ?? cliente.Bairro;

    private static string? ObterCidade(Cliente cliente)
        => cliente.Empresa.Endereco?.Municipio.Nome ?? cliente.Cidade;

    private static string? ObterUf(Cliente cliente)
        => cliente.Empresa.Endereco?.Municipio.Estado.Sigla ?? cliente.Uf;
}
