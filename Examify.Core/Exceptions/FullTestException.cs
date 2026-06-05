// Tạo custom exception
// Examify.Core/Exceptions/FullTestException.cs
namespace Examify.Core.Exceptions;

public class FullTestException : Exception
{
    public FullTestException() : base() { }
    public FullTestException(string message) : base(message) { }
    public FullTestException(string message, Exception innerException) : base(message, innerException) { }
}