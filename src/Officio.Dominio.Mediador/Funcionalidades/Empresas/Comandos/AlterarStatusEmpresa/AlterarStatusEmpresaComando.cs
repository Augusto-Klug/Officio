using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Empresas.Comandos.AlterarStatusEmpresa;

public record AlterarStatusEmpresaComando(Guid EmpresaId, bool Ativa) : IRequest<Resultado<bool>>;
