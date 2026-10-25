using Officio.Dominio.Comum;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Servicos.Validadores;

internal static class ValidadorProntidaoContrato
{
    public static IReadOnlyList<string> ValidarParaGeracao(Guid tenantId, Parceiro? contratada, Contrato contrato)
    {
        var pendencias = new List<string>();
        ValidarContratada(tenantId, contratada, pendencias);
        ValidarContratante(contrato.Prospecto, contrato.Empresa, pendencias);
        if (contrato.Empresa is null || contrato.Empresa.Id != contrato.EmpresaId || contrato.Empresa.TenantId != tenantId)
            pendencias.Add("Empresa da CONTRATANTE não encontrada para o tenant atual.");

        var representante = contrato.RepresentanteSocio;
        if (representante is not null && (representante.Id != contrato.RepresentanteSocioId
            || representante.TenantId != tenantId
            || (representante.ProspectId != contrato.ProspectId
                && (!contrato.ClientId.HasValue || representante.ClientId != contrato.ClientId))))
            representante = null;
        ValidarRepresentante(contrato.RepresentanteSocioId, representante, pendencias);

        if (contrato.Orcamento is null || contrato.Orcamento.Id != contrato.BudgetId)
            pendencias.Add("Orçamento vinculado ao contrato não encontrado.");
        else
            ValidarOrcamento(tenantId, contrato.Prospecto, contrato.Orcamento, pendencias, permitirSubstituido: true);
        ValidarVigencia(contrato.DataInicio, contrato.DuracaoAnos ?? 0, contrato.CompetenciaInicialHonorarios, pendencias);
        return pendencias;
    }

    public static IReadOnlyList<string> Validar(
        Guid tenantId,
        Parceiro? contratada,
        Prospecto prospecto,
        Empresa? empresa,
        Orcamento orcamento,
        Guid? representanteSelecionadoId,
        Socio? representante,
        DateTime dataInicio,
        int duracaoAnos,
        DateOnly? competenciaInicial)
    {
        var pendencias = new List<string>();
        ValidarContratada(tenantId, contratada, pendencias);
        ValidarContratante(prospecto, empresa, pendencias);
        ValidarRepresentante(representanteSelecionadoId, representante, pendencias);
        ValidarOrcamento(tenantId, prospecto, orcamento, pendencias);
        ValidarVigencia(dataInicio, duracaoAnos, competenciaInicial, pendencias);
        return pendencias;
    }

    private static void ValidarContratada(Guid tenantId, Parceiro? parceiro, List<string> pendencias)
    {
        if (parceiro is null || parceiro.Id != tenantId)
        {
            pendencias.Add("CONTRATADA não encontrada para o tenant atual.");
            return;
        }

        if (string.IsNullOrWhiteSpace(parceiro.RazaoSocial))
            pendencias.Add("Razão social da CONTRATADA não informada.");
        if (DocumentoFiscal.SomenteDigitos(parceiro.Cnpj)?.Length != 14)
            pendencias.Add("CNPJ da CONTRATADA inválido ou não informado.");
        if (string.IsNullOrWhiteSpace(parceiro.Crc))
            pendencias.Add("CRC da CONTRATADA não informado.");

        AdicionarPendenciaEndereco(
            pendencias,
            "Endereço da CONTRATADA",
            CadastroEndereco.InformacoesFaltantes(parceiro.Endereco));

        var responsavel = parceiro.ResponsavelTecnico;
        if (responsavel is null)
        {
            pendencias.Add("Responsável técnico não cadastrado na contabilidade.");
            return;
        }

        if (responsavel.ParceiroId != parceiro.Id || responsavel.TenantId != tenantId)
            pendencias.Add("Responsável técnico não está vinculado à contabilidade do tenant atual.");
        if (string.IsNullOrWhiteSpace(responsavel.Nome))
            pendencias.Add("Nome do responsável técnico não informado.");
        if (DocumentoFiscal.SomenteDigitos(responsavel.Cpf)?.Length != 11)
            pendencias.Add("CPF do responsável técnico inválido ou não informado.");
        if (string.IsNullOrWhiteSpace(responsavel.Profissao))
            pendencias.Add("Profissão do responsável técnico não informada.");
        if (string.IsNullOrWhiteSpace(responsavel.Crc))
            pendencias.Add("CRC do responsável técnico não informado.");

        AdicionarPendenciaEndereco(
            pendencias,
            "Endereço do responsável técnico",
            CadastroEndereco.InformacoesFaltantes(responsavel.Endereco));
    }

    private static void ValidarContratante(Prospecto prospecto, Empresa? empresa, List<string> pendencias)
    {
        var razaoSocial = empresa?.RazaoSocial ?? prospecto.RazaoSocial;
        if (string.IsNullOrWhiteSpace(razaoSocial))
            pendencias.Add("Razão social da CONTRATANTE não informada.");

        var cnpjProspecto = DocumentoFiscal.SomenteDigitos(prospecto.Cnpj);
        if (cnpjProspecto?.Length != 14)
            pendencias.Add("CNPJ do prospecto não informado ou inválido.");
        if (empresa is not null && DocumentoFiscal.SomenteDigitos(empresa.Cnpj)?.Length != 14)
            pendencias.Add("CNPJ da CONTRATANTE inválido ou não informado.");

        var enderecoFaltante = empresa?.EnderecoId.HasValue == true
            ? CadastroEndereco.InformacoesFaltantes(empresa.Endereco)
            : CadastroEndereco.InformacoesFaltantes(prospecto);
        AdicionarPendenciaEndereco(pendencias, "Endereço da CONTRATANTE", enderecoFaltante);
    }

    private static void ValidarRepresentante(
        Guid? representanteSelecionadoId,
        Socio? representante,
        List<string> pendencias)
    {
        if (representante is null)
        {
            pendencias.Add(representanteSelecionadoId.HasValue
                ? "Representante selecionado não pertence a este prospecto, cliente ou tenant."
                : "Selecione um representante da CONTRATANTE.");
            return;
        }

        var faltantes = CadastroRepresentante.InformacoesFaltantes(representante);
        if (faltantes.Count > 0)
            pendencias.Add($"Cadastro do representante incompleto: {string.Join(", ", faltantes)}.");
    }

    private static void ValidarOrcamento(
        Guid tenantId,
        Prospecto prospecto,
        Orcamento orcamento,
        List<string> pendencias,
        bool permitirSubstituido = false)
    {
        if (orcamento.TenantId != tenantId || orcamento.ProspectId != prospecto.Id)
            pendencias.Add("Orçamento não pertence ao prospecto e tenant atuais.");
        if (orcamento.Status != StatusOrcamento.Aceito
            && !(permitirSubstituido && orcamento.Status == StatusOrcamento.Substituido))
            pendencias.Add("Somente um orçamento aceito pode originar o contrato.");

        ValidarQuantidadeNatureza(
            orcamento,
            NaturezaCobrancaItemOrcamento.HonorarioMensal,
            "Honorário mensal",
            pendencias);
        ValidarQuantidadeNatureza(
            orcamento,
            NaturezaCobrancaItemOrcamento.PorFolha,
            "preço por folha",
            pendencias);

        if (orcamento.DiaVencimento is < 1 or > 31)
            pendencias.Add("Dia de vencimento dos honorários deve estar entre 1 e 31.");
    }

    private static void ValidarQuantidadeNatureza(
        Orcamento orcamento,
        NaturezaCobrancaItemOrcamento natureza,
        string descricao,
        List<string> pendencias)
    {
        var quantidade = orcamento.Itens.Count(item => item.NaturezaCobranca == natureza);
        if (quantidade == 0)
            pendencias.Add($"Orçamento sem item de {descricao}.");
        else if (quantidade > 1)
            pendencias.Add($"Orçamento possui mais de um item de {descricao}.");
    }

    private static void ValidarVigencia(
        DateTime dataInicio,
        int duracaoAnos,
        DateOnly? competenciaInicial,
        List<string> pendencias)
    {
        if (dataInicio == default)
            pendencias.Add("Data de início da vigência não informada.");

        var duracaoValida = duracaoAnos is >= 1 and <= 100;
        if (!duracaoValida)
            pendencias.Add("Duração do contrato deve estar entre 1 e 100 anos.");

        if (!competenciaInicial.HasValue)
            pendencias.Add("Competência inicial dos honorários não informada.");
        else if (competenciaInicial.Value.Day != 1)
            pendencias.Add("Competência inicial dos honorários deve representar o primeiro dia do mês.");

        if (dataInicio == default || !duracaoValida)
            return;

        try
        {
            _ = dataInicio.AddYears(duracaoAnos);
        }
        catch (ArgumentOutOfRangeException)
        {
            pendencias.Add("A data final calculada pela vigência está fora do intervalo permitido.");
        }
    }

    private static void AdicionarPendenciaEndereco(
        List<string> pendencias,
        string descricao,
        IReadOnlyList<string> faltantes)
    {
        if (faltantes.Count > 0)
            pendencias.Add($"{descricao} incompleto: {string.Join(", ", faltantes)}.");
    }
}
