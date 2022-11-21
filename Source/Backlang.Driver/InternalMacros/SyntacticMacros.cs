using LeMP;
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

        ["equality"] = ("Equality", 2),
        ["inequality"] = ("Inequality", 2),

        ["bitwise_not"] = ("OnesComplement", 1),

        ["deref"] = ("Deref", 1),
        ["addrof"] = ("AddressOf", 2),

        ["percent"] = ("Percentage", 1),
    };

    private static LNodeFactory F = new LNodeFactory(EmptySourceFile.Synthetic);

    [LexicalMacro("constructor()", "Convert constructor() to .ctor() function", "#constructor", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode Constructor(LNode node, IMacroContext context)
    {
        var factory = new LNodeFactory(node.Source);
        SyntaxTree.Factory = factory;

        return SyntaxTree.Signature(SyntaxTree.Type(".ctor", new()),
            SyntaxTree.Type("none", LNode.List()), node.Args[0].Args,
            new()).PlusArg(node.Args[1]).WithAttrs(node.Attrs).WithRange(node.Range);
    }

    [LexicalMacro("destructor()", "Convert destructor() to .dtor() function", "#destructor", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode Destructor(LNode node, IMacroContext context)
    {
        var factory = new LNodeFactory(node.Source);
        SyntaxTree.Factory = factory;

        return SyntaxTree.Signature(SyntaxTree.Type(".dtor", new()),
            SyntaxTree.Type("none", LNode.List()), node.Args[0].Args, new())
            .PlusArg(node.Args[1]).WithAttrs(node.Attrs).WithRange(node.Range);
    }

    [LexicalMacro(".dtor()", "Convert destructor() or .dtor() to Finalize", ".dtor", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode DestructorNormalisation(LNode @operator, IMacroContext context)
    {
        return @operator.WithTarget(LNode.Id("Finalize"));
    }

    [LexicalMacro("left /= right;", "Convert to left = left / something", "'/=", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode DivEquals(LNode @operator, IMacroContext context)
    {
        return ConvertToAssignment(@operator, CodeSymbols.Div);
    }

    [LexicalMacro("fn", "Expand notnull postfix for function parameter declaration", "#fn",
       Mode = MacroMode.MatchIdentifierOrCall | MacroMode.PriorityOverrideMax)]
    public static LNode ExpandNotnullAssertionPostfix(LNode node, IMacroContext context)
    {
        var newBody = new LNodeList();
        var newParameters = new LNodeList();

        foreach (var parameter in node[2].Args)
        {
            var nonNullAttribute = LNode.Id(Symbols.AssertNonNull);

            if (parameter.Attrs.Contains(nonNullAttribute))
            {
                var name = parameter[1][0];

                var throwNode = LNode.Call(CodeSymbols.Throw, LNode.List(LNode.Call(CodeSymbols.String, LNode.List(LNode.Literal($"Parameter '{name.Name}' is none")))));
                var ifBody = LNode.Call(CodeSymbols.Braces, LNode.List(throwNode));

                newBody = newBody.Add(SyntaxTree.If(LNode.Call(CodeSymbols.Eq, LNode.List(name, SyntaxTree.None())), ifBody, LNode.Missing));
            }

            newParameters.Add(parameter.WithoutAttrNamed(nonNullAttribute.Name));
        }

        newBody = newBody.AddRange(node[3].Args);

        node = node.WithArgChanged(2, node[3].WithArgs(newParameters));
        node = node.WithArgChanged(3, LNode.Call(CodeSymbols.Braces, newBody));

        return node;
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
            var factory = new LNodeFactory(node.Source);

            return factory.Call(CodeSymbols.New,
                LNode.List(factory.Call(left, right.Args))).WithRange(node.Range);
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
                var formatArgs = new List<LNode>();

                int counter = 0;

                formatString = Regex.Replace(formatString, "\\$(?<name>\\w[0-9a-zA-Z_]*)(:(\\{(?<options>[0-9a-zA-Z_]*)\\}))?", _ => {
                    var sb = new StringBuilder();
                    sb.Append('{').Append(counter++);

                    var options = _.Groups["options"].Value;

                    if (!string.IsNullOrEmpty(options))
                    {
                        sb.Append(':').Append(options);
                    }

                    sb.Append('}');

                    var varRange = new SourceRange(valueNode.Range.Source,
                        _.Index + node.Range.StartIndex + 1, _.Length);

                    formatArgs.Add(SyntaxTree.Factory.Id(_.Groups["name"].Value).WithRange(varRange));

                    return sb.ToString();
                });

                formatArgs.Insert(0, SyntaxTree.Factory.Call(CodeSymbols.String, LNode.List(SyntaxTree.Factory.Literal(formatString))));

                node = ExtensionUtils.coloncolon("string", LNode.Call((Symbol)"Format").WithArgs(formatArgs));
            }
        }

        return node;
    }

    private static LNode ConvertToAssignment(LNode node, Symbol symbol)
    {
        var arg1 = node.Args[0];
        var arg2 = node.Args[1];

        var factory = new LNodeFactory(node.Source);

        return factory.Call(CodeSymbols.Assign, arg1, factory.Call(symbol, arg1, arg2));
    }
}