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

            if (keyword.Type == TokenType.Function)
            {
                cu.Body.Body.Add(FunctionDeclaration.Parse(Iterator, this));
            }
            else if (keyword.Type == TokenType.Enum)
            {
                cu.Body.Body.Add(EnumDeclaration.Parse(Iterator, this));
            }
            else
            {
                cu.Body.Body.Add(ExpressionStatement.Parse(Iterator, this));
            }
        }

        cu.Messages = Messages.Concat(Iterator.Messages).ToList();

        return cu;
    }
}