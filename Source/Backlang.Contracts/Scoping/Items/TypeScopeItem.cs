using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Contracts.Scoping.Items;

public class TypeScopeItem : ScopeItem
{
    public IType Type { get; init; }
    public Scope SubScope { get; init; }

    public void Deconstruct(out string name, out IType type, out Scope subScope)
    {
        name = Name;
        type = Type;
        subScope = SubScope;
    }
}
