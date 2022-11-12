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

            var do_body = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("do_body"));
            var subscope = scope.CreateChildScope();
            AppendBlock(body, do_body, context, method, modulename, subscope);

            AppendExpression(do_body, condition, context.Environment.Boolean, context, subscope, modulename);

            do_body.Flow = new JumpConditionalFlow(do_body, ConditionalJumpKind.True);
            block.Flow = new NothingFlow();
        }

        return block.Graph.AddBasicBlock();
    }
}