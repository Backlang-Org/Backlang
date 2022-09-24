using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public interface IParsePoint
{
    static abstract LNode Parse(TokenIterator iterator, Parser parser);
}