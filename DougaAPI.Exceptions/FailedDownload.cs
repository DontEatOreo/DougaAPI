namespace DougaAPI.Exceptions;

public sealed class FailedDownload : Exception
{
    public FailedDownload(string message) : base(message) { }
}
