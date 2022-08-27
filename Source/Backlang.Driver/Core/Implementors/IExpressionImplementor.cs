namespace Backlang.Driver.Core.Implementors;

public interface IExpressionImplementor
{
    NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename);

    bool CanHandle(LNode node);
}