using Backlang.Codeanalysis.Parsing.AST;
using Loyc.Syntax;

namespace BacklangC.Core;

public static class ExtensionUtils
{
    public static LNode dot(this string s, string other)
    {
        return LNode.Call(CodeSymbols.Dot, LNode.List(LNode.Id(s), LNode.Id(other))).SetStyle(NodeStyle.Operator);
    }

    public static LNode dot(this LNode n, string other)
    {
        return LNode.Call(CodeSymbols.Dot, LNode.List(n, LNode.Id(other))).SetStyle(NodeStyle.Operator);
    }

    public static LNode dot(this string s, LNode other)
    {
        return LNode.Call(CodeSymbols.Dot, LNode.List(LNode.Id(s), other)).SetStyle(NodeStyle.Operator);
    }

    public static LNode dot(this LNode n, LNode other)
    {
        return LNode.Call(CodeSymbols.Dot, LNode.List(n, other)).SetStyle(NodeStyle.Operator);
    }

    public static LNode coloncolon(this string s, string other)
    {
        return LNode.Call(Symbols.ColonColon, LNode.List(LNode.Id(s), LNode.Id(other))).SetStyle(NodeStyle.Operator);
    }

    public static LNode coloncolon(this LNode n, string other)
    {
        return LNode.Call(Symbols.ColonColon, LNode.List(n, LNode.Id(other))).SetStyle(NodeStyle.Operator);
    }

    public static LNode coloncolon(this string s, LNode other)
    {
        return LNode.Call(Symbols.ColonColon, LNode.List(LNode.Id(s), other)).SetStyle(NodeStyle.Operator);
    }

    public static LNode coloncolon(this LNode n, LNode other)
    {
        return LNode.Call(Symbols.ColonColon, LNode.List(n, other)).SetStyle(NodeStyle.Operator);
    }
}