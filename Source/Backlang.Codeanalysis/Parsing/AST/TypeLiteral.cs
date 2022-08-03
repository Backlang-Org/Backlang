using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class TypeLiteral
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        LNode typeNode;

        if (iterator.IsMatch(TokenType.Identifier))
        {
            var typename = iterator.Match(TokenType.Identifier).Text;
            var args = new LNodeList();

            typeNode = SyntaxTree.Type(typename, new());

            if (iterator.IsMatch(TokenType.Star))
            {
                iterator.NextToken();

                typeNode = SyntaxTree.Pointer(typeNode);
            }
            else if (iterator.IsMatch(TokenType.OpenSquare))
            {
                iterator.NextToken();

                var dimensions = 1;

                while (iterator.IsMatch(TokenType.Comma))
                {
                    dimensions++;

                    iterator.NextToken();
                }

                iterator.Match(TokenType.CloseSquare);

                typeNode = SyntaxTree.Array(typeNode, dimensions);
            }
            else if (iterator.IsMatch(TokenType.LessThan))
            {
                iterator.NextToken();

                while (!iterator.IsMatch(TokenType.GreaterThan))
                {
                    if (iterator.IsMatch(TokenType.Identifier))
                    {
                        args.Add(Parse(iterator, parser));
                    }

                    if (!iterator.IsMatch(TokenType.GreaterThan))
                    {
                        iterator.Match(TokenType.Comma);
                    }
                }

                iterator.Match(TokenType.GreaterThan);

                typeNode = typeNode.PlusArgs(args);
            }
        }
        else if (iterator.IsMatch(TokenType.None))
        {
            typeNode = SyntaxTree.Type("none", LNode.List()); // Missing is the normal type for none
            iterator.NextToken();
        }
        else if (iterator.IsMatch(TokenType.OpenParen))
        {
            LNode returnType = LNode.Missing;

            iterator.Match(TokenType.OpenParen);

            var parameters = new LNodeList();
            while (parser.Iterator.Current.Type != TokenType.CloseParen)
            {
                parameters.Add(TypeLiteral.Parse(iterator, parser));

                if (parser.Iterator.Current.Type != TokenType.CloseParen)
                {
                    parser.Iterator.Match(TokenType.Comma);
                }
            }

            parser.Iterator.Match(TokenType.CloseParen);

            if (iterator.Current.Type == TokenType.Arrow)
            {
                iterator.NextToken();

                returnType = TypeLiteral.Parse(iterator, parser);
            }

            typeNode = LNode.Call(CodeSymbols.Fn, LNode.List(returnType, LNode.Missing, LNode.Call(CodeSymbols.AltList, parameters)));
        }
        else
        {
            parser.Messages.Add(Message.Error(parser.Document, "Expected Identifier or Function-Signature as TypeLiteral, but got " + iterator.Current.Type, parser.Iterator.Current.Line, parser.Iterator.Current.Column));
            typeNode = LNode.Missing;
            iterator.NextToken();
        }

        return typeNode;
    }

    public static bool TryParse(Parser parser, out LNode node)
    {
        var cursor = parser.Iterator.Position;
        node = Parse(parser.Iterator, parser);

        if (node == LNode.Missing)
        {
            parser.Iterator.Position = cursor;
            return false;
        }

        return true;
    }
}