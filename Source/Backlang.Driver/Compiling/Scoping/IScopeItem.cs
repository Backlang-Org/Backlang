namespace Backlang.Driver.Compiling.Scoping;

public interface IScopeItem
{
    bool IsMutable { get; set; }
    string Name { get; set; }
}