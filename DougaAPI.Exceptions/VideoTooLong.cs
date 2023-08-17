namespace DougaAPI.Exceptions;

public sealed class VideoTooLong : Exception
{
    public VideoTooLong(string message) : base(message) { }
}
