﻿using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Contracts.TypeSystem;

public class ObjectType : DescribedType
{
    public ObjectType(IAssembly assembly) : base(new SimpleName("Object").Qualify("System"), assembly)
    {
    }
}