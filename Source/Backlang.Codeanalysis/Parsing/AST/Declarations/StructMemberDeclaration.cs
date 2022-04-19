using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class StructMemberDeclaration
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var nameToken = iterator.Match(TokenType.Identifier);
        LNode value = LNode.Missing;

        iterator.Match(TokenType.Colon);

        var type = TypeLiteral.Parse(iterator, parser);

        if (iterator.Current.Type == TokenType.EqualsToken)
        {
            iterator.NextToken();

            value = Expression.Parse(parser);
        }

        iterator.Match(TokenType.Semicolon);

        return SyntaxTree.Factory.Var(type, nameToken.Text, value);
    }
}