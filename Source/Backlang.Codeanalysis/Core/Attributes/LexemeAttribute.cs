namespace Backlang.Codeanalysis.Core.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class LexemeAttribute : Attribute
{
    public LexemeAttribute(string lexeleme)
    {
        Lexeme = lexeleme;
    }

    public string Lexeme { get; set; }
}
