using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class BitFieldDeclaration : IParsePoint<LNode>
{
    public List<BitFieldMemberDeclaration> Members { get; set; } = new();
    public string Name { get; set; }

    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var declaration = new BitFieldDeclaration();

        declaration.Name = iterator.Match(TokenType.Identifier).Text;

        iterator.Match(TokenType.OpenCurly);

        while (iterator.Current.Type != TokenType.CloseCurly)
        {
            var member = BitFieldMemberDeclaration.Parse(iterator, parser);

            if (iterator.Current.Type != TokenType.CloseCurly)
            {
                iterator.Match(TokenType.Comma);
            }

            declaration.Members.Add(member);
        }

        iterator.Match(TokenType.CloseCurly);

        return declaration;
    }
}