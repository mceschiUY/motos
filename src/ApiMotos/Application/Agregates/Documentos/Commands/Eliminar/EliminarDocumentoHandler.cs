using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Domain.Agregates.Documentos;

namespace ApiMotos.Application.Agregates.Documentos.Commands.Eliminar
{
    public class EliminarDocumentoHandler : ICommandHandler<EliminarDocumentoCommand, Result<int>>
    {
        private readonly IDocumentoRepositorio _documentoRepositorio;

        public EliminarDocumentoHandler(IDocumentoRepositorio documentoRepositorio)
        {
            _documentoRepositorio = documentoRepositorio;
        }

        public async Task<Result<int>> Handle(EliminarDocumentoCommand command, CancellationToken cancellationToken)
        {
            var documento = await _documentoRepositorio.FindAsync(command.Id);

            if (documento == null)
                return Result.Fail<int>("Documento no encontrado");

            _documentoRepositorio.Delete(documento);

            return Result.Ok(command.Id);
        }
    }
}
