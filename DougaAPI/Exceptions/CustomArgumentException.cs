using JetBrains.Annotations;

namespace DougaAPI.Exceptions;

[UsedImplicitly]
public sealed class CustomArgumentException : ArgumentException
{
    public CustomArgumentException(string message) : base(message) { }
}