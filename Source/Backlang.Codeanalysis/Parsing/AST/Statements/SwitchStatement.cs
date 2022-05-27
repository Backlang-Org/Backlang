﻿using Loyc.Syntax;

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
            bool autoBreak = iterator.IsMatch(TokenType.Break);

            if (autoBreak) iterator.Match(TokenType.Break);

            if (iterator.IsMatch(TokenType.Case))
                cases.Add(ParseCase(parser, autoBreak));
            else if (iterator.IsMatch(TokenType.If))
                cases.Add(ParseIf(parser, autoBreak));
            else if (iterator.IsMatch(TokenType.Default))
                cases.Add(ParseDefault(parser, autoBreak));
            else
            {
                parser.Messages.Add(Message.Error(parser.Document, "Switch Statement can only have case, if or default, but got " + iterator.Current.Text, iterator.Current.Line, iterator.Current.Column));
                return LNode.Missing;
            }
        }

        parser.Iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Switch(element, cases);
    }

    private static LNode ParseCase(Parser parser, bool autoBreak)
    {
        parser.Iterator.Match(TokenType.Case);

        var condition = Expression.Parse(parser);

        parser.Iterator.Match(TokenType.Colon);

        var body = Statement.ParseOneOrBlock(parser);

        if (autoBreak)
            body = body.Add(LNode.Call(CodeSymbols.Break));

        return SyntaxTree.Case(condition, body);
    }

    private static LNode ParseIf(Parser parser, bool autoBreak)
    {
        parser.Iterator.Match(TokenType.If);

        var condition = Expression.Parse(parser);

        parser.Iterator.Match(TokenType.Colon);

        var body = Statement.ParseOneOrBlock(parser);

        if (autoBreak)
            body = body.Add(LNode.Call(CodeSymbols.Break));

        return SyntaxTree.If(condition, body, LNode.List());
    }

    private static LNode ParseDefault(Parser parser, bool autoBreak)
    {
        parser.Iterator.Match(TokenType.Default);

        parser.Iterator.Match(TokenType.Colon);

        var body = Statement.ParseOneOrBlock(parser);

        if(autoBreak)
            body = body.Add(LNode.Call(CodeSymbols.Break));

        return SyntaxTree.Case(LNode.Call(CodeSymbols.Default), body);
    }

}