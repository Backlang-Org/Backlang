using Backlang.Codeanalysis.Core.Attributes;
using System.Reflection;

namespace Backlang.Codeanalysis.Parsing;

public static class TokenUtils
{
    private static readonly Dictionary<string, TokenType> TokenTypeRepresentations = new Dictionary<string, TokenType>();

    static TokenUtils()
    {
        var typeValues = (TokenType[])Enum.GetValues(typeof(TokenType));

        foreach (var keyword in typeValues)
        {
            var attributes = keyword.GetType().GetField(Enum.GetName<TokenType>(keyword)).GetCustomAttributes<KeywordAttribute>(true);

            if (attributes != null && attributes.Any())
            {
                foreach (var attribute in attributes)
                {
                    TokenTypeRepresentations.Add(attribute.Keyword, keyword);
                }
            }
        }
    }

    public static TokenType GetTokenType(string text)
    {
        if (TokenTypeRepresentations.ContainsKey(text))
        {
            return TokenTypeRepresentations[text];
        }

        return TokenType.Identifier;
    }

    public static bool IsOperator(this Token token)
    {
        foreach (var opInfo in Expression.Operators)
        {
            if (opInfo.TokenType == token.Type)
            {
                return true;
            }
        }

        return false;
    }
}