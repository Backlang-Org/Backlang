namespace Backlang.Contracts.TypeSystem;

public class U8Type : DescribedType
{
    public U8Type(IAssembly assembly) : base(new SimpleName("Byte").Qualify("System"), assembly)
    {
    }
}