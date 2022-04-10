namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class ImplementationDeclaration : SyntaxNode, IParsePoint<SyntaxNode>
{
    public ImplementationDeclaration(SyntaxNode target, Block body)
    {
        Target = target;
        Body = body;
    }

    public Block Body { get; set; }
    public SyntaxNode Target { get; set; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        if (iterator.Current.Type == TokenType.For || iterator.Current.Type == TokenType.Of)
        {
            iterator.NextToken();

            SyntaxNode target = null;

            if (iterator.Peek(1).Type == TokenType.RangeOperator)
            {
                target = Expression.Parse(parser);
            }
            else
            {
                target = TypeLiteral.Parse(iterator, parser);
            }

            iterator.Match(TokenType.OpenCurly);

            Block body = new();
            while (iterator.Current.Type != TokenType.EOF && iterator.Current.Type != TokenType.CloseCurly)
            {
                body.Body.Add(parser.InvokeParsePoint(parser.DeclarationParsePoints));
            }

            iterator.Match(TokenType.CloseCurly);

            return new ImplementationDeclaration(target, body);
        }

        parser.Messages.Add(
            Message.Error(parser.Document,
                "Expected For Or Of", iterator.Current.Line, iterator.Current.Column));

        return new InvalidNode();
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}