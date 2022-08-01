using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Backends.Bs2k.TypeSystem;

public class I64Type : DescribedType
{
    public I64Type(IAssembly assembly) : base(new SimpleName("Int64").Qualify("System"), assembly)
    {
    }
}