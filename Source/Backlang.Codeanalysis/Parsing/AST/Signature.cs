using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class Signature
{
    public static LNode Parse(Parser parser)
    {
        var iterator = parser.Iterator;

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

        iterator.Match(TokenType.OpenParen);

        var parameters = ParseParameterDeclarations(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        LNodeList attributes = ParseAttributes(parser);

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

        return SyntaxTree.Signature(LNode.Id((Symbol)name), returnType, parameters)
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

    public static LNodeList ParseAttributes(Parser parser)
    {
        LNodeList attributes = new();

        if (parser.Iterator.Current.Type == TokenType.Static)
        {
            parser.Iterator.NextToken();

            attributes.Add(LNode.Id(CodeSymbols.Static));
        }

        if (parser.Iterator.Current.Type == TokenType.Private)
        {
            parser.Iterator.NextToken();

            attributes.Add(LNode.Id(CodeSymbols.Private));
        }

        return attributes;
    }
}