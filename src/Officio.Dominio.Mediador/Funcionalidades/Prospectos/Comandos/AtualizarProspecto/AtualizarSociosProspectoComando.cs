using MediatR;
using FluentValidation;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Validadores;
namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.AtualizarProspecto;
public record AtualizarSociosProspectoComando(Guid Id, CriarProspectoRequisicao Dados) : IRequest<Resultado<bool>>;
public class AtualizarSociosProspectoHandler(IProspectoServico servico) : IRequestHandler<AtualizarSociosProspectoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(AtualizarSociosProspectoComando request, CancellationToken ct)
        => servico.AtualizarSociosAsync(request.Id, request.Dados, ct);
}
public class AtualizarSociosProspectoValidador : AbstractValidator<AtualizarSociosProspectoComando>
{
    public AtualizarSociosProspectoValidador()
    {
        RuleForEach(x => x.Dados.Socios).SetValidator(new SocioRequisicaoValidador());
        RuleFor(x => x.Dados.Socios).Must(s => s is null || s.Sum(x => x.Participacao) <= 100m)
            .WithMessage("A participacao total dos socios nao pode ultrapassar 100%.");
    }
}
