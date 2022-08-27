using Backlang.Codeanalysis.Parsing.AST;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public sealed partial class Parser
{
    public readonly List<Message> Messages;

    public Parser(SourceFile<StreamCharSource> document, List<Token> tokens, List<Message> messages)
    {
        Document = document;
        Iterator = new(tokens, document);
        Messages = messages;

        InitParsePoints();
    }

    public SourceFile<StreamCharSource> Document { get; }

    public TokenIterator Iterator { get; set; }

    public static (LNodeList Tree, List<Message> Messages) Parse(SourceDocument src)
    {
        SourceFile<StreamCharSource> document = src;

        if (document.Text == null)
        {
            return (LNode.List(LNode.Missing), new() { Message.Error("Empty File", SourceRange.Synthetic) });
        }

        var lexer = new Lexer();
        var tokens = lexer.Tokenize(document);

        var parser = new Parser(document, tokens, lexer.Messages);

        return parser.Program();
    }

    public (LNodeList, List<Message>) Program()
    {
        var node = Start();

        Iterator.Match(TokenType.EOF);

        return (node.Body, node.Messages);
    }

    private CompilationUnit Start()
    {
        var cu = new CompilationUnit();

        var body = InvokeDeclarationParsePoints();

        cu.Messages = Messages.Concat(Iterator.Messages).ToList();
        cu.Body = body;

        return cu;
    }
}