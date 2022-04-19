using Backlang.Codeanalysis.Parsing.AST;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public static class SyntaxTree
{
    public static LNodeFactory Factory = new(EmptySourceFile.Unknown);

    public static LNode Binary(Symbol op, LNode left, LNode right)
    {
        return LNode.Call(op, LNode.List(left, right)).SetStyle(NodeStyle.Operator);
    }

    public static LNode Default()
    {
        return Default(LNode.Missing);
    }

    public static LNode Default(LNode type)
    {
        return LNode.Call(CodeSymbols.Default, LNode.List(type));
    }

    public static LNode Fn(LNode name, TypeLiteral type, LNodeList args, LNodeList body)
    {
        return LNode.Call(args, CodeSymbols.Fn, LNode.List(type, name,
                LNode.Call(CodeSymbols.AltList, args), LNode.Call(CodeSymbols.Braces, body).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode For(LNode init, LNode arr, LNodeList body)
    {
        return LNode.Call(
            CodeSymbols.For,
                LNode.List(LNode.Call(CodeSymbols.AltList,
                    LNode.List(LNode.Call(CodeSymbols.In,
                        LNode.List(init, arr)).SetStyle(NodeStyle.Operator))), LNode.Missing,
                            LNode.Call(CodeSymbols.AltList),
                                LNode.Call(CodeSymbols.Braces,
                                    body).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode If(LNode cond, LNodeList ifBody, LNodeList elseBody)
    {
        return LNode.Call(CodeSymbols.If,
            LNode.List(cond, LNode.Call(CodeSymbols.Braces,
                ifBody).SetStyle(NodeStyle.StatementBlock),
                    LNode.Call(CodeSymbols.Braces,
                        elseBody).SetStyle(NodeStyle.StatementBlock)));
    }

    public static LNode SizeOf(LNode type)
    {
        return LNode.Call(CodeSymbols.Sizeof, LNode.List(type));
    }

    public static LNode Unary(Symbol op, LNode arg)
    {
        return LNode.Call(op, LNode.List(arg)).SetStyle(NodeStyle.Operator);
    }

    public static LNode While(LNode cond, LNodeList body)
    {
        return LNode.Call(
            CodeSymbols.While,
                LNode.List(cond,
                    LNode.Call(CodeSymbols.Braces,
                        body).SetStyle(NodeStyle.StatementBlock)));
    }
}