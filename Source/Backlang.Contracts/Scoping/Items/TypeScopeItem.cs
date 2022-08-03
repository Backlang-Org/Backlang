using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Contracts.Scoping.Items;

public class TypeScopeItem : ScopeItem
{
    public IType Type { get; init; }
    public Scope SubScope { get; init; }
}
