namespace Backlang.Codeanalysis.Parsing;

public sealed class ParsePoints<T> : Dictionary<TokenType, Func<TokenIterator, Parser, T>>
{ }
