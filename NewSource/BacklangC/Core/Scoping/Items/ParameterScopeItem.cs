using DistIL.AsmIO;
using DistIL.IR;

namespace BacklangC.Core.Scoping.Items;

public class ParameterScopeItem : ScopeItem
{
    public Argument Parameter { get; init; }

    public override TypeDesc Type => Parameter.ResultType;


    public void Deconstruct(out string name, out Argument parameter, out TypeDesc type)
    {
        name = Name;
        parameter = Parameter;
        type = Type;
    }
}