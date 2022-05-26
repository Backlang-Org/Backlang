using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;
public sealed class SwitchStatement : IParsePoint<LNode>
{

    /*
     * switch element {
     *  case cond: oneExpr;
     *  case cond: { block; }
     *  if boolean: oneExpr;
     *  if boolean: { block; }
     *  default: oneExpr;
     *  default: { block; }
     * }
     */
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var element = Expression.Parse(parser);

        parser.Iterator.Match(TokenType.OpenCurly);

        var cases = new LNodeList();

        while (!parser.Iterator.IsMatch(TokenType.CloseCurly))
        {
            if(iterator.IsMatch(TokenType.Case))
                cases.Add(ParseCase(parser));
            if (iterator.IsMatch(TokenType.If))
                cases.Add(ParseIf(parser));
            if (iterator.IsMatch(TokenType.Default))
                cases.Add(ParseDefault(parser));
        }

        parser.Iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Switch(element, cases);
    }

    private static LNode ParseCase(Parser parser)
    {
        parser.Iterator.Match(TokenType.Case);

        var condition = Expression.Parse(parser);

        parser.Iterator.Match(TokenType.Colon);

        var body = Statement.ParseOneOrBlock(parser);

        return SyntaxTree.Case(condition, body);
    }

    private static LNode ParseIf(Parser parser)
    {
        parser.Iterator.Match(TokenType.If);

        var condition = Expression.Parse(parser);

        parser.Iterator.Match(TokenType.Colon);

        var body = Statement.ParseOneOrBlock(parser);

        return SyntaxTree.If(condition, body, LNode.List());
    }

    private static LNode ParseDefault(Parser parser)
    {
        parser.Iterator.Match(TokenType.Default);

        parser.Iterator.Match(TokenType.Colon);

        var body = Statement.ParseOneOrBlock(parser);

        return SyntaxTree.Case(LNode.Call(CodeSymbols.Default), body);
    }

}
