using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Backends.Bs2k.TypeSystem;

public class StringType : DescribedType
{
    public StringType(IAssembly assembly) : base(new SimpleName("String").Qualify("System"), assembly)
    {
    }
}