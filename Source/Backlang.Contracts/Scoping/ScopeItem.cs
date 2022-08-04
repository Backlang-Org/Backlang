using Furesoft.Core.CodeDom.Compiler.Core;

namespace Backlang.Contracts.Scoping;

public abstract class ScopeItem
{
    public string Name { get; init; }
    public abstract IType Type { get; }
}