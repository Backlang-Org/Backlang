namespace Backlang_Compiler.Core;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class KeywordAttribute : Attribute
{
    public KeywordAttribute(string keyword)
    {
        Keyword = keyword;
    }

    public string Keyword { get; set; }
}