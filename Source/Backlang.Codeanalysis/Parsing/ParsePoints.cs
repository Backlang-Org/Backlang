using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public sealed class ParsePoints : Dictionary<TokenType, Func<TokenIterator, Parser, LNode>>
{
}