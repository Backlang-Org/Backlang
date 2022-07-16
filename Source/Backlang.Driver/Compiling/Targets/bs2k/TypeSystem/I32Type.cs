using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Targets.bs2k.TypeSystem;

public class I32Type : DescribedType
{
    public I32Type(IAssembly assembly) : base(new SimpleName("Int32").Qualify("System"), assembly)
    {
    }
}