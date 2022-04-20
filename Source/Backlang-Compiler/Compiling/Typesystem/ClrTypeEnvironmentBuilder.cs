using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using System.Reflection;

namespace Backlang_Compiler.Compiling.Typesystem;

public class ClrTypeEnvironmentBuilder
{
    public static IAssembly BuildAssembly()
    {
        var assembly = new DescribedAssembly(new QualifiedName("System"));

        var types = typeof(uint).Assembly.GetTypes();

        foreach (var type in types)
        {
            if (!type.IsPublic) continue;

            assembly.AddType(new DescribedType(new SimpleName(type.Name).Qualify("System"), assembly));
        }

        var resolver = new TypeResolver(assembly);
        foreach (var type in types)
        {
            if (!type.IsPublic) continue;

            var t = (DescribedType)resolver.ResolveTypes(new SimpleName(type.Name).Qualify("System")).First();

            AddMethods(type, t, resolver);
        }

        return assembly;
    }

    private static void AddMethods(Type type, DescribedType t, TypeResolver resolver)
    {
        foreach (var m in type.GetMethods())
        {
            if (m.Name.StartsWith("get_") || m.Name.StartsWith("set_")) continue;

            var method = new DescribedMethod(t, new SimpleName(m.Name), m.IsStatic, resolver.ResolveTypes(new SimpleName(m.ReturnType.Name).Qualify(m.ReturnType.Namespace)).FirstOrDefault());

            ConvertParameter(m.GetParameters(), method, resolver);

            t.AddMethod(method);
        }
    }

    private static void ConvertParameter(ParameterInfo[] parameterInfos, DescribedMethod method, TypeResolver resolver)
    {
        foreach (var p in parameterInfos)
        {
            var pa = new Parameter(resolver.ResolveTypes(new SimpleName(p.ParameterType.Name).Qualify(p.ParameterType.Namespace)).FirstOrDefault(), p.Name);

            method.AddParameter(pa);
        }
    }
}