namespace DougaAPI.Exceptions;

public class CustomInvalidOperationException : InvalidOperationException
{
    public CustomInvalidOperationException(string message) : base(message) { }
}