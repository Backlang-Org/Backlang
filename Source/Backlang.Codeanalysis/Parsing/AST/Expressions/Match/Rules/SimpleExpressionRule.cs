using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Expressions.Match;
namespace Backlang.Codeanalysis.Parsing.AST.Expressions.Match.Rules;

public sealed class SimpleExpressionRule : MatchRule
{
    public Expression Matcher { get; set; }
}
