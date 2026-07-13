// Tạo file này
// Examify.Core/Exceptions/BadRequestException.cs
namespace Examify.Core.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException() : base() { }
    public BadRequestException(string message) : base(message) { }
    public BadRequestException(string message, Exception innerException)
        : base(message, innerException) { }
}