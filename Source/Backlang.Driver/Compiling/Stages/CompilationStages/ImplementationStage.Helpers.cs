using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Contracts;
using Flo;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc;
using Loyc.Syntax;
using System.Collections.Immutable;

namespace Backlang.Driver.Compiling.Stages.CompilationStages;

public sealed partial class ImplementationStage : IHandler<CompilerContext, CompilerContext>
{
    public enum ConditionalJumpKind
    {
        NotEquals,
        Equals,
        True,
    }

    private static ImmutableDictionary<Symbol, Type> LiteralTypeMap = new Dictionary<Symbol, Type>
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
    }.ToImmutableDictionary();

    public static IType GetLiteralType(LNode value, TypeResolver resolver)
    {
        if (LiteralTypeMap.ContainsKey(value.Name))
        {
            return Utils.ResolveType(resolver, LiteralTypeMap[value.Name]);
        }
        else if (value is IdNode id) { } //todo: symbol table
        else
        {
            return Utils.ResolveType(resolver, value.Args[0].Value.GetType());
        }

        return Utils.ResolveType(resolver, typeof(void));
    }

    private static IMethod GetMatchingMethod(CompilerContext context, List<IType> argTypes, IEnumerable<IMethod> methods, string methodname)
    {
        foreach (var m in methods)
        {
            if (m.Name.ToString() != methodname) continue;

            if (m.Parameters.Count == argTypes.Count)
            {
                if (MatchesParameters(m, argTypes))
                    return m;
            }
        }

        return null;
    }

    private static bool MatchesParameters(IMethod method, List<IType> argTypes)
    {
        var methodParams = string.Join(',', method.Parameters.Select(_ => _.Type.FullName.ToString()));
        var monocecilParams = string.Join(',', argTypes.Select(_ => _.FullName.ToString()));

        return methodParams.Equals(monocecilParams, StringComparison.Ordinal);
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
}