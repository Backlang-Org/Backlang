namespace Backlang.Driver.Compiling.Scoping.Items;

public class VariableScopeItem : IScopeItem
{
    public bool IsMutable { get; set; }
    public string Name { get; set; }
}
