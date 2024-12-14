﻿using DistIL.AsmIO;

namespace BacklangC.Core.Scoping.Items;

public class FunctionScopeItem : ScopeItem
{
    public List<MethodDesc> Overloads { get; init; } = new();
    public bool IsStatic => Overloads[0].IsStatic;
    public Scope SubScope { get; init; }

    public override TypeDesc Type => Overloads[0].ReturnType;

    public void Deconstruct(out string name, out List<MethodDesc> method, out bool isStatic, out Scope subScope)
    {
        name = Name;
        method = Overloads;
        isStatic = IsStatic;
        subScope = SubScope;
    }
}