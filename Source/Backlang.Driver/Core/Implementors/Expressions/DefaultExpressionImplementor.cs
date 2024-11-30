using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Expressions;

public class DefaultExpressionImplementor : IExpressionImplementor
{
    private readonly ImmutableDictionary<string, object> _defaults = new Dictionary<string, object>
    {
        ["Boolean"] = default(bool),
        ["Char"] = default(char),
        ["Byte"] = default(sbyte),
        ["Short"] = default(short),
        ["Int32"] = default(int),
        ["Int64"] = default(long),
        ["UByte"] = default(byte),
        ["UShort"] = default(ushort),
        ["UInt32"] = default(uint),
        ["UInt64"] = default(ulong),
        ["Float16"] = default(Half),
        ["Float32"] = default(float),
        ["Float64"] = default(double),
        ["String"] = string.Empty
    }.ToImmutableDictionary();

    public bool CanHandle(LNode node)
    {
        return node.ArgCount == 1 && node is ("'default", _);
    }

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        object value = null;

        if (_defaults.ContainsKey(elementType.Name.ToString()))
        {
            value = _defaults[elementType.Name.ToString()];
        }

        var constant = ConvertConstant(elementType, value);
        var v = block.AppendInstruction(constant);

        return block.AppendInstruction(Instruction.CreateLoad(elementType, v));
    }
}