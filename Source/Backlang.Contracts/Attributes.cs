using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Contracts;
public static class Attributes
{
    public static readonly IAttribute Mutable = new IntrinsicAttribute("mutable");
}
