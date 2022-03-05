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

            /*if (keyword.Type == TokenType.Set)
            {
                cu.Body.Body.Add(ParseVariableAssignment());
            }
            else if (keyword.Type == TokenType.Call)
            {
                NextToken();

                cu.Body.Body.Add(ParseIdentifierListOrCall());

                Match(TokenType.Dot);
            }
            else
            {
                cu.Body.Body.Add(ParseExpressionStatement());
            }
            */
        }

        cu.Messages = Messages;

        return cu;
    }

    private SyntaxNode ParseExpressionStatement()
    {
        var expr = Expression.Parse(this);

        Match(TokenType.Dot);

        return new ExpressionStatement(expr);
    }
}