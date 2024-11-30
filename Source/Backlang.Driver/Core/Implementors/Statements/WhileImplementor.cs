using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class WhileImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(LNode node, BasicBlockBuilder block, CompilerContext context, IMethod method,
        QualifiedName? modulename, Scope scope, BranchLabels branchLabels)
    {
        if (node is var (_, condition, body))
        {
            TypeDeducer.ExpectType(condition, scope, context, modulename.Value,
                context.Environment.Boolean);

            var while_start = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_start"));
            var while_condition = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_condition"));
            var while_end = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_end"));
            var while_after = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("while_after"));
            while_after.Flow = new NothingFlow();

            branchLabels = new BranchLabels { breakBranch = while_after, continueBranch = while_condition };

            AppendBlock(body, while_start, context, method, modulename, scope.CreateChildScope(), branchLabels);

            AppendExpression(while_condition, condition, context.Environment.Boolean, context, scope, modulename);

            while_condition.Flow = new JumpConditionalFlow(while_start, ConditionalJumpKind.True);

            block.Flow = new JumpFlow(while_condition);

            while_end.Flow = new NothingFlow();

            return while_after;
        }

        return null;
    }
}