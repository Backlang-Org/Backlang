namespace Backlang.Driver.Core.Implementors.Expressions;

public class TupleExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.Calls(CodeSymbols.Tuple);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block, IType elementType, CompilerContext context, Scope scope)
    {
        if (elementType.FullName.Qualifier is GenericName gt && elementType is DirectTypeSpecialization dt)
        {
            var valueTags = new List<ValueTag>();

            var gargs = elementType.GetGenericArguments();
            for (var i = 0; i < node.Args.Count; i++)
            {
                var arg = node.Args[i];
                var argType = gargs[i];

                valueTags.Add(ImplementationStage.AppendExpression(block, arg, argType, context, scope));
            }

            var ctor = dt.Declaration.Methods.FirstOrDefault(_ => _.IsConstructor && _.Parameters.Count == valueTags.Count);

            if (ctor != null)
            {
                block.AppendInstruction(Instruction.CreateNewObject(ctor, valueTags));
            }
        }

        return null;
    }
}