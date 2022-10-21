using LeMP;
using Loyc.Collections;
using System.Text.RegularExpressions;

namespace Backlang.Driver.InternalMacros;

[ContainsMacros]
public static class SyntacticMacros
{
    private static readonly Dictionary<string, (string OperatorName, int ArgumentCount)> OpMap = new()
    {
        ["add"] = ("Addition", 2),
        ["sub"] = ("Subtraction", 2),
        ["mul"] = ("Multiply", 2),
        ["div"] = ("Division", 2),
        ["mod"] = ("Modulus", 2),

        ["logical_not"] = ("LogicalNot", 1),

        ["neg"] = ("UnaryNegation", 1),

        ["bitwise_and"] = ("BitwiseAnd", 2),
        ["bitwise_or"] = ("BitwiseOr", 2),
        ["exclusive_or"] = ("ExclusiveOr", 2),

        ["bitwise_not"] = ("OnesComplement", 1),

        ["deref"] = ("Deref", 1),
        ["addrof"] = ("AddressOf", 2),

        ["percent"] = ("Percentage", 1),
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

    // this currently doesnt work because macros crash when they have statements in their body
    [LexicalMacro("main/Main", "Converts main/Main to the main function", "main", "Main", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode Main(LNode @operator, IMacroContext context)
    {
        if (!@operator.Kind.Equals(LNodeKind.Call))
        {
            return @operator;
        }
        return SyntaxTree.Signature(
            SyntaxTree.Type("main", new()),
            SyntaxTree.Type("none", new()), new(), new())
            .PlusArg(@operator.Args[1]).PlusAttr(LNode.Id(CodeSymbols.Public)).PlusAttr(LNode.Id(CodeSymbols.Static));
    }

    [LexicalMacro("left /= right;", "Convert to left = left / something", "'/=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode DivEquals(LNode @operator, IMacroContext context)
    {
        return ConvertToAssignment(@operator, CodeSymbols.Div);
    }

    [LexicalMacro("operator", "Convert to public static op_", "#fn",
        Mode = MacroMode.MatchIdentifierOrCall | MacroMode.PriorityOverride)]
    public static LNode ExpandOperator(LNode @operator, IMacroContext context)
    {
        var operatorAttribute = SyntaxTree.Factory.Id((Symbol)"#operator");
        if (@operator.Attrs.Contains(operatorAttribute))
        {
            var newAttrs = new LNodeList() { LNode.Id(CodeSymbols.Public), LNode.Id(CodeSymbols.Static), LNode.Id(CodeSymbols.Operator) };
            var modChanged = @operator.WithAttrs(newAttrs);
            var fnName = @operator.Args[1];
            var compContext = (CompilerContext)context.ScopedProperties["Context"];

            if (fnName is (_, (_, var name)) && OpMap.ContainsKey(name.Name.Name))
            {
                var op = OpMap[name.Name.Name];

                if (@operator[2].ArgCount != op.ArgumentCount)
                {
                    compContext.AddError(@operator, $"Cannot overload operator, parameter count mismatch. {op.ArgumentCount} parameters expected");
                }

                var newTarget = SyntaxTree.Type("op_" + op.OperatorName, LNode.List()).WithRange(fnName.Range);
                return modChanged.WithArgChanged(1, newTarget);
            }
        }

        return @operator;
    }

    [LexicalMacro("12%", "divide by 100 (percent)", "'%", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode Percentage(LNode @operator, IMacroContext context)
    {
        if (@operator is (_, var inner))
        {
            if (inner.ArgCount == 1 && inner.Args[0] is LiteralNode ln)
            {
                dynamic percentValue = ((dynamic)ln.Value) / 100;

                if (percentValue is int pi)
                {
                    return LNode.Call(CodeSymbols.Int32, LNode.List(LNode.Literal(pi)));
                }
                else if (percentValue is double di)
                {
                    return LNode.Call(Symbols.Float32, LNode.List(LNode.Literal(di)));
                }
            }

            return SyntaxTree.Binary(CodeSymbols.Div, inner,
                LNode.Call(CodeSymbols.Int32, LNode.List(LNode.Literal(100))));
        }

        return @operator;
    }

    [LexicalMacro("^hat", "Gets a handle for the variable", "'^", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode HandleOperator(LNode @operator, IMacroContext context)
    {
        if (@operator.Args.Count != 1) return @operator;

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

    [LexicalMacro("target(dotnet) {}", "Only Compile Code If CompilationTarget Is Selected", "target", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode TargetMacro(LNode node, IMacroContext context)
    {
        var target = (string)context.ScopedProperties["Target"];
        var selectedTarget = node.Args[0].Name.Name;

        if (target == selectedTarget)
        {
            return node.Args[1];
        }

        context.DropRemainingNodes = true;

        return LNode.Call((Symbol)"'{}");
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

    [LexicalMacro("left |= right;", "Convert to left = left | something", "'|=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode OrEquals(LNode @operator, IMacroContext context)
    {
        return ConvertToAssignment(@operator, CodeSymbols.Or);
    }

    [LexicalMacro("left &= right;", "Convert to left = left & something", "'&=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode AndEquals(LNode @operator, IMacroContext context)
    {
        return ConvertToAssignment(@operator, CodeSymbols.And);
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

    [LexicalMacro("\"hello $name\"", "Interpolate a string", "#string")]
    public static LNode InterpolateString(LNode node, IMacroContext context)
    {
        if (node is (_, var valueNode))
        {
            string formatString = valueNode.Value.ToString();
            if (formatString.Contains('$'))
            {
                var interpolateOptions = GetInterpoltedStringOptions(formatString);
                var formatArgs = new List<LNode>();

                int counter = 0;
                foreach (var item in interpolateOptions)
                {
                    if (formatString[item.start - 1] == '\\')
                    {
                        continue;
                    }

                    formatString = formatString.Replace($"{item.name}", "{" + counter++ + "}");

                    var varRange = new SourceRange(valueNode.Range.Source,
                        item.start + node.Range.StartIndex + 1, item.length);

                    formatArgs.Add(SyntaxTree.Factory.Id(item.name[1..]).WithRange(varRange));
                }

                formatArgs.Insert(0, SyntaxTree.Factory.Call(CodeSymbols.String, LNode.List(SyntaxTree.Factory.Literal(formatString))));

                node = ExtensionUtils.coloncolon("string", LNode.Call((Symbol)"Format").WithArgs(formatArgs));
            }
        }

        return node;
    }

    private static List<(string name, int start, int length)> GetInterpoltedStringOptions(string value)
    {
        var result = new List<(string name, int start, int length)>();

        var match = Regex.Matches(value, "\\$[a-zA-Z_][0-9a-zA-Z_]*");

        foreach (Match m in match)
        {
            result.Add((m.Value, m.Index, m.Length));
        }

        return result;
    }

    private static LNode ConvertToAssignment(LNode @operator, Symbol symbol)
    {
        var arg1 = @operator.Args[0];
        var arg2 = @operator.Args[1];

        return F.Call(CodeSymbols.Assign, arg1, F.Call(symbol, arg1, arg2));
    }
}