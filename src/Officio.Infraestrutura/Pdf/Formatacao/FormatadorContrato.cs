using System.Globalization;
using System.Text.RegularExpressions;
using Officio.Dominio.Interfaces;

namespace Officio.Infraestrutura.Pdf.Formatacao;

internal sealed partial class FormatadorContrato
{
    internal static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");
    private readonly Dictionary<string, string> valores;

    public FormatadorContrato(DadosPdfContrato dados)
    {
        var contratante = dados.Contratante;
        var contratada = dados.Contratada;
        var representante = contratante.Representante;
        var responsavel = contratada.ResponsavelTecnico;
        valores = new()
        {
            ["ContratanteRazaoSocial"] = contratante.RazaoSocial,
            ["ContratanteCnpj"] = Documento(contratante.Cnpj),
            ["ContratanteCidadeUf"] = CidadeUf(contratante.Endereco),
            ["ContratanteEndereco"] = Endereco(contratante.Endereco),
            ["RepresentanteRelacao"] = representante.TipoRelacao,
            ["RepresentanteNome"] = representante.Nome,
            ["RepresentanteProfissao"] = representante.Profissao,
            ["RepresentanteCpf"] = Documento(representante.Cpf),
            ["RepresentanteCidadeUf"] = CidadeUf(representante.Endereco),
            ["RepresentanteEndereco"] = Endereco(representante.Endereco),
            ["ContratadaRazaoSocial"] = contratada.RazaoSocial,
            ["ContratadaCnpj"] = Documento(contratada.Cnpj),
            ["ContratadaCidadeUf"] = CidadeUf(contratada.Endereco),
            ["ContratadaUf"] = contratada.Endereco.Uf,
            ["ContratadaEndereco"] = Endereco(contratada.Endereco),
            ["ContratadaCrc"] = contratada.Crc,
            ["ResponsavelNome"] = responsavel.Nome,
            ["ResponsavelProfissao"] = responsavel.Profissao,
            ["ResponsavelCrc"] = responsavel.Crc,
            ["ResponsavelUf"] = responsavel.Endereco.Uf,
            ["ResponsavelCpf"] = Documento(responsavel.Cpf),
            ["ResponsavelCidadeUf"] = CidadeUf(responsavel.Endereco),
            ["ResponsavelEndereco"] = Endereco(responsavel.Endereco),
            ["HonorarioMensal"] = Moeda(dados.ValorHonorarioMensal),
            ["PrecoPorFolha"] = Moeda(dados.ValorUnitarioPorFolha),
            ["Competencia"] = dados.CompetenciaInicialHonorarios.ToString("MM/yyyy", Cultura),
            ["DataInicio"] = dados.DataInicio.ToString("dd/MM/yyyy", Cultura),
            ["Vencimento"] = $"{dados.DiaVencimento:00} ({NumeroPorExtenso.Inteiro(dados.DiaVencimento)})",
            ["Duracao"] = $"{dados.DuracaoAnos:00} ({NumeroPorExtenso.Inteiro(dados.DuracaoAnos)}) {(dados.DuracaoAnos == 1 ? "ano" : "anos")}"
        };
    }

    public string Aplicar(string modelo) => Marcador().Replace(modelo, m =>
        valores.TryGetValue(m.Groups[1].Value, out var valor) ? valor
            : throw new InvalidOperationException($"Marcador contratual desconhecido: {m.Groups[1].Value}."));

    internal static string Moeda(decimal valor)
    {
        valor = decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
        return $"{valor.ToString("C2", Cultura)} ({NumeroPorExtenso.Moeda(valor)})";
    }
    internal static string CidadeUf(DadosEnderecoContrato endereco) => $"{endereco.Cidade}/{endereco.Uf}";
    internal static string Endereco(DadosEnderecoContrato endereco)
    {
        var complemento = string.IsNullOrWhiteSpace(endereco.Complemento) ? "" : $", {endereco.Complemento}";
        var cep = Digitos(endereco.Cep);
        if (cep.Length == 8) cep = cep.Insert(5, "-");
        return $"{endereco.Logradouro}, n° {endereco.Numero}{complemento}, Bairro {endereco.Bairro}, CEP {cep}";
    }

    internal static string Documento(string documento)
    {
        var digitos = Digitos(documento);
        return digitos.Length switch
        {
            11 => $"{digitos[..3]}.{digitos[3..6]}.{digitos[6..9]}-{digitos[9..]}",
            14 => $"{digitos[..2]}.{digitos[2..5]}.{digitos[5..8]}/{digitos[8..12]}-{digitos[12..]}",
            _ => documento
        };
    }

    private static string Digitos(string valor) => string.Concat(valor.Where(char.IsAsciiDigit));

    [GeneratedRegex(@"\{\{([A-Za-z]+)\}\}")]
    private static partial Regex Marcador();
}
