using MediatR;

namespace ApiMotos.Application.Artesanal.Comercial.ClientesSinVisitar
{
    /// <summary>Clientes con vendedor asignado y sin actividad en los últimos `Dias` (default 30).</summary>
    public record ClientesSinVisitarQuery(int Dias = 30) : IRequest<List<ClienteSinVisitarDto>>;
}
