using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using System.Reflection;

namespace Backlang_Compiler.Compiling.Typesystem;

public class ClrTypeEnvironmentBuilder
{
    public static IAssembly CollectTypes(Assembly ass)
    {
        var assembly = new DescribedAssembly(new QualifiedName(ass.GetName().Name));

        var types = ass.GetTypes();

        foreach (var type in types)
        {
            if (!type.IsPublic) continue;

            assembly.AddType(new DescribedType(new SimpleName(type.Name).Qualify(type.Namespace), assembly));
        }

        return assembly;
    }

    public static void FillTypes(Assembly ass, TypeResolver resolver)
    {
        foreach (var type in ass.GetTypes())
        {
            if (!type.IsPublic) continue;

            var t = ResolveType(resolver, type);

            if (type.BaseType != null)
            {
                var bt = ResolveType(resolver, type.BaseType);

                if (bt != null)
                {
                    t.AddBaseType(bt);
                }
            }

            foreach (var item in type.GetGenericArguments())
            {
                t.AddGenericParameter(new DescribedGenericParameter(t, new SimpleName(item.Name)));
            }

            AddMembers(type, t, resolver);
        }
    }

    public static DescribedType ResolveType(TypeResolver resolver, Type type)
    {
        return (DescribedType)resolver.ResolveTypes(new SimpleName(type.Name).Qualify(type.Namespace))?.FirstOrDefault();
    }

    public static DescribedType ResolveType(TypeResolver resolver, string name, string ns)
    {
        return (DescribedType)resolver.ResolveTypes(new SimpleName(name).Qualify(ns))?.FirstOrDefault();
    }

    private static void AddMembers(Type type, DescribedType t, TypeResolver resolver)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is ConstructorInfo ctor)
            {
                var method = new DescribedMethod(t,
                    new SimpleName(ctor.Name), ctor.IsStatic, ResolveType(resolver, typeof(void)));

                method.IsConstructor = true;

                ConvertParameter(ctor.GetParameters(), method, resolver);

                t.AddMethod(method);
            }
            else if (member is MethodInfo m)
            {
                var method = new DescribedMethod(t, new SimpleName(m.Name), m.IsStatic, ResolveType(resolver, m.ReturnType));

                ConvertParameter(m.GetParameters(), method, resolver);

                t.AddMethod(method);
            }
            else if (member is FieldInfo field)
            {
                var f = new DescribedField(t, new SimpleName(field.Name),
                    field.IsStatic, ResolveType(resolver, field.FieldType));

                t.AddField(f);
            }
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