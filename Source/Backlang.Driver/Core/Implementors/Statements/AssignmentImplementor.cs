using Backlang.Contracts.Scoping.Items;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class AssignmentImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block, LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is (_, var left, var right))
        {
            var lt = TypeDeducer.Deduce(left, scope, context);
            var rt = TypeDeducer.Deduce(right, scope, context);
            TypeDeducer.ExpectType(right, scope, context, lt);

            if (scope.TryGet<VariableScopeItem>(left.Name.Name, out var va))
            {
                if (!va.IsMutable)
                {
                    context.AddError(node, $"Cannot assing immutable variable '{va.Name}'");
                }

                var value = AppendExpression(block, right, rt, context, scope);

                block.AppendInstruction(Instruction.CreateStore(lt, new ValueTag(va.Parameter.Name.ToString()), value));
            }
            else
            {
                context.AddError(node, $"Variable '{left.Name.Name}' not found");
            }
        }

        return block;
    }
}