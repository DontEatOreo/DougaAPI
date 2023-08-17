namespace DougaAPI.Exceptions;

public sealed class MissingFile : Exception
{
    public MissingFile(string message) : base(message) { }
}
