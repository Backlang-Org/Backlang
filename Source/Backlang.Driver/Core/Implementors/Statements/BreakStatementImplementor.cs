using Backlang.Driver.Core.Flows;

namespace Backlang.Driver.Core.Implementors.Statements;

public class BreakStatementImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        parameters.block.Flow = new BreakFlow(parameters.branchLabels.breakBranch);

        return parameters.block;
    }
}