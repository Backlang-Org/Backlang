namespace Backlang.Driver.Compiling.Scoping;

public abstract class ScopeItem
{
    public bool IsMutable { get; set; }
    public string Name { get; set; }

    public void Deconstruct(out string name, out bool isMutable)
    {
        name = Name;
        isMutable = IsMutable;
    }
}