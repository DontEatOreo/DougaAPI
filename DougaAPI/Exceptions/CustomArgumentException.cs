using JetBrains.Annotations;

namespace DougaAPI.Exceptions;

[UsedImplicitly]
public class CustomArgumentException : ArgumentException
{
    public CustomArgumentException(string message) : base(message) { }
}