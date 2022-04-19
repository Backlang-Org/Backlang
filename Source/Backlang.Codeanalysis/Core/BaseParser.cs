using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Core;

public abstract class BaseParser<TLexer, TParser>
    where TParser : BaseParser<TLexer, TParser>
    where TLexer : BaseLexer, new()
{
    public readonly List<Message> Messages;

    protected BaseParser(SourceDocument document, List<Token> tokens, List<Message> messages)
    {
        Document = document;
        Iterator = new(tokens, document);
        Messages = messages;
    }

    public SourceDocument Document { get; }
    public TokenIterator Iterator { get; set; }

    public static (LNodeList Tree, List<Message> Messages) Parse(SourceDocument document)
    {
        if (string.IsNullOrEmpty(document.Source) || document.Source == null)
        {
            return (LNode.List(LNode.Missing), new() { Message.Error(document, "Empty File", 0, 0) });
        }

        var lexer = new TLexer();
        var tokens = lexer.Tokenize(document);

        var parser = (TParser)Activator.CreateInstance(typeof(TParser), document, tokens, lexer.Messages);

        return (parser.Program(), parser.Messages);
    }

    public LNodeList Program()
    {
        var node = Start();

        Iterator.Match(TokenType.EOF);

        return node.Body;
    }

    internal abstract LNode ParsePrimary(ParsePoints<LNode> parsePoints = null);

    protected abstract CompilationUnit Start();
}