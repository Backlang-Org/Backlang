using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Backends.Bs2k.TypeSystem;

public class VoidType : DescribedType
{
    public VoidType(IAssembly assembly) : base(new SimpleName("Void").Qualify("System"), assembly)
    {
    }
}