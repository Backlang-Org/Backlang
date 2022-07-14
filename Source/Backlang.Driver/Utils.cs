using Backlang.Codeanalysis.Parsing.AST;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
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

    public static QualifiedName GetModuleName(CompilationUnit tree)
    {
        var moduleDefinition = tree.Body.SingleOrDefault(_ => _.Calls(CodeSymbols.Namespace));

        if (moduleDefinition != null)
        {
            return ShrinkDottedModuleName(moduleDefinition.Args[0]);
        }

        return new SimpleName("").Qualify();
    }

    public static QualifiedName GetQualifiedName(LNode lNode)
    {
        if (lNode.Calls(CodeSymbols.Dot))
        {
            QualifiedName qname = GetQualifiedName(lNode.Args[0]);

            return GetQualifiedName(lNode.Args[1]).Qualify(qname);
        }

        return new SimpleName(lNode.Name.Name).Qualify();
    }

    private static QualifiedName ShrinkDottedModuleName(LNode lNode)
    {
        if (lNode.Calls(CodeSymbols.Dot))
        {
            return ShrinkDottedModuleName(lNode.Args[1]).Qualify(ShrinkDottedModuleName(lNode.Args[0]));
        }
        else
        {
            return new SimpleName(lNode.Name.Name).Qualify();
        }
    }
}