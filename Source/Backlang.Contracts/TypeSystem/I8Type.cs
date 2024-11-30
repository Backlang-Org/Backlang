namespace Backlang.Contracts.TypeSystem;

public class I8Type : DescribedType
{
    public I8Type(IAssembly assembly) : base(new SimpleName("SByte").Qualify("System"), assembly)
    {
    }
}