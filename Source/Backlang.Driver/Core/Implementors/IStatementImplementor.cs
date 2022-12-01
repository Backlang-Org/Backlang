namespace Backlang.Driver.Core.Implementors;

public interface IStatementImplementor
{
    BasicBlockBuilder Implement(LNode node, BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename, Scope scope, BranchLabels branchLabels = null);
}