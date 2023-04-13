using JetBrains.Annotations;

namespace DougaAPI.Exceptions;

[UsedImplicitly]
public class CustomFileNotFoundException : FileNotFoundException
{
    public CustomFileNotFoundException(string message) : base(message) { }
}