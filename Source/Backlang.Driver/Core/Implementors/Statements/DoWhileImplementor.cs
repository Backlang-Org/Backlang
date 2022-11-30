using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class DoWhileImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block, LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is (_, var body, var condition))
        {
            TypeDeducer.ExpectType(condition, scope, context, modulename.Value, context.Environment.Boolean);

            var do_body = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("do_start"));
            var do_condition = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("do_body"));
            var do_after = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("do_after"));

            do_after.Flow = new NothingFlow();
            do_body.Flow = new NothingFlow();

            AppendBlock(body, do_body, context, method, modulename, scope.CreateChildScope());

            AppendExpression(do_condition, condition, context.Environment.Boolean, context, scope.CreateChildScope(), modulename);

            do_condition.Flow = new JumpConditionalFlow(do_body, ConditionalJumpKind.True);
           
            return do_after;
        }

        return block;
    }
}