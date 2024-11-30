namespace Backlang.Driver.Core.Implementors.Expressions;

public class TupleExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node)
    {
        return node.Calls(CodeSymbols.Tuple);
    }

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        if (elementType.FullName.Qualifier is GenericName gt)
        {
            var valueTags = new List<ValueTag>();
            var genericTypes = new List<IType>();

            var gargs = elementType.GetGenericArguments();
            for (var i = 0; i < node.Args.Count; i++)
            {
                var arg = node.Args[i];
                var argType = gargs[i];

                valueTags.Add(ImplementationStage.AppendExpression(block, arg, argType, context, scope, modulename));
                genericTypes.Add(argType);
            }

            var ctor = elementType.Methods.FirstOrDefault(_ =>
                _.IsConstructor && _.Parameters.Count == valueTags.Count);

            if (ctor != null)
            {
                var directMethodSpecialization = ctor.MakeGenericMethod(gargs);

                GenericTypeMap.Cache.Add((elementType.FullName, directMethodSpecialization), elementType);
                block.AppendInstruction(Instruction.CreateNewObject(directMethodSpecialization, valueTags));
            }
        }

        return null;
    }
}