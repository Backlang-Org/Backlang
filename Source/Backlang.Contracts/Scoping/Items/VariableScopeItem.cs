using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Contracts.Scoping.Items;

public class VariableScopeItem : ScopeItem
{
    public bool IsMutable { get; init; }

    public Parameter Parameter { get; set; }

    public override IType Type => Parameter.Type;

    public void Deconstruct(out string name, out bool isMutable)
    {
        name = Name;
        isMutable = IsMutable;
    }
}