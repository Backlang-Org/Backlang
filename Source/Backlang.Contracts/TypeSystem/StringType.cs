using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Contracts.TypeSystem;

public class StringType : DescribedType
{
    public StringType(IAssembly assembly) : base(new SimpleName("String").Qualify("System"), assembly)
    {
    }
}