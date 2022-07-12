using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc.Syntax;
using System.Text;

namespace Backlang.Driver;

public sealed class Utils
{
    public static string GenerateIdentifier()
    {
        var sb = new StringBuilder();
        const string ALPHABET = "abcdefhijklmnopqrstABCDEFGHIJKLMNOPQRSTUVWXYZ&%$";
        var random = new Random();

        for (var i = 0; i < random.Next(5, 9); i++)
        {
            sb.Append(ALPHABET[random.Next()]);
        }

        return sb.ToString();
    }
    public static void SetAccessModifier(LNode node, DescribedMember type, AccessModifier defaultChoice = AccessModifier.Private)
    {
        if (node.Attrs.Contains(LNode.Id(CodeSymbols.Private)))
        {
            type.IsPrivate = true;
        }
        else if (node.Attrs.Contains(LNode.Id(CodeSymbols.Protected)))
        {
            type.IsProtected = true;
        }
        else if (node.Attrs.Contains(LNode.Id(CodeSymbols.Public)))
        {
            type.IsPublic = true;
        }
        else
        {
            type.RemoveAccessModifier();
            type.AddAttribute(AccessModifierAttribute.Create(defaultChoice));
        }
    }
}