using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Backends.Bs2k.TypeSystem;

public class CharType : DescribedType
{
    public CharType(IAssembly assembly) : base(new SimpleName("Char").Qualify("System"), assembly)
    {
    }
}