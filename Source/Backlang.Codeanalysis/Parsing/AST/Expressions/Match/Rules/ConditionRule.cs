namespace Backlang.Codeanalysis.Parsing.AST.Expressions.Match.Rules;

public sealed class ConditionRule : MatchRule
{
    public Expression Condition { get; internal set; }
    public Token OperatorToken { get; internal set; }
}