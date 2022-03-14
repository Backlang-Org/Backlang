namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class StructDeclaration : IParsePoint<SyntaxNode>
{
    public List<object> Members { get; set; } = new();
    public string Name { get; set; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        throw new NotImplementedException();
    }
}