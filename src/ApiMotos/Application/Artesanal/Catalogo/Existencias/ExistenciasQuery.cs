using ApiMotos.Application.Common;

namespace ApiMotos.Application.Artesanal.Catalogo.Existencias
{
    /// <summary>
    /// Existencias agrupables (plan Etapa D, escena "¿qué se está acabando?", 2026-09-12): una
    /// fila por SKU con producto, marca, categoría (y su raíz para agrupar), saldo por depósito,
    /// comprometido, ventas de 30 días, cobertura y semáforo. DepositoId null = todos.
    /// </summary>
    public record ExistenciasQuery(int? DepositoId = null) : IQuery<ExistenciasDto>;
}
