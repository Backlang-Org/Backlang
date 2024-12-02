namespace Backlang.Codeanalysis.Core.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class KeywordAttribute : Attribute
{
    public KeywordAttribute(string keyword)
    {
        Keyword = keyword;
    }

    public string Keyword { get; set; }
}