using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Documentos.Queries.Documentos
{
    public record DocumentosQuery() : IQuery<List<DocumentosDto>>;
}
