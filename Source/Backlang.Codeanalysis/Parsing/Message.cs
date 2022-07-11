using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public enum MessageSeverity
{
    Error, Warning, Info, Hint,
}

public sealed class Message
{
    public Message(SourceFile<StreamCharSource> document, MessageSeverity severity, string text, int line, int column)
    {
        Document = document;
        Severity = severity;
        Text = text;
        Column = column;
        Line = line;
    }

    public int Column { get; set; }
    public SourceFile<StreamCharSource> Document { get; }
    public int Line { get; set; }
    public MessageSeverity Severity { get; set; }
    public string Text { get; set; }

    public static Message Error(SourceFile<StreamCharSource> document, string message, int line, int column)
    {
        return new Message(document, MessageSeverity.Error, message, line, column);
    }

    public static Message Error(string message)
    {
        return Error(null, message, -1, -1);
    }

    public static Message Info(SourceFile<StreamCharSource> document, string message, int line, int column)
    {
        return new Message(document, MessageSeverity.Info, message, line, column);
    }

    public static Message Warning(SourceFile<StreamCharSource> document, string message, int line, int column)
    {
        return new Message(document, MessageSeverity.Warning, message, line, column);
    }

    public override string ToString()
    {
        if (Document == null) return Text;
        return $"{Document.FileName}:{Line}:{Column} {Text}";
    }
}