using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Domain.Agregates.Documentos;

namespace ApiMotos.Application.Agregates.Documentos.Commands.Cargar
{
    public class CargarDocumentoHandler : ICommandHandler<CargarDocumentoCommand, Result<int>>
    {
        private readonly IDocumentoRepositorio _documentoRepositorio;

        public CargarDocumentoHandler(IDocumentoRepositorio documentoRepositorio)
        {
            _documentoRepositorio = documentoRepositorio;
        }

        public async Task<Result<int>> Handle(CargarDocumentoCommand command, CancellationToken cancellationToken)
        {
            var documentoResult = Documento.Crear(
                command.Nombre,
                command.Extension,
                command.Contenido,
                command.MimeType,
                command.RelacionId,
                command.RelacionNombre
            );

            if (documentoResult.IsFailed)
                return Result.Fail<int>(documentoResult.Errors);

            var documento = documentoResult.Value;
            await _documentoRepositorio.AddAsync(documento);

            return Result.Ok(documento.Id);
        }
    }
}
