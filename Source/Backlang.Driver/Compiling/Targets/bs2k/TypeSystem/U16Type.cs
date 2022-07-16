using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Targets.bs2k.TypeSystem;

public class U16Type : DescribedType
{
    public U16Type(IAssembly assembly) : base(new SimpleName("UInt16").Qualify("System"), assembly)
    {
    }
}