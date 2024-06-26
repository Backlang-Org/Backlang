﻿using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class DiscriminatedUnionDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var nametoken = iterator.Match(TokenType.Identifier);
        iterator.Match(TokenType.EqualsToken);

        var types = new LNodeList();

        do
        {
            //ToDo: Refactor To Helper Method: RepeatUntil
            var from = iterator.Current;
            iterator.Match(TokenType.Pipe);
            types.Add(ParseType(iterator, parser).WithRange(from, iterator.Prev));
        } while (iterator.IsMatch(TokenType.Pipe));

        iterator.Match(TokenType.Semicolon);

        return SyntaxTree.DiscriminatedUnion(nametoken, types).WithRange(keywordToken, iterator.Prev);
    }

    public static LNode ParseType(TokenIterator iterator, Parser parser)
    {
        var nameToken = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.OpenParen);

        var parameters = ParseParameterDeclarations(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        return SyntaxTree.DiscriminatedType(nameToken, parameters);
    }

    public static LNodeList ParseParameterDeclarations(TokenIterator iterator, Parser parser)
    {
        var parameters = new LNodeList();
        while (iterator.Current.Type != TokenType.CloseParen)
        {
            while (iterator.Current.Type != TokenType.Comma && iterator.Current.Type != TokenType.CloseParen)
            {
                var isMutable = iterator.ConsumeIfMatch(TokenType.Mutable);

                var parameter = ParameterDeclaration.Parse(iterator, parser);

                if (iterator.Current.Type == TokenType.Comma)
                {
                    iterator.NextToken();
                }

                if (isMutable)
                {
                    parameter = parameter.PlusAttr(SyntaxTree.Factory.Id(Symbols.Mutable));
                }

                parameters.Add(parameter);
            }
        }

        return parameters;
    }
}