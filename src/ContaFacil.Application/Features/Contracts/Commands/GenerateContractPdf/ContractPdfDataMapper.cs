using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Domain.Entities;

namespace ContaFacil.Application.Features.Contracts.Commands.GenerateContractPdf;

public static class ContractPdfDataMapper
{
    public static ContractPdfData Map(Contract contract, Tenant tenant)
    {
        var client = contract.Cliente;
        var budget = contract.Budget;

        var contratada = new ContratadaData(
            RazaoSocial: tenant.Nome,
            Cnpj: tenant.Cnpj ?? "",
            Endereco: "",
            Cidade: "",
            Uf: "",
            Email: "",
            Telefone: "");

        var endereco = string.Join(", ",
            new[] { client.Logradouro, client.Numero }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        var contratante = new ContratanteData(
            RazaoSocial: client.RazaoSocial,
            NomeFantasia: client.NomeFantasia ?? client.RazaoSocial,
            Cnpj: client.Cnpj ?? "",
            RegimeTributario: client.RegimeTributario?.ToString() ?? "",
            Endereco: endereco,
            Cidade: client.Cidade ?? "",
            Uf: client.Uf ?? "",
            Email: client.Email ?? "",
            Telefone: client.Telefone ?? "");

        var socios = client.Socios
            .Select(s => new SocioData(s.Nome, s.Cpf ?? "", s.Participacao, Administrador: false))
            .ToList();

        var servicos = budget?.Itens
            .Select(i => new ServicoData(i.Descricao, "Mensal", i.ValorUnitario * i.Quantidade))
            .ToList() ?? [];

        return new ContractPdfData
        {
            NumeroContrato      = contract.Numero,
            DataEmissao         = contract.DataInicio,
            Versao              = 1,
            Contratada          = contratada,
            Contratante         = contratante,
            Socios              = socios,
            Servicos            = servicos,
            ValorMensalidade    = budget?.ValorMensalidade ?? contract.ValorTotal,
            ValorPorFuncionario = budget?.ValorPorFuncionario ?? 0,
            QtdFuncionarios     = budget?.QtdFuncionarios ?? 0,
            ValorTaxasExtras    = budget?.ValorTaxasExtras ?? 0,
            Desconto            = budget?.Desconto ?? 0,
            ValorTotal          = contract.ValorTotal,
            DiaVencimento       = contract.DiaVencimento,
            DataInicio          = DateOnly.FromDateTime(contract.DataInicio),
            DataFim             = contract.DataFim.HasValue
                                    ? DateOnly.FromDateTime(contract.DataFim.Value)
                                    : null,
            RenovacaoAuto       = contract.RenovacaoAutomatica,
            ClausulasExtras     = contract.ClausulasExtras,
        };
    }
}
