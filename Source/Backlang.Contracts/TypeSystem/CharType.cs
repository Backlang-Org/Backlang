namespace Backlang.Contracts.TypeSystem;

public class CharType : DescribedType
{
    public CharType(IAssembly assembly) : base(new SimpleName("Char").Qualify("System"), assembly)
    {
    }
}