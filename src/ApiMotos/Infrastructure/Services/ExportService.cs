using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace ApiMotos.Infrastructure.Services;

/// <summary>
/// Servicio genérico de exportación de datos a diferentes formatos
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exporta datos a formato Excel (.xlsx)
    /// </summary>
    Task<byte[]> ExportToExcelAsync<T>(
        IEnumerable<T> data,
        ExportOptions options) where T : class;

    /// <summary>
    /// Exporta datos a formato CSV
    /// </summary>
    Task<byte[]> ExportToCsvAsync<T>(
        IEnumerable<T> data,
        ExportOptions options) where T : class;

    /// <summary>
    /// Exporta datos dinámicos (diccionarios) a Excel
    /// </summary>
    Task<byte[]> ExportDynamicToExcelAsync(
        IEnumerable<Dictionary<string, object?>> data,
        ExportOptions options);

    /// <summary>
    /// Exporta datos dinámicos a CSV
    /// </summary>
    Task<byte[]> ExportDynamicToCsvAsync(
        IEnumerable<Dictionary<string, object?>> data,
        ExportOptions options);
}

/// <summary>
/// Opciones de configuración para exportación
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Nombre del archivo (sin extensión)
    /// </summary>
    public string FileName { get; set; } = "export";

    /// <summary>
    /// Nombre de la hoja en Excel
    /// </summary>
    public string SheetName { get; set; } = "Datos";

    /// <summary>
    /// Título del reporte (se muestra en la primera fila)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Columnas a incluir (null = todas)
    /// </summary>
    public List<string>? Columns { get; set; }

    /// <summary>
    /// Mapeo de nombres de columna para mostrar
    /// </summary>
    public Dictionary<string, string>? ColumnHeaders { get; set; }

    /// <summary>
    /// Incluir fecha de generación
    /// </summary>
    public bool IncludeTimestamp { get; set; } = true;

    /// <summary>
    /// Formato de fecha para el timestamp
    /// </summary>
    public string TimestampFormat { get; set; } = "dd/MM/yyyy HH:mm";

    /// <summary>
    /// Aplicar formato de tabla en Excel
    /// </summary>
    public bool ApplyTableStyle { get; set; } = true;

    /// <summary>
    /// Autoajustar ancho de columnas
    /// </summary>
    public bool AutoFitColumns { get; set; } = true;
}

/// <summary>
/// Implementación del servicio de exportación
/// </summary>
public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportToExcelAsync<T>(
        IEnumerable<T> data,
        ExportOptions options) where T : class
    {
        return await Task.Run(() =>
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(options.SheetName);

                var dataList = data.ToList();
                var startRow = 1;

                // Título opcional
                if (!string.IsNullOrEmpty(options.Title))
                {
                    worksheet.Cell(startRow, 1).Value = options.Title;
                    worksheet.Cell(startRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(startRow, 1).Style.Font.FontSize = 14;
                    startRow++;
                }

                // Timestamp opcional
                if (options.IncludeTimestamp)
                {
                    worksheet.Cell(startRow, 1).Value = $"Generado: {DateTime.Now.ToString(options.TimestampFormat)}";
                    worksheet.Cell(startRow, 1).Style.Font.Italic = true;
                    worksheet.Cell(startRow, 1).Style.Font.FontColor = XLColor.Gray;
                    startRow++;
                }

                if (startRow > 1) startRow++; // Línea en blanco

                // Obtener propiedades
                var properties = typeof(T).GetProperties()
                    .Where(p => options.Columns == null || options.Columns.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                // Headers
                for (int i = 0; i < properties.Count; i++)
                {
                    var propName = properties[i].Name;
                    var headerName = options.ColumnHeaders?.GetValueOrDefault(propName) ?? FormatHeaderName(propName);
                    worksheet.Cell(startRow, i + 1).Value = headerName;
                    worksheet.Cell(startRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(startRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Datos
                var dataRow = startRow + 1;
                foreach (var item in dataList)
                {
                    for (int i = 0; i < properties.Count; i++)
                    {
                        var value = properties[i].GetValue(item);
                        SetCellValue(worksheet.Cell(dataRow, i + 1), value);
                    }
                    dataRow++;
                }

                // Aplicar estilo de tabla
                if (options.ApplyTableStyle && dataList.Count > 0)
                {
                    var range = worksheet.Range(startRow, 1, dataRow - 1, properties.Count);
                    var table = range.CreateTable();
                    table.Theme = XLTableTheme.TableStyleMedium2;
                }

                // Autoajustar columnas
                if (options.AutoFitColumns)
                {
                    worksheet.Columns().AdjustToContents();
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);

                _logger.LogInformation(
                    "[Export] Excel generado: {FileName}, {Rows} filas, {Cols} columnas",
                    options.FileName, dataList.Count, properties.Count);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Export] Error generando Excel");
                throw;
            }
        });
    }

    public async Task<byte[]> ExportToCsvAsync<T>(
        IEnumerable<T> data,
        ExportOptions options) where T : class
    {
        return await Task.Run(() =>
        {
            try
            {
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream, Encoding.UTF8);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ","
                });

                var dataList = data.ToList();

                // Configurar mapeo de columnas si se especificaron
                if (options.Columns != null && options.Columns.Count > 0)
                {
                    var properties = typeof(T).GetProperties()
                        .Where(p => options.Columns.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                        .ToList();

                    // Escribir headers personalizados
                    foreach (var prop in properties)
                    {
                        var headerName = options.ColumnHeaders?.GetValueOrDefault(prop.Name) ?? prop.Name;
                        csv.WriteField(headerName);
                    }
                    csv.NextRecord();

                    // Escribir datos
                    foreach (var item in dataList)
                    {
                        foreach (var prop in properties)
                        {
                            var value = prop.GetValue(item);
                            csv.WriteField(FormatValue(value));
                        }
                        csv.NextRecord();
                    }
                }
                else
                {
                    csv.WriteRecords(dataList);
                }

                writer.Flush();

                _logger.LogInformation(
                    "[Export] CSV generado: {FileName}, {Rows} filas",
                    options.FileName, dataList.Count);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Export] Error generando CSV");
                throw;
            }
        });
    }

    public async Task<byte[]> ExportDynamicToExcelAsync(
        IEnumerable<Dictionary<string, object?>> data,
        ExportOptions options)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(options.SheetName);

                var dataList = data.ToList();
                if (dataList.Count == 0)
                {
                    worksheet.Cell(1, 1).Value = "No hay datos para exportar";
                    using var emptyStream = new MemoryStream();
                    workbook.SaveAs(emptyStream);
                    return emptyStream.ToArray();
                }

                var startRow = 1;

                // Título opcional
                if (!string.IsNullOrEmpty(options.Title))
                {
                    worksheet.Cell(startRow, 1).Value = options.Title;
                    worksheet.Cell(startRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(startRow, 1).Style.Font.FontSize = 14;
                    startRow++;
                }

                // Timestamp opcional
                if (options.IncludeTimestamp)
                {
                    worksheet.Cell(startRow, 1).Value = $"Generado: {DateTime.Now.ToString(options.TimestampFormat)}";
                    worksheet.Cell(startRow, 1).Style.Font.Italic = true;
                    startRow++;
                }

                if (startRow > 1) startRow++;

                // Obtener columnas del primer registro
                var columns = options.Columns ?? dataList.First().Keys.ToList();

                // Headers
                for (int i = 0; i < columns.Count; i++)
                {
                    var colName = columns[i];
                    var headerName = options.ColumnHeaders?.GetValueOrDefault(colName) ?? FormatHeaderName(colName);
                    worksheet.Cell(startRow, i + 1).Value = headerName;
                    worksheet.Cell(startRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(startRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Datos
                var dataRow = startRow + 1;
                foreach (var item in dataList)
                {
                    for (int i = 0; i < columns.Count; i++)
                    {
                        var colName = columns[i];
                        item.TryGetValue(colName, out var value);
                        SetCellValue(worksheet.Cell(dataRow, i + 1), value);
                    }
                    dataRow++;
                }

                // Aplicar estilo de tabla
                if (options.ApplyTableStyle)
                {
                    var range = worksheet.Range(startRow, 1, dataRow - 1, columns.Count);
                    var table = range.CreateTable();
                    table.Theme = XLTableTheme.TableStyleMedium2;
                }

                if (options.AutoFitColumns)
                {
                    worksheet.Columns().AdjustToContents();
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);

                _logger.LogInformation(
                    "[Export] Excel dinámico generado: {FileName}, {Rows} filas",
                    options.FileName, dataList.Count);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Export] Error generando Excel dinámico");
                throw;
            }
        });
    }

    public async Task<byte[]> ExportDynamicToCsvAsync(
        IEnumerable<Dictionary<string, object?>> data,
        ExportOptions options)
    {
        return await Task.Run(() =>
        {
            try
            {
                var dataList = data.ToList();
                if (dataList.Count == 0)
                {
                    return Encoding.UTF8.GetBytes("No hay datos para exportar");
                }

                var sb = new StringBuilder();
                var columns = options.Columns ?? dataList.First().Keys.ToList();

                // Headers
                var headers = columns.Select(c =>
                    options.ColumnHeaders?.GetValueOrDefault(c) ?? c);
                sb.AppendLine(string.Join(",", headers.Select(EscapeCsvField)));

                // Datos
                foreach (var item in dataList)
                {
                    var values = columns.Select(c =>
                    {
                        item.TryGetValue(c, out var value);
                        return EscapeCsvField(FormatValue(value));
                    });
                    sb.AppendLine(string.Join(",", values));
                }

                _logger.LogInformation(
                    "[Export] CSV dinámico generado: {FileName}, {Rows} filas",
                    options.FileName, dataList.Count);

                return Encoding.UTF8.GetBytes(sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Export] Error generando CSV dinámico");
                throw;
            }
        });
    }

    private void SetCellValue(IXLCell cell, object? value)
    {
        if (value == null)
        {
            cell.Value = "";
            return;
        }

        switch (value)
        {
            case DateTime dt:
                cell.Value = dt;
                cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                break;
            case DateOnly d:
                cell.Value = d.ToDateTime(TimeOnly.MinValue);
                cell.Style.DateFormat.Format = "dd/MM/yyyy";
                break;
            case bool b:
                cell.Value = b ? "Sí" : "No";
                break;
            case decimal dec:
                cell.Value = dec;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case double dbl:
                cell.Value = dbl;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case int i:
                cell.Value = i;
                break;
            case long l:
                cell.Value = l;
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private string FormatValue(object? value)
    {
        if (value == null) return "";

        return value switch
        {
            DateTime dt => dt.ToString("dd/MM/yyyy HH:mm"),
            DateOnly d => d.ToString("dd/MM/yyyy"),
            bool b => b ? "Sí" : "No",
            decimal dec => dec.ToString("N2"),
            double dbl => dbl.ToString("N2"),
            _ => value.ToString() ?? ""
        };
    }

    private string FormatHeaderName(string propertyName)
    {
        // Convierte "nombreCompleto" o "NombreCompleto" a "Nombre Completo"
        var result = new StringBuilder();
        foreach (var c in propertyName)
        {
            if (char.IsUpper(c) && result.Length > 0)
            {
                result.Append(' ');
            }
            result.Append(result.Length == 0 ? char.ToUpper(c) : c);
        }
        return result.ToString();
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
