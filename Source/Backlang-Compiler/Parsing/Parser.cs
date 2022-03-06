using Backlang_Compiler.Core;
using Backlang_Compiler.Parsing.AST;
using Backlang_Compiler.Parsing.AST.Statements;

namespace Backlang_Compiler.Parsing;

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