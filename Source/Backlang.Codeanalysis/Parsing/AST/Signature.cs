using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class Signature
{
    public static LNode Parse(Parser parser)
    {
        var iterator = parser.Iterator;

        LNode name;
        if (!TypeLiteral.TryParse(parser, out name))
        {
            //error
            parser.Messages.Add(Message.Error(parser.Document,
                $"Expected Identifier, got {iterator.Current.Text}", iterator.Current.Line, iterator.Current.Column));
        }

        LNode returnType = LNode.Missing;
        LNodeList generics = new();

        iterator.Match(TokenType.OpenParen);

        var parameters = ParseParameterDeclarations(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        while (iterator.IsMatch(TokenType.Where))
        {
            iterator.NextToken();
            var genericName = LNode.Id(iterator.Match(TokenType.Identifier).Text);
            iterator.Match(TokenType.Colon);
            var bases = new LNodeList();
            do
            {
                if (iterator.IsMatch(TokenType.Comma)) iterator.NextToken();
                bases.Add(TypeLiteral.Parse(iterator, parser));
            } while (iterator.IsMatch(TokenType.Comma));

            generics.Add(LNode.Call(Symbols.Where, LNode.List(genericName, LNode.Call(CodeSymbols.Base, bases))));
        }

        if (iterator.IsMatch(TokenType.Arrow))
        {
            iterator.NextToken();

            returnType = TypeLiteral.Parse(iterator, parser);
        }

        return SyntaxTree.Signature(name, returnType, parameters, generics);
    }

    public static LNodeList ParseParameterDeclarations(TokenIterator iterator, Parser parser)
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