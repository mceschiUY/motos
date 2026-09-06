using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Auto-provisión de base de datos en desarrollo. ARCHIVO GENERADO por Forja
/// (AgentesDesarrollo) — NO forma parte del template read-only.
///
/// Al arrancar el API garantiza que exista la base y todas las tablas de TODOS los
/// DbContext del proyecto (infra + dominio), creando desde el modelo EF lo que falte.
/// Idempotente: los objetos ya existentes se ignoran. Cubre la generación inicial y
/// también las entidades nuevas que agregue codepulse-dev (basta reiniciar el API).
/// </summary>
public static class DbBootstrap
{
    public static void EnsureDatabaseProvisioned(this WebApplication app)
    {
        // Solo en desarrollo: en producción el esquema se gestiona por otro medio.
        if (!app.Environment.IsDevelopment()) return;

        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DbBootstrap");

        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        // Descubrir todos los DbContext del ensamblado y resolverlos desde DI.
        var contexts = new List<DbContext>();
        foreach (var t in typeof(DbBootstrap).Assembly.GetTypes())
        {
            if (!typeof(DbContext).IsAssignableFrom(t) || t.IsAbstract || t == typeof(DbContext))
                continue;
            if (sp.GetService(t) is DbContext ctx)
                contexts.Add(ctx);
        }

        if (contexts.Count == 0)
        {
            logger.LogWarning("DbBootstrap: no se encontraron DbContext registrados.");
            return;
        }

        // 1) Garantizar la base (vacía) desde cualquier contexto.
        try
        {
            var creator = contexts[0].GetService<IRelationalDatabaseCreator>();
            if (!creator.Exists())
            {
                creator.Create();
                logger.LogInformation("DbBootstrap: base de datos creada.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DbBootstrap: no se pudo garantizar la base de datos.");
            return;
        }

        // 2) Crear tablas faltantes desde el modelo EF de cada contexto (idempotente).
        //    Cada contexto de dominio incluye también las tablas de infra (heredadas del
        //    Context base): el primero las crea, los demás reintentan y se ignora "ya existe".
        var contextsOk = 0;
        foreach (var ctx in contexts)
        {
            try
            {
                var script = ctx.Database.GenerateCreateScript();
                foreach (var batch in SplitBatches(script))
                {
                    try
                    {
                        ctx.Database.ExecuteSqlRaw(batch);
                    }
                    catch (SqlException ex) when (IsAlreadyExists(ex))
                    {
                        // El objeto (tabla/índice/constraint) ya existe — idempotente.
                    }
                }
                contextsOk++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "DbBootstrap: error provisionando {Context}", ctx.GetType().Name);
            }
        }

        logger.LogInformation("DbBootstrap: esquema verificado para {Count} contextos.", contextsOk);

        // 2b) SYNC DE COLUMNAS (conservador): si la tabla ya existía pero le faltan columnas
        //     del modelo (spec que evolucionó entre corridas, o tabla heredada), se agregan
        //     con ALTER TABLE ADD — NUNCA se dropea ni se pierde data. Si una columna existe
        //     con otro tipo, solo se avisa por log (resolución manual). Sin este paso, el
        //     "ya existe" del paso 2 dejaba tablas con esquema viejo en silencio.
        try
        {
            foreach (var ctx in contexts)
            {
                foreach (var et in ctx.Model.GetEntityTypes())
                {
                    var table = et.GetTableName();
                    if (string.IsNullOrEmpty(table)) continue;
                    var soi = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(table, et.GetSchema());

                    var dbCols = ctx.Database.SqlQueryRaw<string>(
                        "SELECT COLUMN_NAME + '|' + DATA_TYPE AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = {0}", table)
                        .ToList();
                    if (dbCols.Count == 0) continue; // la tabla no existe: la crea el paso 2

                    var existentes = dbCols
                        .Select(s => s.Split('|'))
                        .ToDictionary(p => p[0], p => p.Length > 1 ? p[1] : "", StringComparer.OrdinalIgnoreCase);

                    foreach (var prop in et.GetProperties())
                    {
                        var col = prop.GetColumnName(soi);
                        if (col == null) continue;
                        var tipo = prop.GetColumnType(soi) ?? "nvarchar(max)";

                        if (existentes.TryGetValue(col, out var tipoActual))
                        {
                            // Existe: si el tipo base difiere, avisar (no tocar).
                            if (!string.IsNullOrEmpty(tipoActual) &&
                                !tipo.StartsWith(tipoActual, StringComparison.OrdinalIgnoreCase))
                                logger.LogWarning("DbBootstrap: {Table}.{Col} es {Actual} en la base pero el modelo espera {Esperado} — revisar a mano.",
                                    table, col, tipoActual, tipo);
                            continue;
                        }

                        // Falta: agregarla. NOT NULL sobre tabla con filas necesita DEFAULT.
                        var def = tipo.StartsWith("nvarchar", StringComparison.OrdinalIgnoreCase) || tipo.StartsWith("varchar", StringComparison.OrdinalIgnoreCase) ? "N''"
                            : tipo.StartsWith("datetime", StringComparison.OrdinalIgnoreCase) ? "GETUTCDATE()"
                            : "0";
                        var sql = prop.IsNullable
                            ? $"ALTER TABLE [{table}] ADD [{col}] {tipo} NULL"
                            : $"ALTER TABLE [{table}] ADD [{col}] {tipo} NOT NULL CONSTRAINT [DF_{table}_{col}] DEFAULT {def}";
                        try
                        {
                            ctx.Database.ExecuteSqlRaw(sql);
                            logger.LogInformation("DbBootstrap: columna agregada {Table}.{Col} ({Tipo})", table, col, tipo);
                        }
                        catch (SqlException ex)
                        {
                            logger.LogWarning("DbBootstrap: no se pudo agregar {Table}.{Col}: {Msg}", table, col, ex.Message);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "DbBootstrap: sync de columnas no completado.");
        }

        // 3) FOREIGN KEYs entre agregados: cada entidad vive en su propio DbContext, así que
        //    EF no conoce las relaciones cruzadas. Forja las emite como Scripts/FK_*.sql
        //    idempotentes que se aplican acá, cuando ya existen todas las tablas.
        try
        {
            var scriptsDir = System.IO.Path.Combine(app.Environment.ContentRootPath, "Scripts");
            if (System.IO.Directory.Exists(scriptsDir))
            {
                var fkOk = 0;
                foreach (var file in System.IO.Directory.GetFiles(scriptsDir, "FK_*.sql").OrderBy(f => f))
                {
                    foreach (var batch in SplitBatches(System.IO.File.ReadAllText(file)))
                    {
                        try { contexts[0].Database.ExecuteSqlRaw(batch); fkOk++; }
                        catch (SqlException ex)
                        {
                            // p.ej. 547: datos existentes violan la FK — se avisa, no rompe el arranque.
                            logger.LogWarning("DbBootstrap: FK no aplicada ({File}): {Msg}",
                                System.IO.Path.GetFileName(file), ex.Message);
                        }
                    }
                }
                if (fkOk > 0)
                    logger.LogInformation("DbBootstrap: {Count} foreign keys verificadas.", fkOk);

                // 4) SEEDS del core (Cfg_*.sql): configuración del sitio y hermanos. Son
                //    idempotentes (IF NOT EXISTS en tabla y datos) pero NADIE los ejecutaba:
                //    todo sitio nacía con /configuracion vacía (bug dbcc33, 2026-09-05).
                var cfgOk = 0;
                foreach (var file in System.IO.Directory.GetFiles(scriptsDir, "Cfg_*.sql").OrderBy(f => f))
                {
                    foreach (var batch in SplitBatches(System.IO.File.ReadAllText(file)))
                    {
                        try { contexts[0].Database.ExecuteSqlRaw(batch); cfgOk++; }
                        catch (SqlException ex)
                        {
                            logger.LogWarning("DbBootstrap: seed no aplicado ({File}): {Msg}",
                                System.IO.Path.GetFileName(file), ex.Message);
                        }
                    }
                }
                if (cfgOk > 0)
                    logger.LogInformation("DbBootstrap: {Count} lotes de seed de configuración aplicados.", cfgOk);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "DbBootstrap: no se pudieron aplicar las foreign keys.");
        }
    }

    private static IEnumerable<string> SplitBatches(string script)
    {
        return Regex
            .Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Select(b => b.Trim())
            .Where(b => b.Length > 0);
    }

    private static bool IsAlreadyExists(SqlException ex)
    {
        foreach (SqlError e in ex.Errors)
        {
            // 2714: ya existe un objeto con ese nombre · 1913: el índice ya existe.
            if (e.Number == 2714 || e.Number == 1913) return true;
        }
        return false;
    }
}
