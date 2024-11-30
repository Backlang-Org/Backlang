using System.Text.RegularExpressions;

namespace Backlang.ResourcePreprocessor.Mif.MifFormat;

public readonly struct Token<TType>
{
    public Token(TType type, string value)
    {
        Type = type;
        Value = value;
    }

    public TType Type { get; }

    public string Value { get; }
}

public class Tokenizer<TType>
{
    private readonly TType defaultTokenType;
    private readonly IList<TokenType> tokenTypes = new List<TokenType>();

    public Tokenizer(TType defaultTokenType)
    {
        this.defaultTokenType = defaultTokenType;
    }

    public Tokenizer<TType> Token(TType type, params string[] matchingRegexs)
    {
        foreach (var matchingRegex in matchingRegexs)
        {
            tokenTypes.Add(new TokenType(type, matchingRegex, false));
        }

        return this;
    }

    public Tokenizer<TType> Ignore(TType type, params string[] matchingRegexs)
    {
        foreach (var matchingRegex in matchingRegexs)
        {
            tokenTypes.Add(new TokenType(type, matchingRegex, true));
        }

        return this;
    }

    public IList<Token<TType>> Tokenize(string input)
    {
        IEnumerable<Token<TType>> tokens = new[] { new Token<TType>(defaultTokenType, input) };
        foreach (var type in tokenTypes)
        {
            tokens = ExtractTokenType(tokens, type);
        }

        return tokens.ToList();
    }

    private IEnumerable<Token<TType>> ExtractTokenType(
        IEnumerable<Token<TType>> tokens,
        TokenType toExtract)
    {
        var tokenType = toExtract.Type;
        var tokenMatcher = new Regex(toExtract.MatchingRegex, RegexOptions.Multiline);
        foreach (var token in tokens)
        {
            if (!token.Type.Equals(defaultTokenType))
            {
                yield return token;
                continue;
            }

            var matches = tokenMatcher.Matches(token.Value);
            if (matches.Count == 0)
            {
                yield return token;
                continue;
            }

            var currentIndex = 0;
            foreach (Match match in matches)
            {
                if (toExtract.Ignore)
                {
                    continue;
                }

                if (currentIndex < match.Index)
                {
                    yield return new Token<TType>(
                        defaultTokenType,
                        token.Value.Substring(currentIndex, match.Index - currentIndex));
                }

                yield return new Token<TType>(tokenType, match.Value);
                currentIndex = match.Index + match.Length;
            }

            if (currentIndex < token.Value.Length)
            {
                yield return new Token<TType>(
                    defaultTokenType,
                    token.Value.Substring(currentIndex, token.Value.Length - currentIndex));
            }
        }
    }

    private readonly struct TokenType
    {
        public TokenType(TType type, string matchingRegex, bool ignore)
        {
            Type = type;
            MatchingRegex = matchingRegex;
            Ignore = ignore;
        }

        public TType Type { get; }

        public string MatchingRegex { get; }
        public bool Ignore { get; }
    }
}