namespace Backlang.Driver.Core.Implementors.Expressions;

public class CtorExpressionImplementor : IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.Calls(CodeSymbols.New);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        var args = node.Args.Without(node.Args[0]);
        node = node.WithArgs(args);

        var argTypes = new List<IType>();

        foreach (var arg in node.Args)
        {
            var type = TypeDeducer.Deduce(arg, scope, context, modulename.Value);
            argTypes.Add(type);
        }

        var callTags = ImplementationStage.AppendCallArguments(context, block, node, scope, modulename);

        var constructor = ImplementationStage.GetMatchingMethod(context, argTypes, elementType.Methods, ".ctor");
        return block.AppendInstruction(
            Instruction.CreateNewObject(
                constructor, callTags));
    }
}