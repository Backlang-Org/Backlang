namespace Backlang.Contracts.Scoping;

public abstract class ScopeItem
{
    public bool IsMutable { get; init; }
    public string Name { get; init; }

    public void Deconstruct(out string name, out bool isMutable)
    {
        name = Name;
        isMutable = IsMutable;
    }
}