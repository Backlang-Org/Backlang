using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public class VariableStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;

        bool isMutable = false;
        LNode type = SyntaxTree.Type("", LNode.List());
        LNode value = LNode.Missing;

        Token mutableToken = null;

        if (iterator.Current.Type == TokenType.Mutable)
        {
            isMutable = true;
            mutableToken = iterator.NextToken();
        }

        var nameToken = iterator.Match(TokenType.Identifier);
        var name = SyntaxTree.Factory.Id(nameToken.Text).WithRange(nameToken);

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

        var node = SyntaxTree.Factory.Var(type, name, value).WithRange(keywordToken, iterator.Prev);

        return isMutable ? node.WithAttrs(SyntaxTree.Factory.Id(Symbols.Mutable).WithRange(mutableToken)) : node;
    }
}