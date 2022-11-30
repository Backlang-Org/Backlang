using Backlang.Driver.Core.Flows;

namespace Backlang.Driver.Core.Implementors.Statements;

public class ContinueStatementImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        parameters.block.Flow = new ContinueFlow(parameters.branchLabels.continueBranch);

        return parameters.block;
    }
}