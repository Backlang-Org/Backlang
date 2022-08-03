﻿using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Loyc.Syntax;

namespace Backlang.Driver.Core;

public interface IImplementor
{
    BasicBlockBuilder Implement(CompilerContext context, IMethod method,
        BasicBlockBuilder block, LNode node, QualifiedName? modulename, Scope scope);
}