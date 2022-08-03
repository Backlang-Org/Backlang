using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Contracts.Scoping.Items;

public class FieldScopeItem : ScopeItem
{
    public IField Field { get; init; }
}
