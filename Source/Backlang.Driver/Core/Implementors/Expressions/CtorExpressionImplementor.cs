namespace Backlang.Driver.Core.Implementors.Expressions;

public class CtorExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.Calls(CodeSymbols.New);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        var argTypes = DeduceArgs(node, context, scope, modulename);

        var callTags = ImplementationStage.AppendCallArguments(context, block, node.Args[0], scope, modulename);

        var constructor = ImplementationStage.GetMatchingMethod(context, argTypes, elementType.Methods, ".ctor", false);

        if (constructor == null)
        {
            var parameterNamesJoined = string.Join(',', argTypes);
            context.AddError(node, $"No matching constructor found for {elementType.Name}({parameterNamesJoined})");

            return null;
        }

        return block.AppendInstruction(
            Instruction.CreateNewObject(
                constructor, callTags));
    }

    private static List<IType> DeduceArgs(LNode node, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        var argTypes = new List<IType>();

        foreach (var arg in node.Args[0].Args)
        {
            var type = TypeDeducer.Deduce(arg, scope, context, modulename.Value);
            argTypes.Add(type);
        }

        return argTypes;
    }
}