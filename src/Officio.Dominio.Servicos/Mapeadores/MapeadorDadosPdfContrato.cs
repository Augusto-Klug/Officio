using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Mapeadores;

internal static class MapeadorDadosPdfContrato
{
    // Chamado somente após a validação de prontidão, inclusive em regenerações.
    public static DadosPdfContrato Mapear(Contrato contrato, Parceiro parceiro, DateTime dataGeracao)
    {
        var empresa = contrato.Empresa!;
        var prospecto = contrato.Prospecto;
        var responsavel = parceiro.ResponsavelTecnico!;
        var representante = contrato.RepresentanteSocio!;
        var pessoa = representante.Pessoa;
        var enderecoContratante = empresa.EnderecoId.HasValue
            ? MapearEndereco(empresa.Endereco!)
            : new DadosEnderecoContrato(
                prospecto.Logradouro!, prospecto.Numero!, prospecto.Complemento, prospecto.Bairro!, prospecto.Cep!,
                prospecto.Municipio?.Nome ?? prospecto.Cidade!, prospecto.Municipio?.Estado.Sigla ?? prospecto.Uf!);

        return new DadosPdfContrato
        {
            NumeroContrato = contrato.Numero,
            DataEmissao = dataGeracao,
            Contratada = new DadosContratada(parceiro.RazaoSocial, parceiro.Cnpj, parceiro.Crc!,
                MapearEndereco(parceiro.Endereco!),
                new DadosResponsavelTecnico(responsavel.Nome, responsavel.Profissao, responsavel.Cpf,
                    responsavel.Crc, MapearEndereco(responsavel.Endereco!))),
            Contratante = new DadosContratante(empresa.RazaoSocial, empresa.NomeFantasia, empresa.Cnpj,
                empresa.RegimeTributario, enderecoContratante, empresa.Email, empresa.Telefone,
                new DadosRepresentanteContrato(representante.TipoRelacao!.Descricao, pessoa.Nome, pessoa.Profissao!,
                    pessoa.Cpf!, MapearEndereco(pessoa.Endereco!))),
            ValorHonorarioMensal = contrato.ValorHonorarioMensal!.Value,
            ValorUnitarioPorFolha = contrato.ValorUnitarioPorFolha!.Value,
            DiaVencimento = contrato.DiaVencimentoComercial,
            DataInicio = DateOnly.FromDateTime(contrato.DataInicio),
            DataFim = contrato.DataFim.HasValue ? DateOnly.FromDateTime(contrato.DataFim.Value) : null,
            DuracaoAnos = contrato.DuracaoAnos!.Value,
            CompetenciaInicialHonorarios = contrato.CompetenciaInicialHonorarios!.Value,
            ClausulasExtras = contrato.ClausulasExtras
        };
    }

    private static DadosEnderecoContrato MapearEndereco(Endereco endereco)
        => new(endereco.Logradouro, endereco.Numero!, endereco.Complemento, endereco.Bairro!, endereco.Cep!,
            endereco.Municipio.Nome, endereco.Municipio.Estado.Sigla);
}
