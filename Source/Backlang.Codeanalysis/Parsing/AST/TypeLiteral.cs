namespace Backlang.Codeanalysis.Parsing.AST;

public class TypeLiteral : SyntaxNode
{
    public string[] Arguments { get; set; }
    public int Dimensions { get; set; }
    public bool IsArrayType => Dimensions > 0;
    public bool IsPointer { get; set; }

    public string Typename { get; set; }

    public static TypeLiteral Parse(TokenIterator iterator)
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

        return literal;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}