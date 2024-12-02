using DistIL.AsmIO;
using DistIL.IR;

namespace BacklangC.Core.Scoping.Items;

public class VariableScopeItem : ScopeItem
{
    public bool IsMutable { get; init; }

    public LocalSlot Parameter { get; set; }

    public override TypeDesc Type => Parameter.Type;

    public void Deconstruct(out string name, out bool isMutable)
    {
        name = Name;
        isMutable = IsMutable;
    }
}