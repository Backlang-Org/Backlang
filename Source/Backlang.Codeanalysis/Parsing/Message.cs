using Backlang.Codeanalysis.Core;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public enum MessageSeverity
{
    Error, Warning, Info, Hint
}

public sealed class Message
{
    public Message(MessageSeverity severity, string text, SourceRange range)
    {
        if (range.Source is SourceFile<StreamCharSource> doc)
        {
            Document = doc;
        }

        Severity = severity;
        Text = text;
        Range = range;
    }

    public SourceRange Range { get; set; }

    public SourceFile<StreamCharSource> Document { get; }

    public MessageSeverity Severity { get; set; }
    public string Text { get; set; }

    public static Message Error(LocalizableString message, SourceRange range)
    {
        return new Message(MessageSeverity.Error, message, range);
    }

    public static Message Error(LocalizableString message)
    {
        return new Message(MessageSeverity.Error, message, SourceRange.Synthetic);
    }

    public static Message Info(string message, SourceRange range)
    {
        return new Message(MessageSeverity.Info, message, range);
    }

    public static Message Warning(string message, SourceRange range)
    {
        return new Message(MessageSeverity.Warning, message, range);
    }

    public override string ToString()
    {
        if (Document == null)
        {
            return Text;
        }

        return $"{Document.FileName}:{Range.Start.Line}:{Range.Start.Column} {Text}";
    }
}