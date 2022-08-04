using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Driver.Compiling;

public class EmbeddedResourceAttribute : IAttribute
{
    public EmbeddedResourceAttribute(string name, string filename)
    {
        Name = name;
        Filename = filename;
    }

    public string Name { get; set; }
    public string Filename { get; set; }

    public IType AttributeType => null;
}