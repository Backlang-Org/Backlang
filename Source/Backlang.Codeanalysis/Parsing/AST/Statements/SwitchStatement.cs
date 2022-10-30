using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class SwitchStatement : IParsePoint
{
    /*
     * switch element {
     *  case cond: oneExpr;
     *  case cond: { block; }
     *  if boolean: oneExpr;
     *  if boolean: { block; }
     *  break when > value: { block; }
     *  default: oneExpr;
     *  default: { block; }
     * }
     */

    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var element = Expression.Parse(parser);

        parser.Iterator.Match(TokenType.OpenCurly);

        var cases = new LNodeList();

        while (!parser.Iterator.IsMatch(TokenType.CloseCurly))
        {
            bool autoBreak = iterator.IsMatch(TokenType.Break);

            if (autoBreak) iterator.Match(TokenType.Break);

            if (iterator.IsMatch(TokenType.Case))
                cases.Add(ParseCase(parser, autoBreak));
            else if (iterator.IsMatch(TokenType.If))
                cases.Add(ParseIf(parser, autoBreak));
            else if (iterator.IsMatch(TokenType.When))
                cases.Add(ParseWhen(parser, autoBreak));
            else if (iterator.IsMatch(TokenType.Default))
                cases.Add(ParseDefault(parser, autoBreak));
            else
            {
                var range = new SourceRange(parser.Document, iterator.Current.Start, iterator.Current.Text.Length);

                parser.AddError(new(Core.ErrorID.UnknownSwitchOption), range);
                return LNode.Missing;
            }
        }

        parser.Iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Switch(element, cases).WithRange(keywordToken, iterator.Prev);
    }

    private static LNode ParseCase(Parser parser, bool autoBreak)
    {
        var keywordToken = parser.Iterator.Match(TokenType.Case);

        var condition = Expression.Parse(parser);

        parser.Iterator.Match(TokenType.Colon);

        var body = Statement.ParseOneOrBlock(parser);

        if (autoBreak)
            body = body.PlusArg(LNode.Call(CodeSymbols.Break));

        return SyntaxTree.Case(condition, body).WithRange(keywordToken, parser.Iterator.Prev);
    }

    private static LNode ParseDefault(Parser parser, bool autoBreak)
    {
        parser.Iterator.Match(TokenType.Default);

        parser.Iterator.Match(TokenType.Colon);

        var body = Statement.ParseOneOrBlock(parser);

        if (autoBreak)
            body = body.PlusArg(LNode.Call(CodeSymbols.Break));

        return SyntaxTree.Case(LNode.Call(CodeSymbols.Default), body);
    }

    private static LNode ParseIf(Parser parser, bool autoBreak)
    {
        parser.Iterator.Match(TokenType.If);

        var condition = Expression.Parse(parser);

        parser.Iterator.Match(TokenType.Colon);

        var body = Statement.ParseOneOrBlock(parser);

        if (autoBreak)
            body = body.PlusArg(LNode.Call(CodeSymbols.Break));

        return SyntaxTree.If(condition, body, SyntaxTree.Factory.Braces());
    }

    private static LNode ParseWhen(Parser parser, bool autoBreak)
    {
        parser.Iterator.Match(TokenType.When);

        LNode binOp = LNode.Missing;
        LNode right = LNode.Missing;

        if (Expression.GetBinaryOperatorPrecedence(parser.Iterator.Current.Type) > 0)
        {
            // with binary expression
            var operatorToken = parser.Iterator.NextToken();

            right = Expression.Parse(parser);
            binOp = LNode.Call(GSymbol.Get($"'{operatorToken.Text}"));
        }
        else
        {
            // with element function
            if (!parser.Iterator.IsMatch(TokenType.Identifier))
            {
                var range = new SourceRange(parser.Document, parser.Iterator.Current.Start, parser.Iterator.Current.Text.Length);

                parser.AddError(new(Core.ErrorID.ExpectedIdentifier, parser.Iterator.Current.Text), range);

                return LNode.Missing;
            }
            var name = LNode.Id(parser.Iterator.Current.Text);
            parser.Iterator.Match(TokenType.Identifier);
            parser.Iterator.Match(TokenType.OpenParen);
            var args = Expression.ParseList(parser, TokenType.CloseParen);

            right = LNode.Call(name, args);
        }

        parser.Iterator.Match(TokenType.Colon);

        var body = Statement.ParseOneOrBlock(parser);

        if (autoBreak)
            body = body.PlusArg(LNode.Call(CodeSymbols.Break));

        return SyntaxTree.When(binOp, right, body);
    }
}