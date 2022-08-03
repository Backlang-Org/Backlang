namespace Backlang.Contracts.Scoping.Items;

public class VariableScopeItem : ScopeItem
{
    public bool IsMutable { get; init; }

    public void Deconstruct(out string name, out bool isMutable)
    {
        name = Name;
        isMutable = IsMutable;
    }
}
