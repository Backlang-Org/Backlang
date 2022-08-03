using Backlang.Codeanalysis.Parsing.AST;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Loyc.Syntax;
using System.Text;

namespace Backlang.Driver;

public sealed class Utils
{
    private static int labelCounter;

    public static FlowGraphBuilder CreateGraphBuilder()
    {
        var graph = new FlowGraphBuilder();
        graph.AddAnalysis(new ConstantAnalysis<ExceptionDelayability>(PermissiveExceptionDelayability.Instance));

        return graph;
    }

    public static string GenerateIdentifier()
    {
        var sb = new StringBuilder();
        const string ALPHABET = "abcdefhijklmnopqrstABCDEFGHIJKLMNOPQRSTUVWXYZ&%$";
        var random = new Random();

        for (var i = 0; i < random.Next(5, 9); i++)
        {
            sb.Append(ALPHABET[random.Next(ALPHABET.Length)]);
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
        else if (node.Attrs.Contains(LNode.Id(CodeSymbols.Internal)))
        {
            type.IsInternal = true;
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
        bool isPointer = false;

        if (lNode is ("#type*", var arg))
        {
            isPointer = true;
            lNode = arg;
        }

        if (lNode is ("#type", (_, var type)))
        {
            lNode = type;
        }

        if (lNode.ArgCount == 3 && lNode is ("#fn", var retType, _, var args))
        {
            string typename = retType == LNode.Missing ? "Action`" + (args.ArgCount) : "Func`" + (args.ArgCount + 1);

            return new SimpleName(typename).Qualify("System");
        }

        if (lNode.Calls(CodeSymbols.Dot))
        {
            QualifiedName qname = GetQualifiedName(lNode.Args[0]);

            return GetQualifiedName(lNode.Args[1]).Qualify(qname);
        }

        var name = new SimpleName(lNode.Name.Name).Qualify();

        return isPointer ? new PointerName(name, PointerKind.Transient).Qualify() : name;
    }

    public static string NewLabel(string name)
    {
        return $"{name}_{labelCounter++}";
    }

    public static QualifiedName AppendAttributeToName(QualifiedName fullname)
    {
        var qualifier = fullname.Slice(0, fullname.PathLength - 1);

        return new SimpleName(fullname.FullyUnqualifiedName.ToString() + "Attribute").Qualify(qualifier);
    }

    public static string GetMethodName(LNode function)
    {
        return function.Args[1].Args[0].Args[0].Name.Name;
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