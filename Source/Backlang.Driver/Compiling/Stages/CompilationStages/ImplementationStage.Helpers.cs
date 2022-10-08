using Flo;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;

namespace Backlang.Driver.Compiling.Stages.CompilationStages;

public sealed partial class ImplementationStage : IHandler<CompilerContext, CompilerContext>
{
    public static ImmutableDictionary<Symbol, Type> LiteralTypeMap = new Dictionary<Symbol, Type>
    {
        [CodeSymbols.Bool] = typeof(bool),

        [CodeSymbols.String] = typeof(string),
        [CodeSymbols.Char] = typeof(char),

        [CodeSymbols.Int8] = typeof(byte),
        [CodeSymbols.Int16] = typeof(short),
        [CodeSymbols.UInt16] = typeof(ushort),
        [CodeSymbols.Int32] = typeof(int),
        [CodeSymbols.UInt32] = typeof(uint),
        [CodeSymbols.Int64] = typeof(long),
        [CodeSymbols.UInt64] = typeof(ulong),

        [Symbols.Float16] = typeof(Half),
        [Symbols.Float32] = typeof(float),
        [Symbols.Float64] = typeof(double),

        [CodeSymbols.Void] = typeof(void),
    }.ToImmutableDictionary();

    public enum ConditionalJumpKind
    {
        NotEquals,
        Equals,
        True,
    }

    public static IType GetLiteralType(LNode value, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        if (LiteralTypeMap.ContainsKey(value.Name))
        {
            return Utils.ResolveType(context.Binder, LiteralTypeMap[value.Name]);
        }
        else if (value.IsId)
        {
            return TypeDeducer.Deduce(value, scope, context, modulename.Value);
        }

        return Utils.ResolveType(context.Binder, typeof(void));
    }

    public static Instruction ConvertConstant(IType elementType, object value)
    {
        Constant constant;
        switch (value)
        {
            case uint v:
                constant = new IntegerConstant(v);
                break;

            case int v:
                constant = new IntegerConstant(v);
                break;

            case long v:
                constant = new IntegerConstant(v);
                break;

            case ulong v:
                constant = new IntegerConstant(v);
                break;

            case byte v:
                constant = new IntegerConstant(v);
                break;

            case short v:
                constant = new IntegerConstant(v);
                break;

            case ushort v:
                constant = new IntegerConstant(v);
                break;

            case float v:
                constant = new Float32Constant(v);
                break;

            case double v:
                constant = new Float64Constant(v);
                break;

            case string v:
                constant = new StringConstant(v);
                break;

            case char v:
                constant = new IntegerConstant(v);
                break;

            case bool v:
                constant = BooleanConstant.Create(v);
                break;

            default:
                if (value == null)
                {
                    constant = NullConstant.Instance;
                }
                else
                {
                    if (elementType.Attributes.Contains(IntrinsicAttribute.GetIntrinsicAttributeType("#Enum")))
                    {
                        constant = new EnumConstant(value, elementType);
                    }
                    else
                    {
                        constant = null;
                    }
                }

                break;
        }

        return Instruction.CreateConstant(constant,
                                           elementType);
    }

    public static bool MatchesParameters(IMethod method, List<IType> argTypes)
    {
        //ToDo: fix matches parameter (implicit casting is currently not working)

        bool matchesAllParameters = false;
        for (int i = 0; i < argTypes.Count; i++)
        {
            if (i == 0)
            {
                matchesAllParameters = ImplicitTypeCastTable.IsAssignableTo(argTypes[i], method.Parameters[i].Type);
                continue;
            }

            matchesAllParameters &= ImplicitTypeCastTable.IsAssignableTo(argTypes[i], method.Parameters[i].Type);
        }

        return matchesAllParameters;
    }

    private static IMethod GetMatchingMethod(CompilerContext context, List<IType> argTypes, IEnumerable<IMethod> methods, string methodname)
    {
        foreach (var m in methods.Where(_ => _.Name.ToString() == methodname))
        {
            if (m.Parameters.Count == argTypes.Count)
            {
                if (MatchesParameters(m, argTypes))
                    return m;
            }
        }

        context.Messages.Add(Message.Error($"Cannot find matching function '{methodname}({string.Join(", ", argTypes.Select(_ => _.FullName.ToString()))})'"));

        return null;
    }
}