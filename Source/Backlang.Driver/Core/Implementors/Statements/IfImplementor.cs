using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class IfImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(LNode node, BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename, Scope scope, BranchLabels branchLabels = null)
    {
        if (node is (_, (_, var condition, var body, var el)))
        {
            TypeDeducer.ExpectType(condition, scope, context, modulename.Value,
                context.Environment.Boolean);

            if (el.Name != LNode.Missing.Name)
            {
                ImplementIfElse(block, context, method, modulename, scope, branchLabels, condition, body, el);
            }
            else
            {
                ImplementIf(block, context, method, modulename, scope, branchLabels, condition, body);
            }

            var if_after = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("if_after"));
            if_after.Flow = new NothingFlow();

            return if_after;
        }

        return null;
    }

    private static void ImplementIf(BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename,
     Scope scope, BranchLabels branchLabels, LNode condition, LNode body)
    {
        var if_start = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("if_start"));
        var if_condition = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("if_condition"));
        var if_end = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("if_end"));

        AppendBlock(body, if_start, context, method, modulename, scope.CreateChildScope(), branchLabels);

        AppendExpression(if_condition, condition, context.Environment.Boolean, context, scope, modulename);

        if_condition.Flow = new JumpConditionalFlow(if_start, ConditionalJumpKind.True);

        block.Flow = new JumpFlow(if_condition);

        if_end.Flow = new NothingFlow();
    }

    private static BasicBlockBuilder ImplementIfElse(BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename,
                                                    Scope scope, BranchLabels branchLabels, LNode condition, LNode body, LNode elseBody)
    {
        var if_condition = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("if_condition"));
        var if_body = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("if_start"));
        var else_body = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("else_start"));
        var else_end = block.Graph.AddBasicBlock(LabelGenerator.NewLabel("else_end"));

        else_end.Flow = new NothingFlow();

        AppendExpression(if_condition, condition, context.Environment.Boolean, context, scope, modulename);
        if_condition.Flow = new JumpConditionalFlow(else_body, ConditionalJumpKind.False);

        AppendBlock(body, if_body, context, method, modulename, scope.CreateChildScope(), branchLabels);
        AppendBlock(elseBody, else_body, context, method, modulename, scope.CreateChildScope(), branchLabels);

        if (if_body.Flow is NothingFlow)
        {
            if_body.Flow = new JumpFlow(else_end);
        }
        
        return else_end;
    }
}