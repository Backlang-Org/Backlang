namespace Backlang.Contracts.TypeSystem;

public class ObjectType : DescribedType
{
    public ObjectType(IAssembly assembly) : base(new SimpleName("Object").Qualify("System"), assembly)
    {
    }
}