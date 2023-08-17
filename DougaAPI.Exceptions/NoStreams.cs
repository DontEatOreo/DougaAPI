namespace DougaAPI.Exceptions;

public sealed class NoStreams : Exception
{
    public NoStreams(string message) : base(message) { }
}
