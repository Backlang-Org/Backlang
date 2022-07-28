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
        Parallel.ForEach(ass.GetTypes(), type => {
            if (!type.IsPublic) return;

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
                var attrType = attr.GetType();
                var attribute = new DescribedAttribute(ResolveType(resolver, attrType));

                foreach (var prop in attrType.GetProperties())
                {
                    var value = prop.GetValue(attr);

                    attribute.ConstructorArguments.Add(new AttributeArgument(ResolveType(resolver, prop.PropertyType), value));
                }

                t.AddAttribute(attribute);
            }

            AddMembers(type, t, resolver);
        });
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

    public static void AddMembers(Type type, DescribedType t, TypeResolver resolver)
    {
        Parallel.ForEach(type.GetMembers(), member => {
            if (member is ConstructorInfo ctor && ctor.IsPublic)
            {
                var method = new DescribedMethod(t,
                    new SimpleName(ctor.Name), ctor.IsStatic, ResolveType(resolver, typeof(void)));

                method.IsConstructor = true;

                ConvertParameter(ctor.GetParameters(), method, resolver);

                t.AddMethod(method);
            }
            else if (member is MethodInfo m && m.IsPublic)
            {
                AddMethod(t, resolver, m);
            }
            else if (member is FieldInfo field && field.IsPublic)
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
        });
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

    public static void AddMethod(DescribedType t, TypeResolver resolver, MethodInfo m, string newName = null)
    {
        var method = new DescribedMethod(t, new SimpleName(newName ?? m.Name), m.IsStatic, ResolveType(resolver, m.ReturnType));

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

    private static void ConvertParameter(ParameterInfo[] parameterInfos, DescribedMethod method, TypeResolver resolver)
    {
        foreach (var p in parameterInfos)
        {
            var type = ClrTypeEnvironmentBuilder.ResolveType(resolver, p.ParameterType);

            if (type != null)
            {
                var pa = new Parameter(type, p.Name);
                method.AddParameter(pa);
            }
        }
    }
}