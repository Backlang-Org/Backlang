using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Contracts.Scoping.Items;

public class FunctionScopeItem : ScopeItem
{
    public IMethod Method { get; init; }
    public bool IsStatic => Method.IsStatic;
    public Scope SubScope { get; init; }

    public void Deconstruct(out string name, out IMethod method, out bool isStatic, out Scope subScope)
    {
        name = Name;
        method = Method;
        isStatic = IsStatic;
        subScope = SubScope;
    }
}
