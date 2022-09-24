namespace Backlang.Driver.Core.Implementors;

public interface IStatementImplementor
{
    BasicBlockBuilder Implement(CompilerContext context, IMethod method,
        BasicBlockBuilder block, LNode node, QualifiedName? modulename, Scope scope);
}