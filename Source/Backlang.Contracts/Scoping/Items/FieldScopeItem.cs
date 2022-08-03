using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Contracts.Scoping.Items;

public class FieldScopeItem : ScopeItem
{
    public IField Field { get; init; }
    public bool IsMutable => Field.Attributes.Contains(Attributes.Mutable.AttributeType);
    public bool IsStatic => Field.IsStatic;

    public void Deconstruct(out string name, out IField field, out bool isMutable, out bool isStatic)
    {
        name = Name;
        field = Field;
        isMutable = IsMutable;
        isStatic = IsStatic;
    }
}
