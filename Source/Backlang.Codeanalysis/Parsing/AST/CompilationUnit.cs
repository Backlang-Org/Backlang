using Loyc.Syntax;
using System.Text;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class CompilationUnit
{
    public LNodeList Body { get; set; } = new();
    public SourceFile<StreamCharSource> Document { get; private set; }
    public List<Message> Messages { get; set; } = new List<Message>();

    public static CompilationUnit FromFile(string filename)
    {
        var filebody = File.ReadAllBytes(filename);

        var document = new SourceFile<StreamCharSource>(new(new MemoryStream(filebody)), filename);

        SyntaxTree.Factory = new(document);

        var result = Parser.Parse(document);

        return new CompilationUnit { Body = result.Tree, Messages = result.Messages, Document = document };
    }

    public static CompilationUnit FromText(string text)
    {
        var body = Encoding.Default.GetBytes(text);
        var document = new SourceFile<StreamCharSource>(new(new MemoryStream(body)), "inline.txt");

        var result = Parser.Parse(document);

        return new CompilationUnit { Body = result.Tree, Messages = result.Messages, Document = document };
    }
}