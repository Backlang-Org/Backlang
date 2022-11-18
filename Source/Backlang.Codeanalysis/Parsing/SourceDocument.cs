using Loyc.Syntax;
using System.Text;

namespace Backlang.Codeanalysis.Parsing;

public sealed class SourceDocument
{
    private readonly SourceFile<StreamCharSource> _document;

    public SourceDocument(string filename)
    {
        _document = new SourceFile<StreamCharSource>(new(File.OpenRead(filename)), filename);
    }

    public SourceDocument(string filename, string content)
    {
        var filebody = Encoding.Default.GetBytes(content);

        _document = new SourceFile<StreamCharSource>(new(new MemoryStream(filebody)), filename);
    }

    public static implicit operator SourceFile<StreamCharSource>(SourceDocument doc)
    {
        SyntaxTree.Factory = new(doc._document);

        return doc._document;
    }
}