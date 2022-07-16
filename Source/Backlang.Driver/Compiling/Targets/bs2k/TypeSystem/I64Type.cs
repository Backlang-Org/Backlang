using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Targets.bs2k.TypeSystem;

public class I64Type : DescribedType
{
    public I64Type(IAssembly assembly) : base(new SimpleName("I64").Qualify("System"), assembly)
    {
    }
}