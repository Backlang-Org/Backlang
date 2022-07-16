using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Targets.bs2k.TypeSystem;

public class U8Type : DescribedType
{
    public U8Type(IAssembly assembly) : base(new SimpleName("Byte").Qualify("System"), assembly)
    {
    }
}