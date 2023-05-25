namespace DougaAPI.Exceptions;

public sealed class CustomInvalidOperationException : InvalidOperationException
{
    public CustomInvalidOperationException(string message) : base(message) { }
}