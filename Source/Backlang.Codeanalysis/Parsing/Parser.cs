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
        while (Current.Type != (TokenType.EOF))
        {
            var keyword = Current;

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

        Match(TokenType.Semicolon);

        return new ExpressionStatement(expr);
    }

    private SyntaxNode ParseFunctionDeclaration()
    {
        NextToken();

        var name = Match(TokenType.Identifier);
        Token returnTypeToken = null;

        Match(TokenType.OpenParen);

        var parameters = ParseParameterDeclarations();

        Match(TokenType.CloseParen);

        if (Current.Type == TokenType.Arrow)
        {
            NextToken();

            returnTypeToken = Match(TokenType.Identifier);
        }

        Match(TokenType.OpenCurly);

        var body = new Block();
        while (Current.Type != (TokenType.CloseCurly))
        {
            var keyword = Current;

            if (keyword.Type == TokenType.Declare)
            {
                body.Body.Add(ParseVariableDeclaration());
            }
            else
            {
                body.Body.Add(ParseExpressionStatement());
            }
        }

        Match(TokenType.CloseCurly);

        return new FunctionDeclaration(name, returnTypeToken, parameters, body);
    }

    private ParameterDeclaration ParseParameterDeclaration()
    {
        var name = Match(TokenType.Identifier);

        Match(TokenType.Colon);

        var type = Match(TokenType.Identifier);

        Expression? defaultValue = null;

        if (Current.Type == TokenType.EqualsToken)
        {
            NextToken();

            defaultValue = Expression.Parse(this);
        }

        return new ParameterDeclaration(name, type, defaultValue);
    }

    private List<ParameterDeclaration> ParseParameterDeclarations()
    {
        var parameters = new List<ParameterDeclaration>();
        while (Current.Type != TokenType.CloseParen)
        {
            // (id : type)
            // ()
            // (id : type = defaultvalue)

            while (Current.Type != TokenType.Comma && Current.Type != TokenType.CloseParen)
            {
                var parameter = ParseParameterDeclaration();

                if (Current.Type == TokenType.Comma)
                {
                    NextToken();
                }

                parameters.Add(parameter);
            }
        }

        return parameters;
    }

    private SyntaxNode ParseVariableDeclaration()
    {
        NextToken();

        var nameToken = Match(TokenType.Identifier);
        Token? typeToken = null;
        Expression? value = null;

        if (Current.Type == TokenType.Colon)
        {
            NextToken();

            typeToken = NextToken();
        }

        if (Current.Type == TokenType.EqualsToken)
        {
            NextToken();

            value = Expression.Parse(this);
        }

        Match(TokenType.Semicolon);

        return new VariableDeclarationStatement(nameToken, typeToken, value);
    }
}