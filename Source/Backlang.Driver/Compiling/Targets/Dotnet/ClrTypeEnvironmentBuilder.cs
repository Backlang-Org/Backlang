using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public class ClrTypeEnvironmentBuilder
{
    public static IAssembly CollectTypes(Assembly ass)
    {
        var assembly = new DescribedAssembly(new QualifiedName(ass.GetName().Name));

        var types = ass.GetTypes();

        foreach (var type in types)
        {
            if (!type.IsPublic) continue;

            var ns = QualifyNamespace(type.Namespace);

            var dt = new DescribedType(new SimpleName(type.Name).Qualify(ns), assembly);
            dt.IsSealed = type.IsSealed;

            assembly.AddType(dt);
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

            foreach (var attr in type.GetCustomAttributes())
            {
                t.AddAttribute(new DescribedAttribute(ResolveType(resolver, attr.GetType())));
            }

            AddMembers(type, t, resolver);
        }
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

    private static QualifiedName QualifyNamespace(string @namespace)
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

                foreach (var attr in m.GetCustomAttributes())
                {
                    method.AddAttribute(new DescribedAttribute(ResolveType(resolver, attr.GetType())));
                }

                if (m.IsSpecialName)
                {
                    method.AddAttribute(new DescribedAttribute(ResolveType(resolver, typeof(SpecialNameAttribute))));
                }

                t.AddMethod(method);
            }
            else if (member is FieldInfo field)
            {
                var f = new DescribedField(t, new SimpleName(field.Name),
                    field.IsStatic, ResolveType(resolver, field.FieldType));

                t.AddField(f);
            }
            else if (member is PropertyInfo prop)
            {
                var p = new DescribedProperty(new SimpleName(prop.Name),
                     ResolveType(resolver, prop.PropertyType), t);

                t.AddProperty(p);
            }
        }
    }

    private static void ConvertParameter(ParameterInfo[] parameterInfos, DescribedMethod method, TypeResolver resolver)
    {
        foreach (var p in parameterInfos)
        {
            var type = resolver.ResolveTypes(new SimpleName(p.ParameterType.Name).Qualify(p.ParameterType.Namespace)).FirstOrDefault();

            if (type != null)
            {
                var pa = new Parameter(type, p.Name);
                method.AddParameter(pa);
            }
        }
    }
}