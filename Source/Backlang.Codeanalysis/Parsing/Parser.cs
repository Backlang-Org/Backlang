using Backlang.Codeanalysis.Core;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;

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
            if (Iterator.Current.Type == TokenType.Function)
            {
                cu.Body.Body.Add(FunctionDeclaration.Parse(Iterator, this));
            }
            else if (Iterator.Current.Type == TokenType.Enum)
            {
                cu.Body.Body.Add(EnumDeclaration.Parse(Iterator, this));
            }
            else
            {
                Messages.Add(Message.Error($"Expected 'fn' or 'enum', got '{Iterator.Current.Type}'", Iterator.Current.Line, Iterator.Current.Column));
                Iterator.NextToken();
            }
        }

        cu.Messages = Messages.Concat(Iterator.Messages).ToList();

        return cu;
    }
}