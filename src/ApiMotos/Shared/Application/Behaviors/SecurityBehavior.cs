// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Pipeline de Autorización MediatR
// Se ejecuta ANTES de FluentValidation para fail-fast en seguridad
// ═══════════════════════════════════════════════════════════════════════════════

using System.Reflection;
using MediatR;
using ApiMotos.Shared.Abstractions;
using ApiMotos.Shared.Exceptions;
using ApiMotos.Shared.Infrastructure;

namespace ApiMotos.Shared.Application.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior para autorización basada en Capabilities.
/// Verifica atributos [ZasAuthorize] en Commands/Queries antes de ejecutar.
/// </summary>
/// <typeparam name="TRequest">Tipo del request (Command/Query)</typeparam>
/// <typeparam name="TResponse">Tipo de respuesta</typeparam>
public class SecurityBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SecurityBehavior<TRequest, TResponse>> _logger;

    public SecurityBehavior(
        ICurrentUserService currentUser,
        ILogger<SecurityBehavior<TRequest, TResponse>> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);
        var requestName = requestType.Name;

        // 1. Verificar [ZasAllowAnonymous] - permite acceso sin autenticación
        if (requestType.GetCustomAttribute<ZasAllowAnonymousAttribute>() != null)
        {
            _logger.LogDebug("[Security] {Request} permite acceso anónimo", requestName);
            return await next();
        }

        // 2. Obtener atributos [ZasAuthorize]
        var authorizeAttributes = requestType
            .GetCustomAttributes<ZasAuthorizeAttribute>()
            .ToList();

        // 3. Si no hay atributos de autorización, permitir (no requiere autorización)
        if (authorizeAttributes.Count == 0)
        {
            return await next();
        }

        // 4. Verificar autenticación
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning(
                "[Security] Usuario no autenticado intentó acceder a {Request}. " +
                "Capabilities requeridas: [{Capabilities}]",
                requestName,
                string.Join(", ", authorizeAttributes.Select(a => a.Capability)));

            throw SecurityException.NotAuthenticated(authorizeAttributes.First().Capability);
        }

        // 5. Verificar cada capability requerida
        var missingCapabilities = new List<string>();

        foreach (var attr in authorizeAttributes)
        {
            if (!_currentUser.HasCapability(attr.Capability))
            {
                missingCapabilities.Add(attr.Capability);
            }
        }

        // 6. Si faltan capabilities, denegar acceso
        if (missingCapabilities.Count > 0)
        {
            _logger.LogWarning(
                "[Security] Usuario {UserId} ({UserName}) sin permisos para {Request}. " +
                "Capabilities faltantes: [{MissingCapabilities}]",
                _currentUser.UserId,
                _currentUser.UserName,
                requestName,
                string.Join(", ", missingCapabilities));

            throw SecurityException.Forbidden(
                missingCapabilities.First(),
                _currentUser.UserId);
        }

        // 7. Autorizado - continuar al siguiente behavior/handler
        _logger.LogDebug(
            "[Security] Usuario {UserId} ({UserName}) autorizado para {Request}",
            _currentUser.UserId,
            _currentUser.UserName,
            requestName);

        return await next();
    }
}
