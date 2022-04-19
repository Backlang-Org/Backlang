using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class FunctionDeclaration : IParsePoint<LNode>
{
    public FunctionDeclaration(Token name,
                               TypeLiteral returnType, bool isStatic, bool isPrivate, bool isOperator,
                               List<ParameterDeclaration> parameters,
                               Block body)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Body = body;
        IsStatic = isStatic;
        IsPrivate = isPrivate;
        IsOperator = isOperator;
    }

    public Block Body { get; set; }
    public bool IsOperator { get; }
    public bool IsPrivate { get; }
    public bool IsStatic { get; set; }
    public Token Name { get; }
    public List<ParameterDeclaration> Parameters { get; }
    public TypeLiteral ReturnType { get; }

    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var name = iterator.Match(TokenType.Identifier);
        TypeLiteral returnType = null;
        bool isStatic = false, isPrivate = false, isOperator = false;

        iterator.Match(TokenType.OpenParen);

        var parameters = ParseParameterDeclarations(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        if (iterator.Current.Type == TokenType.Static)
        {
            iterator.NextToken();

            isStatic = true;
        }

        if (iterator.Current.Type == TokenType.Private)
        {
            iterator.NextToken();

            isPrivate = true;
        }

        if (iterator.Current.Type == TokenType.Operator)
        {
            iterator.NextToken();

            isOperator = true;
        }

        if (iterator.Current.Type == TokenType.Arrow)
        {
            iterator.NextToken();

            returnType = TypeLiteral.Parse(iterator, parser);
        }

        return new FunctionDeclaration(name, returnType, isStatic, isPrivate, isOperator, parameters, Statement.ParseBlock(parser));
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