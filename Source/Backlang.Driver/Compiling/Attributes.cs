using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling;
public static class Attributes
{
    public static readonly IAttribute Mutable = new IntrinsicAttribute("mutable");
}
