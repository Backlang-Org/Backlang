using DistIL.AsmIO;

namespace BacklangC.Core.Scoping;

public abstract class ScopeItem
{
    public string Name { get; init; }
    public abstract TypeDesc Type { get; }
}