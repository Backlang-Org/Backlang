namespace Backlang.Codeanalysis.Parsing;

public enum MessageSeverity
{
    Error, Warning, Info, Hint
}

public class Message
{
    public Message(SourceDocument document, MessageSeverity severity, string text, int line, int column)
    {
        Document = document;
        Severity = severity;
        Text = text;
        Column = column;
        Line = line;
    }

    public int Column { get; set; }
    public SourceDocument Document { get; }
    public int Line { get; set; }
    public MessageSeverity Severity { get; set; }
    public string Text { get; set; }

    public static Message Error(SourceDocument document, string message, int line, int column)
    {
        return new Message(document, MessageSeverity.Error, message, line, column);
    }

    public static Message Info(SourceDocument document, string message, int line, int column)
    {
        return new Message(document, MessageSeverity.Info, message, line, column);
    }

    public static Message Warning(SourceDocument document, string message, int line, int column)
    {
        return new Message(document, MessageSeverity.Warning, message, line, column);
    }

    public override string ToString()
    {
        return $"{Document.Filename}:{Line}:{Column} {Text}";
    }
}