﻿namespace Backlang.Contracts.TypeSystem;

public class U16Type : DescribedType
{
    public U16Type(IAssembly assembly) : base(new SimpleName("UInt16").Qualify("System"), assembly)
    {
    }
}