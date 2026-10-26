using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class ResponsavelTecnico : EntidadeAuditavel
{
    public Guid ParceiroId { get; private set; }
    public string Nome { get; private set; } = default!;
    public string Cpf { get; private set; } = default!;
    public string Profissao { get; private set; } = default!;
    public string Crc { get; private set; } = default!;
    public Guid? EnderecoId { get; private set; }
    public Parceiro Parceiro { get; private set; } = default!;
    public Endereco? Endereco { get; private set; }

    private ResponsavelTecnico() { }

    public static ResponsavelTecnico Criar(Guid parceiroId, string nome, string cpf, string profissao, string crc)
    {
        var responsavel = new ResponsavelTecnico { TenantId = parceiroId, ParceiroId = parceiroId };
        responsavel.Atualizar(nome, cpf, profissao, crc);
        return responsavel;
    }

    public void Atualizar(string nome, string cpf, string profissao, string crc)
    {
        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(profissao) || string.IsNullOrWhiteSpace(crc))
            throw new ArgumentException("Informe nome, profissão e CRC do responsável técnico.");
        var documento = DocumentoFiscal.SomenteDigitos(cpf);
        if (documento?.Length != 11) throw new ArgumentException("CPF deve conter 11 dígitos.");
        Nome = nome.Trim();
        Cpf = documento;
        Profissao = profissao.Trim();
        Crc = crc.Trim();
    }

    public void VincularEndereco(Guid? enderecoId) => EnderecoId = enderecoId;
}
