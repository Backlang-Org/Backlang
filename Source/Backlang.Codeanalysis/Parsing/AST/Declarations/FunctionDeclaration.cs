﻿using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class FunctionDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        string name = null;
        if (iterator.Current.Type == TokenType.Identifier)
        {
            name = iterator.Current.Text;
            iterator.NextToken();
        }
        else
        {
            //error
            parser.Messages.Add(Message.Error(parser.Document,
                $"Expected Identifier, got {iterator.Current.Text}", iterator.Current.Line, iterator.Current.Column));
        }

        LNode returnType = LNode.Missing;

        LNodeList attributes = new();

        iterator.Match(TokenType.OpenParen);

        var parameters = ParseParameterDeclarations(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        if (iterator.Current.Type == TokenType.Static)
        {
            iterator.NextToken();

            attributes.Add(LNode.Id(CodeSymbols.Static));
        }

        if (iterator.Current.Type == TokenType.Private)
        {
            iterator.NextToken();

            attributes.Add(LNode.Id(CodeSymbols.Private));
        }

        if (iterator.Current.Type == TokenType.Operator)
        {
            iterator.NextToken();

            attributes.Add(LNode.Id(CodeSymbols.Operator));
        }

        if (iterator.Current.Type == TokenType.Arrow)
        {
            iterator.NextToken();

            returnType = TypeLiteral.Parse(iterator, parser);
        }

        return SyntaxTree.Fn(LNode.Id((Symbol)name), returnType, parameters, Statement.ParseBlock(parser))
            .WithAttrs(attributes);
    }

    private static LNodeList ParseParameterDeclarations(TokenIterator iterator, Parser parser)
    {
        var parameters = new LNodeList();
        while (iterator.Current.Type != TokenType.CloseParen)
        {
            while (iterator.Current.Type != TokenType.Comma && iterator.Current.Type != TokenType.CloseParen)
            {
                var parameter = ParameterDeclaration.Parse(iterator, parser);

                if (iterator.Current.Type == TokenType.Comma)
                {
                    iterator.NextToken();
                }

                parameters.Add(parameter);
            }
        }

        return parameters;
    }
}