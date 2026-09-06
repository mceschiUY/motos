using MediatR;

namespace ApiMotos.Application.Common
{
    public interface ICommand : IRequest{}

    public interface ICommand<out TResponse> : IRequest<TResponse>{}

    public interface IQuery : IRequest{}

    public interface IQueryHandler<in TQuery, TResult> :IRequestHandler<TQuery, TResult> where TQuery : IQuery<TResult>{}

    public interface IQuery<out TResponse> : IRequest<TResponse>{ }

}
