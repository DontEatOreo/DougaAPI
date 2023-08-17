using Xabe.FFmpeg;

namespace DougaAPI.Interfaces;

public interface IConverter<in T>
{
    public IConversion BuildConversion(IEnumerable<IStream> streams, T query, string output);
}
