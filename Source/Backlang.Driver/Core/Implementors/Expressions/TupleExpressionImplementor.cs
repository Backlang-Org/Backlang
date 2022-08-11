namespace Backlang.Driver.Core.Implementors.Expressions;

public class TupleExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.Calls(CodeSymbols.Tuple);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block, IType elementType, CompilerContext context, Scope scope)
    {
        if (elementType is GenericType gt)
        {
            var valueTags = new List<ValueTag>();

            for (var i = 0; i < node.Args.Count; i++)
            {
                var arg = node.Args[i];
                var argType = gt.GenericArguments[i];

                valueTags.Add(ImplementationStage.AppendExpression(block, arg, argType, context, scope));
            }

            var ctor = gt.Type.Methods.FirstOrDefault(_ => _.IsConstructor && _.Parameters.Count == valueTags.Count);

            if (ctor != null)
            {
                block.AppendInstruction(Instruction.CreateNewObject(ctor, valueTags));
            }
        }

        return null;
    }
}