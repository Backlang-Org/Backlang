using Backlang.Codeanalysis.Parsing;

namespace Backlang.Codeanalysis.Core;

public abstract class BaseParser<TNode, TLexer, TParser>
    where TParser : BaseParser<TNode, TLexer, TParser>
    where TLexer : BaseLexer, new()
{
    public readonly List<Message> Messages;

    public BaseParser(SourceDocument document, List<Token> tokens, List<Message> messages)
    {
        Document = document;
        Iterator = new(tokens);
        Messages = messages;
    }

    public SourceDocument Document { get; }
    public TokenIterator Iterator { get; set; }

    public static (TNode? Tree, List<Message> Messages) Parse(SourceDocument document)
    {
        if (string.IsNullOrEmpty(document.Source) || document.Source == null)
        {
            return (default, new() { Message.Error("Empty File", 0, 0) });
        }

        var lexer = new TLexer();
        var tokens = lexer.Tokenize(document.Source);

        var parser = (TParser)Activator.CreateInstance(typeof(TParser), document, tokens, lexer.Messages);

        return (parser.Program(), parser.Messages);
    }

    public TNode Program()
    {
        var node = Start();

        Iterator.Match(TokenType.EOF);

        return node;
    }

    internal abstract Expression ParsePrimary(ParsePoints<Expression> parsePoints = null);

    protected abstract TNode Start();
}