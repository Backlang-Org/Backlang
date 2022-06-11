namespace Backlang.Driver.Compiling.Scoping;

public class ScopeItem
{
    public ScopeItem(string name, ScopeType itemType, bool isMutable)
    {
        Name = name;
        IsMutable = isMutable;
        Type = itemType;
    }

    public bool IsMutable { get; set; }
    public string Name { get; set; }
    public ScopeType Type { get; set; }
}