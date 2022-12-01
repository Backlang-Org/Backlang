using Furesoft.Core.CodeDom.Compiler.Flow;

namespace Backlang.Driver.Core.Implementors.Statements;

public class ContinueStatementImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        parameters.block.Flow = new JumpFlow(parameters.branchLabels.continueBranch);

        return parameters.block;
    }
}