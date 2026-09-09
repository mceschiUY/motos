using MediatR;

namespace ApiMotos.Application.Artesanal.Comercial.Agenda
{
    /// <summary>Agenda del vendedor: por defecto hoy .. hoy+7; VendedorId opcional (null = todos).</summary>
    public record AgendaQuery(int? VendedorId = null, DateTime? Desde = null, DateTime? Hasta = null) : IRequest<List<AgendaDto>>;
}
