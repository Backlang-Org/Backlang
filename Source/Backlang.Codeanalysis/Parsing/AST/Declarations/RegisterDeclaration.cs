using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class RegisterDeclaration : IParsePoint<LNode>
{
    public LNode Address { get; set; }
    public string Name { get; set; }

    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var node = new RegisterDeclaration();

        node.Name = iterator.Current.Text;

        iterator.NextToken();

        iterator.Match(TokenType.EqualsToken);

        node.Address = Expression.Parse(parser);

        iterator.Match(TokenType.Semicolon);

        return node;
    }
}