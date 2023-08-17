namespace DougaAPI.Exceptions;

public sealed class FailedFetch : Exception
{
    public FailedFetch(string message) : base(message) { }
}
