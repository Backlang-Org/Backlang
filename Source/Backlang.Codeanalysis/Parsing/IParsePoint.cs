namespace Backlang.Codeanalysis.Parsing;

public interface IParsePoint<T>
{
    static abstract T Parse(TokenIterator iterator, Parser parser);
}