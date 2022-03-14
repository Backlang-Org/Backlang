namespace Backlang.Codeanalysis.Parsing;

public interface IParsePoint<TSelf>
{
    static abstract TSelf Parse(TokenIterator iterator, Parser parser);
}