using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using LeMP;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Driver.InternalMacros;

[ContainsMacros]
public static class SyntacticMacros
{
    public static readonly Dictionary<Symbol, string> TypenameTable = new()
    {
        [CodeSymbols.Object] = "obj",

        [CodeSymbols.Bool] = "bool",

        [CodeSymbols.UInt8] = "u8",
        [CodeSymbols.UInt16] = "u16",
        [CodeSymbols.UInt32] = "u32",
        [CodeSymbols.UInt64] = "u64",

        [CodeSymbols.Int8] = "i8",
        [CodeSymbols.Int16] = "i16",
        [CodeSymbols.Int32] = "i32",
        [CodeSymbols.Int64] = "i64",

        [Symbols.Float16] = "f16",
        [Symbols.Float32] = "f32",
        [Symbols.Float64] = "f64",

        [CodeSymbols.Char] = "char",
        [CodeSymbols.String] = "string",
    };

    private static LNodeFactory F = new LNodeFactory(EmptySourceFile.Synthetic);

    [LexicalMacro("#autofree(hat) {}", "Frees the handles after using them in the body", "autofree")]
    public static LNode AutoFree(LNode @operator, IMacroContext context)
    {
        var body = @operator.Args.Last;

        var handles = @operator.Args.Take(@operator.Args.Count - 1);

        var freeCalls = LNode.List();

        foreach (var handle in handles)
        {
            freeCalls.Add(LNode.Call(LNode.Call(CodeSymbols.Dot, LNode.List(handle, LNode.Id((Symbol)"Free"))).SetStyle(NodeStyle.Operator)));
        }

        return body.WithArgs(LNode.List().AddRange(body.Args).AddRange(freeCalls));
    }

    [LexicalMacro("constructor()", "Convert constructor() to .ctor() function", "#constructor", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode Constructor(LNode @operator, IMacroContext context)
    {
        return SyntaxTree.Signature(SyntaxTree.Type(".ctor", new()), SyntaxTree.Type("none", LNode.List()), @operator.Args[0].Args, new()).PlusArg(@operator.Args[1]).WithAttrs(@operator.Attrs);
    }

    [LexicalMacro("destructor()", "Convert destructor() to .dtor() function", "#destructor", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode Destructor(LNode @operator, IMacroContext context)
    {
        return SyntaxTree.Signature(SyntaxTree.Type(".dtor", new()), SyntaxTree.Type("none", LNode.List()), @operator.Args[0].Args, new()).PlusArg(@operator.Args[1]).WithAttrs(@operator.Attrs);
    }

    [LexicalMacro("left /= right;", "Convert to left = left / something", "'/=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode DivEquals(LNode @operator, IMacroContext context)
    {
        return ConvertToAssignment(@operator, CodeSymbols.Div);
    }

    [LexicalMacro("operator", "Convert to public static op_", "#fn", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode ExpandOperator(LNode @operator, IMacroContext context)
    {
        var operatorAttribute = LNode.Id((Symbol)"#operator");
        if (@operator.Attrs.Contains(operatorAttribute))
        {
            var newAttrs = new LNodeList() { LNode.Id(CodeSymbols.Public), LNode.Id(CodeSymbols.Static), LNode.Id(CodeSymbols.Operator) };
            var modChanged = @operator.WithAttrs(newAttrs);
            var fnName = @operator.Args[1];

            var opMap = GetOpMap();
            if (opMap.ContainsKey(fnName.Name.Name))
            {
                var newTarget = LNode.Id("op_" + opMap[fnName.Name.Name]);
                return modChanged.WithArgChanged(1, newTarget);
            }
        }

        return @operator;
    }

    [LexicalMacro("^hat", "Gets a handle for the variable", "'^", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode HandleOperator(LNode @operator, IMacroContext context)
    {
        if (!@operator.Args[0].IsId) context.Error("Expected Identifier for HandleOperator");

        return LNode.Call(
            LNode.Call(LNode.Id("::"), LNode.List(
                LNode.Call(CodeSymbols.Dot, LNode.List(
                    LNode.Call(CodeSymbols.Dot, LNode.List(
                        LNode.Call(CodeSymbols.Dot, LNode.List(
                            LNode.Id("System"), LNode.Id("Runtime"))).SetStyle(NodeStyle.Operator),
                        LNode.Id("InteropServices"))).SetStyle(NodeStyle.Operator),
                    LNode.Id("GCHandle"))).SetStyle(NodeStyle.Operator),
                LNode.Id("Alloc"))).SetStyle(NodeStyle.Operator),
            LNode.List(@operator.Args[0]));
    }

    [LexicalMacro("Point::new()", "Convert ::New To CodeSymbols.New", "'::", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode Instantiation(LNode node, IMacroContext context)
    {
        if (node.Args.IsEmpty)
        {
            return node;
        }

        var left = node.Args[0];
        var right = node.Args[1];

        if (right.Calls("new"))
        {
            return LNode.Call(CodeSymbols.New,
                LNode.List(LNode.Call(left, right.Args)));
        }

        return node;
    }

    [LexicalMacro("left -= right;", "Convert to left = left - something", "'-=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode MinusEquals(LNode @operator, IMacroContext context)
    {
        return ConvertToAssignment(@operator, CodeSymbols.Sub);
    }

    [LexicalMacro("left *= right;", "Convert to left = left * something", "'*=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode MulEquals(LNode @operator, IMacroContext context)
    {
        return ConvertToAssignment(@operator, CodeSymbols.Mul);
    }

    [LexicalMacro("left += right;", "Convert to left = left + something", "'+=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode PlusEquals(LNode @operator, IMacroContext context)
    {
        return ConvertToAssignment(@operator, CodeSymbols.Add);
    }

    [LexicalMacro("#var", "Type Inference For Let", "#var", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode TypeInferenceForLet(LNode node, IMacroContext context)
    {
        if (node.ArgCount == 0)
        {
            return node;
        }

        var typename = node.Args[0];

        if (!typename.Calls(Symbols.TypeLiteral))
        {
            var definiton = node.Args[1];
            var value = definiton.Args[1];

            if (TypenameTable.ContainsKey(value.Name))
            {
                var newType = SyntaxTree.Type(TypenameTable[value.Name], LNode.List());

                return node.WithArgChanged(0, newType);
            }
        }

        return node;
    }

    [LexicalMacro("**", "Power Operator", "'**", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode PowerOperator(LNode node, IMacroContext context)
    {
        var left = node.Args[0];
        var right = node.Args[1];

        var powCall = F.Call(F.Call(CodeSymbols.Dot, LNode.List(
            LNode.Call(CodeSymbols.Dot, LNode.List(LNode.Id((Symbol)"System"), LNode.Id((Symbol)"Math"))).SetStyle(NodeStyle.Operator),
            LNode.Id((Symbol)"Pow"))).SetStyle(NodeStyle.Operator), LNode.List(left, right));

        return powCall.WithRange(node.Range);
    }

    private static LNode ConvertToAssignment(LNode @operator, Symbol symbol)
    {
        var arg1 = @operator.Args[0];
        var arg2 = @operator.Args[1];

        return F.Call(CodeSymbols.Assign, arg1, F.Call(symbol, arg1, arg2));
    }

    private static Dictionary<string, string> GetOpMap()
    {
        return new()
        {
            ["add"] = "Addition",
            ["sub"] = "Subtraction",
            ["mul"] = "Multiply",
            ["div"] = "Division",
            ["mod"] = "Modulus",
        };
    }
}