using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypePropertyDeclaration
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Match(TokenType.Property);
        LNode type = LNode.Missing;
        LNode value = LNode.Missing;
        var nameToken = iterator.Match(TokenType.Identifier);
        var name = LNode.Id(nameToken.Text);

        if (iterator.Current.Type == TokenType.Colon)
        {
            iterator.NextToken();

            type = TypeLiteral.Parse(iterator, parser);
        }

        if (iterator.Current.Type == TokenType.EqualsToken)
        {
            iterator.NextToken();

            value = Expression.Parse(parser);
        }

        iterator.Match(TokenType.Semicolon);

        return SyntaxTree.Property(type, name, value).WithRange(keywordToken, iterator.Prev);
    }
}