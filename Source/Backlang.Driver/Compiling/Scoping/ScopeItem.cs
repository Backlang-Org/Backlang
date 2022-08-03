namespace Backlang.Driver.Compiling.Scoping;

public abstract class ScopeItem
{
    public bool IsMutable { get; set; }
    public string Name { get; set; }
}