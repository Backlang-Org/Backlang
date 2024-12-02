using Loyc;
using Loyc.Syntax;
using System.Xml;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class DocComment
{
    public static LNode Parse(Parser parser)
    {
        var comment = parser.Iterator.Match(TokenType.DocComment);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml($"<root>{comment.Text}</root>");

        return LNode.Trivia(Symbol.For("DocComment"), xmlDocument);
    }

    public static bool TryParse(Parser parser, out LNode node)
    {
        var result = parser.Iterator.IsMatch(TokenType.DocComment);

        node = result ? Parse(parser) : LNode.Missing;

        return result;
    }
}