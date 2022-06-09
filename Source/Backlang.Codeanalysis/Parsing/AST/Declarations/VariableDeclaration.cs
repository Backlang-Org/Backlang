using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class VariableDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Peek(-1);

        bool isMutable = false;
        LNode type = LNode.Missing;
        LNode value = LNode.Missing;

        if (iterator.Current.Type == TokenType.Mutable)
        {
            isMutable = true;
            iterator.NextToken();
        }

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

        var node = SyntaxTree.Factory.Var(type, name, value).WithRange(keywordToken, iterator.Peek(-1));

        return isMutable ? node.WithAttrs(LNode.Id(Symbols.Mutable)) : node;
    }
}