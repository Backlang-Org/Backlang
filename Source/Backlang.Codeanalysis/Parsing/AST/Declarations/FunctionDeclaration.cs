namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class FunctionDeclaration : SyntaxNode, IParsePoint<SyntaxNode>
{
    public FunctionDeclaration(Token name, TypeLiteral returnType, List<ParameterDeclaration> parameters, Block body)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Body = body;
    }

    public Block Body { get; }
    public Token Name { get; }
    public List<ParameterDeclaration> Parameters { get; }
    public TypeLiteral ReturnType { get; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        var name = iterator.Match(TokenType.Identifier);
        TypeLiteral returnType = null;

        iterator.Match(TokenType.OpenParen);

        var parameters = ParseParameterDeclarations(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        if (iterator.Current.Type == TokenType.Arrow)
        {
            iterator.NextToken();

            returnType = TypeLiteral.Parse(iterator);
        }

        iterator.Match(TokenType.OpenCurly);

        var body = new Block();
        while (iterator.Current.Type != (TokenType.CloseCurly))
        {
            var keyword = iterator.Current;

            body.Body.Add(parser.InvokeStatementParsePoint());
        }

        iterator.Match(TokenType.CloseCurly);

        return new FunctionDeclaration(name, returnType, parameters, body);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    private static List<ParameterDeclaration> ParseParameterDeclarations(TokenIterator iterator, Parser parser)
    {
        var parameters = new List<ParameterDeclaration>();
        while (iterator.Current.Type != TokenType.CloseParen)
        {
            while (iterator.Current.Type != TokenType.Comma && iterator.Current.Type != TokenType.CloseParen)
            {
                var parameter = ParameterDeclaration.Parse(iterator, parser);

                if (iterator.Current.Type == TokenType.Comma)
                {
                    iterator.NextToken();
                }

                parameters.Add((ParameterDeclaration)parameter);
            }
        }

        return parameters;
    }
}