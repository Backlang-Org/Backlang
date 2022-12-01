using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class DoWhileImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(LNode node, BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename, Scope scope, BranchLabels branchLabels = null)
    {
        if (node is (_, var body, var condition))
        {
            TypeDeducer.ExpectType(condition, scope, context,
                modulename.Value, context.Environment.Boolean);

            var do_body = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("do_start"));
            var do_condition = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("do_body"));
            var do_after = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("do_after"));

            do_after.Flow = new NothingFlow();
            do_body.Flow = new NothingFlow();

            branchLabels.breakBranch = do_after;
            branchLabels.continueBranch = do_body;

            AppendBlock(body, do_body, context, method, modulename, scope.CreateChildScope(), branchLabels);

            AppendExpression(do_condition, condition, context.Environment.Boolean,
                context, scope.CreateChildScope(), modulename);

            do_condition.Flow = new JumpConditionalFlow(do_body, ConditionalJumpKind.True);

            return do_after;
        }

        return block;
    }
}