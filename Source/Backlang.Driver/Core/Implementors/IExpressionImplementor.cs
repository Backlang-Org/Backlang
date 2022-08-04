using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Loyc.Syntax;

namespace Backlang.Driver.Core.Implementors;

public interface IExpressionImplementor
{
    NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, IMethod method, CompilerContext context, Scope scope);

    bool CanHandle(LNode node);
}