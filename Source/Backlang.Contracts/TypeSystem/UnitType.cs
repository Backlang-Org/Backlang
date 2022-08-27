namespace Backlang.Contracts.TypeSystem;

public class UnitType : DescribedType
{
    public UnitType(IAssembly assembly) : base(new SimpleName("Unit").Qualify("System"), assembly)
    {
    }

    public UnitType(IType type, IType unit) : this(type.GetDefiningAssemblyOrNull())
    {
        Unit = unit;

        AddBaseType(type);
    }

    public IType Unit { get; }

    public static bool operator ==(UnitType left, IType right)
    {
        return right is UnitType ut && left.Unit == ut.Unit && left.BaseTypes[0] == right.BaseTypes[0];
    }

    public static bool operator !=(UnitType left, IType right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{BaseTypes[0]}<{Unit.Name}>";
    }
}