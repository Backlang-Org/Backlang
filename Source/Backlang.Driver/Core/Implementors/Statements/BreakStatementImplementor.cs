using Furesoft.Core.CodeDom.Compiler.Flow;

namespace Backlang.Driver.Core.Implementors.Statements;

public class BreakStatementImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        parameters.block.Flow = new JumpFlow(parameters.branchLabels.breakBranch);

        return parameters.block;
    }
}