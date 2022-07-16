using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Targets.bs2k.TypeSystem;

public class I8Type : DescribedType
{
    public I8Type(IAssembly assembly) : base(new SimpleName("I8").Qualify("System"), assembly)
    {
    }
}