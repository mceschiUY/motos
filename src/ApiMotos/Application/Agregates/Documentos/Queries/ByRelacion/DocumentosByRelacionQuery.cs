using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Documentos.Queries.Documentos;

namespace ApiMotos.Application.Agregates.Documentos.Queries.ByRelacion
{
    public record DocumentosByRelacionQuery(int RelacionId, string RelacionNombre) : IQuery<List<DocumentosDto>>;
}
