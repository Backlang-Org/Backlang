namespace Backlang.Contracts.TypeSystem;

public class StringType : DescribedType
{
    public StringType(IAssembly assembly) : base(new SimpleName("String").Qualify("System"), assembly)
    {
    }
}