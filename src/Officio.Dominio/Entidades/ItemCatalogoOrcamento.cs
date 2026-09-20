using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class ItemCatalogoOrcamento : EntidadeAuditavel
{
    public string Descricao { get; private set; } = default!;
    public SecaoItemOrcamento Secao { get; private set; }
    public TipoItemOrcamento Tipo { get; private set; }
    public NaturezaCobrancaItemOrcamento NaturezaCobranca { get; private set; }
    public decimal? ValorReferencia { get; private set; }
    public bool Ativo { get; private set; } = true;

    private ItemCatalogoOrcamento() { }

    public static ItemCatalogoOrcamento Criar(
        Guid tenantId,
        string descricao,
        SecaoItemOrcamento secao,
        TipoItemOrcamento tipo,
        decimal? valorReferencia,
        NaturezaCobrancaItemOrcamento naturezaCobranca = NaturezaCobrancaItemOrcamento.NaoRecorrente)
    {
        Validar(descricao, tipo, valorReferencia, naturezaCobranca);

        return new ItemCatalogoOrcamento
        {
            TenantId = tenantId,
            Descricao = descricao.Trim(),
            Secao = secao,
            Tipo = tipo,
            NaturezaCobranca = naturezaCobranca,
            ValorReferencia = valorReferencia,
            Ativo = true
        };
    }

    public void Atualizar(
        string descricao,
        SecaoItemOrcamento secao,
        TipoItemOrcamento tipo,
        decimal? valorReferencia,
        NaturezaCobrancaItemOrcamento naturezaCobranca)
    {
        Validar(descricao, tipo, valorReferencia, naturezaCobranca);

        Descricao = descricao.Trim();
        Secao = secao;
        Tipo = tipo;
        NaturezaCobranca = naturezaCobranca;
        ValorReferencia = valorReferencia;
    }

    public void Ativar() => Ativo = true;

    public void Desativar() => Ativo = false;

    private static void Validar(
        string descricao,
        TipoItemOrcamento tipo,
        decimal? valorReferencia,
        NaturezaCobrancaItemOrcamento naturezaCobranca)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descricao do item de catalogo e obrigatoria.", nameof(descricao));

        if (valorReferencia < 0)
            throw new ArgumentException("Valor de referencia nao pode ser negativo.", nameof(valorReferencia));

        if (tipo != TipoItemOrcamento.ItemMonetario && naturezaCobranca != NaturezaCobrancaItemOrcamento.NaoRecorrente)
            throw new ArgumentException("Somente itens monetarios podem compor o faturamento recorrente.", nameof(naturezaCobranca));
    }
}
