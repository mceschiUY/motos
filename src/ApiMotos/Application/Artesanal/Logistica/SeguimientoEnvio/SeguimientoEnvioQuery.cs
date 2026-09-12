using ApiMotos.Application.Common;
using ApiMotos.Shared.Infrastructure;

namespace ApiMotos.Application.Artesanal.Logistica.SeguimientoEnvio
{
    /// <summary>
    /// Seguimiento de un envío (escena "Seguimiento del envío", interna y pública).
    /// Se pide por <see cref="EnvioId"/> (uso interno, con login) o por <see cref="CodigoRastreo"/>
    /// (uso público desde el QR; comparación sin distinguir mayúsculas).
    ///
    /// <see cref="Publico"/>=true recorta el DTO: NUNCA salen precios, costos, totales, teléfonos
    /// ni usuarios. Es el modo que consume el endpoint anónimo <c>api/publico/seguimiento/{codigo}</c>.
    ///
    /// [ZasAllowAnonymous]: el SecurityBehavior de MediatR la deja pasar sin usuario autenticado.
    /// La query no tiene [ZasAuthorize] (igual pasaría), pero se declara explícito porque el
    /// endpoint público depende de esto. El recorte de datos lo hace el flag Publico, no la seguridad.
    /// </summary>
    [ZasAllowAnonymous]
    public record SeguimientoEnvioQuery(int? EnvioId, string? CodigoRastreo, bool Publico) : IQuery<SeguimientoEnvioDto?>;
}
