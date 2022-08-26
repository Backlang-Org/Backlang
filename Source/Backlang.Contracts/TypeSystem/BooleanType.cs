using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Contracts.TypeSystem;

public class BooleanType : DescribedType
{
    public BooleanType(IAssembly assembly) : base(new SimpleName("Boolean").Qualify("System"), assembly)
    {
    }
}