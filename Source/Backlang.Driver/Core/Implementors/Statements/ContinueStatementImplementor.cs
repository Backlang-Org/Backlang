using Furesoft.Core.CodeDom.Compiler.Flow;

namespace Backlang.Driver.Core.Implementors.Statements;

public class ContinueStatementImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(LNode node, BasicBlockBuilder block, CompilerContext context, IMethod method,
        QualifiedName? modulename, Scope scope, BranchLabels branchLabels = null)
    {
        block.Flow = new JumpFlow(branchLabels.continueBranch);

        return block;
    }
}