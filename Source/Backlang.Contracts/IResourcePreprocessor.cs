namespace Backlang.Contracts;

public interface IResourcePreprocessor
{
    public string Extension { get; }

    Stream Preprocess(Stream strm);
}