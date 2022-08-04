using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Contracts.Scoping.Items;

public class TypeScopeItem : ScopeItem
{
    public IType TypeInfo { get; init; }
    public Scope SubScope { get; init; }
    public bool IsStatic => Type.Attributes.Contains(FlagAttribute.Static.AttributeType);

    public override IType Type => TypeInfo;

    public void Deconstruct(out string name, out IType type, out Scope subScope, out bool isStatic)
    {
        name = Name;
        type = Type;
        subScope = SubScope;
        isStatic = IsStatic;
    }
}