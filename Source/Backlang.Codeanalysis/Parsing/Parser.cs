using Backlang.Codeanalysis.Core;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Backlang.Codeanalysis.Parsing.AST.Statements;

namespace Backlang.Codeanalysis.Parsing;

public partial class Parser : BaseParser<SyntaxNode, Lexer, Parser>
{
    public Parser(SourceDocument document, List<Token> tokens, List<Message> messages) : base(document, tokens, messages)
    {
    }

    protected override SyntaxNode Start()
    {
        var cu = new CompilationUnit();
        while (Iterator.Current.Type != (TokenType.EOF))
        {
            var keyword = Iterator.Current;

            if (keyword.Type == TokenType.Declare)
            {
                cu.Body.Body.Add(ParseVariableDeclaration());
            }
            else if (keyword.Type == TokenType.Function)
            {
                cu.Body.Body.Add(ParseFunctionDeclaration());
            }
            else
            {
                cu.Body.Body.Add(ParseExpressionStatement());
            }
        }

        cu.Messages = Messages;

        return cu;
    }

    private SyntaxNode ParseExpressionStatement()
    {
        var expr = Expression.Parse(this);

        Iterator.Match(TokenType.Semicolon);

        return new ExpressionStatement(expr);
    }

    private SyntaxNode ParseFunctionDeclaration()
    {
        Iterator.NextToken();

        var name = Iterator.Match(TokenType.Identifier);
        Token returnTypeToken = null;

        Iterator.Match(TokenType.OpenParen);

        var parameters = ParseParameterDeclarations();

        Iterator.Match(TokenType.CloseParen);

        if (Iterator.Current.Type == TokenType.Arrow)
        {
            Iterator.NextToken();

            returnTypeToken = Iterator.Match(TokenType.Identifier);
        }

        Iterator.Match(TokenType.OpenCurly);

        var body = new Block();
        while (Iterator.Current.Type != (TokenType.CloseCurly))
        {
            var keyword = Iterator.Current;

            if (keyword.Type == TokenType.Declare)
            {
                body.Body.Add(ParseVariableDeclaration());
            }
            else
            {
                body.Body.Add(ParseExpressionStatement());
            }
        }

        Iterator.Match(TokenType.CloseCurly);

        return new FunctionDeclaration(name, returnTypeToken, parameters, body);
    }

    private ParameterDeclaration ParseParameterDeclaration()
    {
        var name = Iterator.Match(TokenType.Identifier);

        Iterator.Match(TokenType.Colon);

        var type = Iterator.Match(TokenType.Identifier);

        Expression? defaultValue = null;

        if (Iterator.Current.Type == TokenType.EqualsToken)
        {
            Iterator.NextToken();

            defaultValue = Expression.Parse(this);
        }

        return new ParameterDeclaration(name, type, defaultValue);
    }

    private List<ParameterDeclaration> ParseParameterDeclarations()
    {
        var parameters = new List<ParameterDeclaration>();
        while (Iterator.Current.Type != TokenType.CloseParen)
        {
            while (Iterator.Current.Type != TokenType.Comma && Iterator.Current.Type != TokenType.CloseParen)
            {
                var parameter = ParseParameterDeclaration();

                if (Iterator.Current.Type == TokenType.Comma)
                {
                    Iterator.NextToken();
                }

                parameters.Add(parameter);
            }
        }

        return parameters;
    }

    private SyntaxNode ParseVariableDeclaration()
    {
        Iterator.NextToken();

        var nameToken = Iterator.Match(TokenType.Identifier);
        Token? typeToken = null;
        Expression? value = null;

        if (Iterator.Current.Type == TokenType.Colon)
        {
            Iterator.NextToken();

            typeToken = Iterator.NextToken();
        }

        if (Iterator.Current.Type == TokenType.EqualsToken)
        {
            Iterator.NextToken();

            value = Expression.Parse(this);
        }

        Iterator.Match(TokenType.Semicolon);

        return new VariableDeclarationStatement(nameToken, typeToken, value);
    }
}