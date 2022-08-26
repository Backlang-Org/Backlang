using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Contracts.TypeSystem;

public class I32Type : DescribedType
{
    public I32Type(IAssembly assembly) : base(new SimpleName("Int32").Qualify("System"), assembly)
    {
    }
}