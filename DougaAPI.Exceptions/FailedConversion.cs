namespace DougaAPI.Exceptions;

public sealed class FailedConversion : Exception
{
    public FailedConversion(string message) : base(message) { }
}
