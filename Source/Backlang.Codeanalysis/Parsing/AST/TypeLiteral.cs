namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class TypeLiteral : SyntaxNode
{
    public List<SyntaxNode> Arguments { get; set; } = new();
    public int Dimensions { get; set; }
    public bool IsArrayType => Dimensions > 0;
    public bool IsPointer { get; set; }

    public string Typename { get; set; }

    public static TypeLiteral Parse(TokenIterator iterator, Parser parser)
    {
        var literal = new TypeLiteral();

        var typename = iterator.Match(TokenType.Identifier);
        literal.Typename = typename.Text;

        if (iterator.Current.Type == TokenType.Star)
        {
            literal.IsPointer = true;
            iterator.NextToken();
        }
        else if (iterator.Current.Type == TokenType.OpenSquare)
        {
            iterator.NextToken();

            literal.Dimensions = 1;

            while (iterator.Current.Type == TokenType.Comma)
            {
                literal.Dimensions++;

                iterator.NextToken();
            }

            iterator.Match(TokenType.CloseSquare);
        }
        else if (iterator.Current.Type == TokenType.LessThan)
        {
            iterator.NextToken();

            while (iterator.Current.Type != TokenType.GreaterThan)
            {
                if (iterator.Current.Type == TokenType.Identifier)
                {
                    literal.Arguments.Add(Parse(iterator, parser));
                }

                if (iterator.Current.Type != TokenType.GreaterThan)
                {
                    iterator.Match(TokenType.Comma);
                }
            }

            iterator.Match(TokenType.GreaterThan);
        }

        return literal;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}