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

    private static Dictionary<TokenType, Symbol> possibleAttributes = new() {
        { TokenType.Static, CodeSymbols.Static },
        { TokenType.Private, CodeSymbols.Private },
        { TokenType.Operator, CodeSymbols.Operator }
    };
    public static LNodeList ParseAttributes(Parser parser)
    {
        LNodeList attributes = new();

        TokenType current;
        while (possibleAttributes.ContainsKey(current = parser.Iterator.Current.Type))
        {
            var attrib = LNode.Id(possibleAttributes[current]);
            parser.Iterator.NextToken();
            if(attributes.Contains(attrib))
            {
                parser.Messages.Add(Message.Error(parser.Document, $"Modifier '{attrib.Name.Name}' is already applied", parser.Iterator.Current.Line, parser.Iterator.Current.Column));
                continue;
            }
            attributes.Add(attrib);
        }

        return attributes;
    }
}