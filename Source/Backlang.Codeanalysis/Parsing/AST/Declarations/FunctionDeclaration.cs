using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class FunctionDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var name = iterator.Match(TokenType.Identifier);
        TypeLiteral returnType = null;

        LNodeList attributes = new();

        iterator.Match(TokenType.OpenParen);

        var parameters = ParseParameterDeclarations(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        if (iterator.Current.Type == TokenType.Static)
        {
            iterator.NextToken();

            attributes.Add(LNode.Id(CodeSymbols.Static));
        }

        if (iterator.Current.Type == TokenType.Arrow)
        {
            iterator.NextToken();

            returnType = TypeLiteral.Parse(iterator, parser);
        }

        return SyntaxTree.Fn(LNode.Id((Symbol)name.Text), returnType, parameters, Statement.ParseBlock(parser))
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