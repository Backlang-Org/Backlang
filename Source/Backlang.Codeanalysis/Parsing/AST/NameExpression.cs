namespace Backlang.Codeanalysis.Parsing.AST;

public class NameExpression : Expression, IParsePoint<Expression>
{
    public NameExpression(string name, int line, int column)
    {
        Name = name;
        Line = line;
        Column = column;
    }

    public int Column { get; set; }
    public int Line { get; set; }
    public string Name { get; set; }

    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        var nameExpression = new NameExpression(iterator.Peek(-1).Text, iterator.Peek(-1).Line, iterator.Peek(-1).Column);

        if (iterator.Current.Type == TokenType.OpenSquare)
        {
            iterator.NextToken();

            var arrayAccess = new ArrayAccessExpression();
            arrayAccess.Name = nameExpression;

            while (iterator.Current.Type != TokenType.CloseSquare)
            {
                arrayAccess.Indices.Add(Expression.Parse(parser));

                if (iterator.Current.Type != TokenType.CloseSquare)
                {
                    iterator.Match(TokenType.Comma);
                }
            }

            iterator.Match(TokenType.CloseSquare);

            return arrayAccess;
        }
        else
        {
            return nameExpression;
        }
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}