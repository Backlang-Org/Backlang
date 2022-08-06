namespace Backlang.Driver.Compiling;

public class EmbeddedResourceAttribute : IAttribute
{
    public EmbeddedResourceAttribute(string name, Stream strm)
    {
        Name = name;
        Strm = strm;
    }

    public string Name { get; set; }
    public Stream Strm { get; }

    public IType AttributeType => null;
}