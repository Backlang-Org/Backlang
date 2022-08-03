using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Contracts.Scoping.Items;

public class FieldScopeItem : ScopeItem
{
    public IField Field { get; init; }
    public bool IsMutable { get; init; }
    public bool IsStatic { get; init; }

    public void Deconstruct(out string name, out IField field, out bool isMutable, out bool isStatic)
    {
        name = Name;
        field = Field;
        isMutable = IsMutable;
        isStatic = IsStatic;
    }
}
