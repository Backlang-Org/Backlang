using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class DoWhileImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        if (parameters.node is (_, var body, var condition))
        {
            TypeDeducer.ExpectType(condition, parameters.scope, parameters.context,
                parameters.modulename.Value, parameters.context.Environment.Boolean);

            var do_body = parameters.block.Graph.AddBasicBlock(LabelGenerator.NewLabel("do_start"));
            var do_condition = parameters.block.Graph.AddBasicBlock(LabelGenerator.NewLabel("do_body"));
            var do_after = parameters.block.Graph.AddBasicBlock(LabelGenerator.NewLabel("do_after"));

            do_after.Flow = new NothingFlow();
            do_body.Flow = new NothingFlow();

            parameters.branchLabels.breakBranch = do_after;
            parameters.branchLabels.continueBranch = do_body;

            AppendBlock(body, do_body, parameters.context, parameters.method,
                parameters.modulename, parameters.scope.CreateChildScope());

            AppendExpression(do_condition, condition, parameters.context.Environment.Boolean,
                parameters.context, parameters.scope.CreateChildScope(), parameters.modulename);

            do_condition.Flow = new JumpConditionalFlow(do_body, ConditionalJumpKind.True);

            return do_after;
        }

        return parameters.block;
    }
}