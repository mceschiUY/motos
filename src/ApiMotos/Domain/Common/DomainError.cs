namespace ApiMotos.Domain.Common
{

    using FluentResults;
    public class DomainError : Error
    {
        public const string ErrorCode = "Error";

        public DomainError(string message, string code)
            : base(message)
        {
            WithMetadata(ErrorCode, code);
        }
    }
}
