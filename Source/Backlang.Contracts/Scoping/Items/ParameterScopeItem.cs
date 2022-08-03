using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Contracts.Scoping.Items;

public class ParameterScopeItem : ScopeItem
{
    public Parameter Parameter { get; init; }
    public IType Type => Parameter.Type;
    public bool HasDefault => Parameter.HasDefault;

    public void Deconstruct(out string name, out Parameter parameter, out IType type, out bool hasDefault)
    {
        name = Name;
        parameter = Parameter;
        type = Type;
        hasDefault = HasDefault;
    }
}
