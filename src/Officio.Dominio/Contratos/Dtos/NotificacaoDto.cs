namespace Officio.Dominio.Contratos.Dtos;

public record NotificacaoDto(
    Guid Id,
    string Tipo,
    string Titulo,
    string? Mensagem,
    Guid? ReferenciaId,
    bool Lida,
    DateTime? LidaEm,
    DateTime CriadoEm);
