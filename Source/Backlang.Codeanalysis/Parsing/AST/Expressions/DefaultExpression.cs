﻿using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class DefaultExpression : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //default(i32)
        //default
        if (iterator.ConsumeIfMatch(TokenType.OpenParen))
        {
            var type = TypeLiteral.Parse(iterator, parser);

            iterator.Match(TokenType.CloseParen);

            return SyntaxTree.Default(type);
        }

        return SyntaxTree.Default();
    }
}