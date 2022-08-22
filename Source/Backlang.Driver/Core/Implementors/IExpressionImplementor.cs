namespace Backlang.Driver.Core.Implementors;

public interface IExpressionImplementor
{
    NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope);

    bool CanHandle(LNode node);
}