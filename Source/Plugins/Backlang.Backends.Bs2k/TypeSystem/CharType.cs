using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Targets.bs2k.TypeSystem;

public class CharType : DescribedType
{
    public CharType(IAssembly assembly) : base(new SimpleName("Char").Qualify("System"), assembly)
    {
    }
}