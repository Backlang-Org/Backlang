using Backlang.Codeanalysis.Core;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class TypeLiteral
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        LNode typeNode;
        var typeToken = iterator.Current;

        if (iterator.IsMatch(TokenType.Identifier))
        {
            var typename = iterator.Match(TokenType.Identifier).Text;
            var args = new LNodeList();

            typeNode = SyntaxTree.Type(typename, new LNodeList()).WithRange(typeToken);

            if (iterator.IsMatch(TokenType.Star))
            {
                iterator.NextToken();

                typeNode = SyntaxTree.Pointer(typeNode).WithRange(typeToken, iterator.Prev);
            }

            if (iterator.IsMatch(TokenType.Ampersand))
            {
                iterator.NextToken();

                typeNode = SyntaxTree.RefType(typeNode).WithRange(typeToken, iterator.Prev);
            }
            else if (iterator.IsMatch(TokenType.Questionmark))
            {
                iterator.NextToken();

                typeNode = SyntaxTree.NullableType(typeNode).WithRange(typeToken, iterator.Prev);
            }
            else if (iterator.IsMatch(TokenType.OpenSquare))
            {
                typeNode = ParseArrayType(iterator, typeNode, typeToken);
            }
            else if (iterator.IsMatch(TokenType.LessThan))
            {
                typeNode = ParseGenericType(iterator, parser, typeToken, typename, args);
            }
        }
        else if (iterator.IsMatch(TokenType.None))
        {
            typeNode = SyntaxTree.Type("none", LNode.List()).WithRange(typeToken);
            iterator.NextToken();
        }
        else if (iterator.IsMatch(TokenType.OpenParen))
        {
            typeNode = ParseFunctionOrTupleType(iterator, parser, typeToken);
        }
        else
        {
            parser.AddError(new LocalizableString(ErrorID.UnexpecedType,
                TokenIterator.GetTokenRepresentation(iterator.Current.Type))); //ToDo: Add Range

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

    private static LNode ParseFunctionOrTupleType(TokenIterator iterator, Parser parser, Token typeToken)
    {
        LNode typeNode;
        iterator.Match(TokenType.OpenParen);

        var parameters = new LNodeList();
        while (parser.Iterator.Current.Type != TokenType.CloseParen)
        {
            parameters.Add(Parse(iterator, parser));

            if (parser.Iterator.Current.Type != TokenType.CloseParen)
            {
                parser.Iterator.Match(TokenType.Comma);
            }
        }

        parser.Iterator.Match(TokenType.CloseParen);

        if (iterator.Current.Type == TokenType.Arrow)
        {
            iterator.NextToken();

            var returnType = Parse(iterator, parser);

            typeNode = SyntaxTree.Factory.Call(CodeSymbols.Fn,
                    LNode.List(returnType, LNode.Missing, LNode.Call(CodeSymbols.AltList, parameters)))
                .WithRange(typeToken, iterator.Prev);
        }
        else
        {
            typeNode = SyntaxTree.Factory.Tuple(parameters).WithRange(typeToken, iterator.Prev);
        }

        return typeNode;
    }

    private static LNode ParseGenericType(TokenIterator iterator, Parser parser, Token typeToken, string typename,
        LNodeList args)
    {
        LNode typeNode;
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

        typeNode = SyntaxTree.Type(typename, args).WithRange(typeToken, parser.Iterator.Prev);
        return typeNode;
    }

    private static LNode ParseArrayType(TokenIterator iterator, LNode typeNode, Token typeToken)
    {
        iterator.NextToken();

        var dimensions = 1;

        while (iterator.IsMatch(TokenType.Comma))
        {
            dimensions++;

            iterator.NextToken();
        }

        iterator.Match(TokenType.CloseSquare);

        typeNode = SyntaxTree.Array(typeNode, dimensions).WithRange(typeToken, iterator.Prev);
        return typeNode;
    }
}