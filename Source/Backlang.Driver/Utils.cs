using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using System.Text;

namespace Backlang.Driver;

public static class Utils
{
    public static FlowGraphBuilder CreateGraphBuilder()
    {
        var graph = new FlowGraphBuilder();
        graph.AddAnalysis(new ConstantAnalysis<ExceptionDelayability>(PermissiveExceptionDelayability.Instance));

        return graph;
    }

    public static QualifiedName QualifyNamespace(string @namespace)
    {
        var spl = @namespace.Split('.');

        QualifiedName? name = null;

        foreach (var path in spl)
        {
            if (name == null)
            {
                name = new SimpleName(path).Qualify();
                continue;
            }

            name = new SimpleName(path).Qualify(name.Value);
        }

        return name.Value;
    }

    public static DescribedType ResolveType(TypeResolver resolver, Type type)
    {
        var ns = QualifyNamespace(type.Namespace);

        return (DescribedType)resolver.ResolveTypes(new SimpleName(type.Name).Qualify(ns))?.FirstOrDefault();
    }

    public static DescribedType ResolveType(TypeResolver resolver, string name, string ns)
    {
        return (DescribedType)resolver.ResolveTypes(new SimpleName(name).Qualify(ns))?.FirstOrDefault();
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
}