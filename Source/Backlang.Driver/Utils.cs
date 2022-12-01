using Backlang.Core.CompilerService;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Runtime.CompilerServices;

namespace Backlang.Driver;

public static class Utils
{
    public static FlowGraphBuilder CreateGraphBuilder()
    {
        var graph = new FlowGraphBuilder();
        graph.AddAnalysis(new ConstantAnalysis<ExceptionDelayability>(PermissiveExceptionDelayability.Instance));

        return graph;
    }

    public static DescribedType ResolveType(TypeResolver resolver, Type type)
    {
        var ns = ConversionUtils.QualifyNamespace(type.Namespace);

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

    public static bool IsUnitType(CompilerContext context, IType resolvedUnit)
    {
        var attr = resolvedUnit.Attributes.GetAll();
        var attrType = Utils.ResolveType(context.Binder, typeof(UnitTypeAttribute));

        return attr.Select(_ => _.AttributeType).Contains(attrType);
    }

    public static void AddCompilerGeneratedAttribute(TypeResolver binder, DescribedType type) {
        var attributeType = ResolveType(binder, typeof(CompilerGeneratedAttribute));

        type.AddAttribute(new DescribedAttribute(attributeType));
    }
}