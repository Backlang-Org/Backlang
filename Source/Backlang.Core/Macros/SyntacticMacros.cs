using LeMP;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Core.Macros;

public static partial class BuiltInMacros
{
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

    [LexicalMacro(@"\\ Fraction", "Creates a fraction with the left and righthand side", @"'\\", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode FractionBinOp(LNode @operator, IMacroContext context)
    {
        var left = @operator.Args[0];
        var right = @operator.Args[1];
        return LNode.Call(LNode.Call(CodeSymbols.ColonColon, LNode.List(LNode.Call(CodeSymbols.Dot, LNode.List(LNode.Call(CodeSymbols.Dot, LNode.List(LNode.Id((Symbol)"Backlang"), LNode.Id((Symbol)"Core"))).SetStyle(NodeStyle.Operator), LNode.Id((Symbol)"Fraction"))).SetStyle(NodeStyle.Operator), LNode.Id(CodeSymbols.New))).SetStyle(NodeStyle.Operator), LNode.List(left, right));
    }

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

    [LexicalMacro("Point::new()", "Convert ::New To CodeSymbols.New", "'::", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode Instantiation(LNode node, IMacroContext context)
    {
        var left = node.Args[0];
        var right = node.Args[1];

        if (right.Calls("new"))
        {
            return LNode.Call(CodeSymbols.New,
                LNode.List(LNode.Call(left, right.Args)));
        }

        return node;
    }

    [LexicalMacro("left += right;", "Convert to left = left + something", "'+=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode PlusEquals(LNode @operator, IMacroContext context)
    {
        return ConverToAssignment(@operator, CodeSymbols.Add);
    }

    [LexicalMacro("left -= right;", "Convert to left = left - something", "'-=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode MinusEquals(LNode @operator, IMacroContext context)
    {
        return ConverToAssignment(@operator, CodeSymbols.Sub);
    }

    [LexicalMacro("left *= right;", "Convert to left = left * something", "'*=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode MulEquals(LNode @operator, IMacroContext context)
    {
        return ConverToAssignment(@operator, CodeSymbols.Mul);
    }

    [LexicalMacro("left /= right;", "Convert to left = left / something", "'/=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode DivEquals(LNode @operator, IMacroContext context)
    {
        return ConverToAssignment(@operator, CodeSymbols.Div);
    }

    private static LNode ConverToAssignment(LNode @operator, Symbol symbol)
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