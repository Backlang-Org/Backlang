using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Backends.Bs2k.TypeSystem;

public class I8Type : DescribedType
{
    public I8Type(IAssembly assembly) : base(new SimpleName("SByte").Qualify("System"), assembly)
    {
    }
}