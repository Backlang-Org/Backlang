using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Contracts.Scoping.Items;

public class FieldScopeItem : ScopeItem
{
    public IField Field { get; init; }
    public bool IsMutable { get; init; }
    public bool IsStatic { get; init; }
}
