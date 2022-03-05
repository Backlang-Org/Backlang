namespace Backlang_Compiler.Parsing.AST;

public class CompilationUnit : SyntaxNode
{
    public Block Body { get; set; } = new Block();
    public List<Message> Messages { get; set; } = new List<Message>();

    public static CompilationUnit FromFile(string filename)
    {
        var document = new SourceDocument(filename);

        var result = Parser.Parse(document);

        return (CompilationUnit)result.Tree;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}