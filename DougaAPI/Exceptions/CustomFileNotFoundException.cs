using JetBrains.Annotations;

namespace DougaAPI.Exceptions;

[UsedImplicitly]
public sealed class CustomFileNotFoundException : FileNotFoundException
{
    public CustomFileNotFoundException(string message) : base(message) { }
}