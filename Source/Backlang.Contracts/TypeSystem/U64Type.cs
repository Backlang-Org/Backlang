﻿using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Contracts.TypeSystem;

public class U64Type : DescribedType
{
    public U64Type(IAssembly assembly) : base(new SimpleName("UInt64").Qualify("System"), assembly)
    {
    }
}