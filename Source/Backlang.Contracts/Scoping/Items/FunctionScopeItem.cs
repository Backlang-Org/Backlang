namespace Backlang.Contracts.Scoping.Items;

public class FunctionScopeItem : ScopeItem
{
    public List<IMethod> Overloads { get; init; } = new();
    public bool IsStatic => Overloads[0].IsStatic;
    public Scope SubScope { get; init; }

    public override IType Type => Overloads[0].ReturnParameter.Type;

    public void Deconstruct(out string name, out List<IMethod> method, out bool isStatic, out Scope subScope)
    {
        name = Name;
        method = Overloads;
        isStatic = IsStatic;
        subScope = SubScope;
    }
}