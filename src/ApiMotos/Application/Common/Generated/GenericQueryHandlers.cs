using MediatR;
using ApiMotos.Application.Common.Abstractions;

namespace ApiMotos.Application.Common.Generated
{
    /// <summary>
    /// Handlers de lectura genéricos. Los shells generados por entidad aportan SOLO el SQL
    /// (const, emitido por el generador — nunca armado en runtime) y los accessors del
    /// query. La mecánica vive acá una sola vez, y la EJECUCIÓN va por el puerto
    /// IQueryService (Refactorización Ola 2): Application ya no abre SqlConnection.
    /// </summary>
    public abstract class GenericListaHandler<TQuery, TDto> : IRequestHandler<TQuery, List<TDto>>
        where TQuery : IRequest<List<TDto>>
    {
        private readonly IQueryService _consultas;
        private readonly string _sql;
        private readonly Func<TQuery, (int? Id, int? Skip, int? Take)> _args;

        protected GenericListaHandler(IQueryService consultas, string sql,
            Func<TQuery, (int? Id, int? Skip, int? Take)> args)
        {
            _consultas = consultas;
            _sql = sql;
            _args = args;
        }

        public async Task<List<TDto>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            var (id, skip, takeExplicito) = _args(query);
            var sql = _sql;

            // Filtro por Id en SQL (GetById O(1)) y paginación con tope por defecto:
            // sin take explícito se limita a 1000 filas — un getAll sin límite baja la
            // tabla entera a memoria (riesgo real con datos de producción).
            var take = takeExplicito ?? 1000;
            if (id is not null)
                sql += "\nWHERE e.Id = @Id";
            else
                sql += "\nORDER BY e.Id OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            return await _consultas.ConsultarAsync<TDto>(sql,
                new { Id = id, Skip = skip ?? 0, Take = take });
        }
    }

    public abstract class GenericPorFkHandler<TQuery, TDto> : IRequestHandler<TQuery, List<TDto>>
        where TQuery : IRequest<List<TDto>>
    {
        private readonly IQueryService _consultas;
        private readonly string _sql;
        private readonly Func<TQuery, object> _parametros;

        protected GenericPorFkHandler(IQueryService consultas, string sql,
            Func<TQuery, object> parametros)
        {
            _consultas = consultas;
            _sql = sql;
            _parametros = parametros;
        }

        public Task<List<TDto>> Handle(TQuery query, CancellationToken cancellationToken)
            => _consultas.ConsultarAsync<TDto>(_sql, _parametros(query));
    }

    public abstract class GenericBuscarHandler<TQuery, TDto> : IRequestHandler<TQuery, List<TDto>>
        where TQuery : IRequest<List<TDto>>
    {
        private readonly IQueryService _consultas;
        private readonly string _sql;
        private readonly Func<TQuery, string> _texto;

        protected GenericBuscarHandler(IQueryService consultas, string sql,
            Func<TQuery, string> texto)
        {
            _consultas = consultas;
            _sql = sql;
            _texto = texto;
        }

        public async Task<List<TDto>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            var texto = _texto(query);
            if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < 2)
                return new List<TDto>();

            return await _consultas.ConsultarAsync<TDto>(_sql, new { q = "%" + texto.Trim() + "%" });
        }
    }

    public abstract class GenericResumenHandler<TQuery, TDto, TGrupo> : IRequestHandler<TQuery, TDto>
        where TQuery : IRequest<TDto>
    {
        private readonly IQueryService _consultas;
        private readonly string _sqlTotal;
        private readonly string? _sqlPorEstado;
        private readonly string? _sqlPorMes;
        private readonly Func<int, List<TGrupo>, List<TGrupo>, TDto> _armar;

        protected GenericResumenHandler(IQueryService consultas, string sqlTotal,
            string? sqlPorEstado, string? sqlPorMes,
            Func<int, List<TGrupo>, List<TGrupo>, TDto> armar)
        {
            _consultas = consultas;
            _sqlTotal = sqlTotal;
            _sqlPorEstado = sqlPorEstado;
            _sqlPorMes = sqlPorMes;
            _armar = armar;
        }

        public async Task<TDto> Handle(TQuery query, CancellationToken cancellationToken)
        {
            var total = await _consultas.EscalarAsync(_sqlTotal);

            var porEstado = _sqlPorEstado != null
                ? await _consultas.ConsultarAsync<TGrupo>(_sqlPorEstado)
                : new List<TGrupo>();

            var porMes = _sqlPorMes != null
                ? await _consultas.ConsultarAsync<TGrupo>(_sqlPorMes)
                : new List<TGrupo>();

            return _armar(total, porEstado, porMes);
        }
    }
}
