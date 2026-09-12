using ApiMotos.Application.Common;

namespace ApiMotos.Application.Artesanal.Comercial.Cliente360
{
    /// <summary>Cliente 360: identidad, salud de la relación, línea de tiempo y top productos. Null = no existe.</summary>
    public record Cliente360Query(int ClienteId) : IQuery<Cliente360Dto?>;
}
