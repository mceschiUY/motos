using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Documentos.Queries.Documentos;

namespace ApiMotos.Application.Agregates.Documentos.Queries.ById
{
    public record DocumentoByIdQuery(int Id) : IQuery<DocumentoContenidoDto?>;
}
