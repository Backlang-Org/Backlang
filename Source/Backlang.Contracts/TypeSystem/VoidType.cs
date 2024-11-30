namespace Backlang.Contracts.TypeSystem;

public class VoidType : DescribedType
{
    public VoidType(IAssembly assembly) : base(new SimpleName("Void").Qualify("System"), assembly)
    {
    }
}