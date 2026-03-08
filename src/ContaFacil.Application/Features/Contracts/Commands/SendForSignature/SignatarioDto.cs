namespace ContaFacil.Application.Features.Contracts.Commands.SendForSignature;

public record SignatarioDto(string Nome, string Email, string? Cpf, string Tipo);
