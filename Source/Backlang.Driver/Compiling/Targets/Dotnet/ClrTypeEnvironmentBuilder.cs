using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public class ClrTypeEnvironmentBuilder
{
    private static ConcurrentBag<(MethodBase, DescribedMethod)> toAdjustParameters = new();

    public static IAssembly CollectTypes(Assembly ass)
    {
        var assembly = new DescribedAssembly(new QualifiedName(ass.GetName().Name));

        var types = ass.GetTypes();

        foreach (var type in types)
        {
            if (!type.IsPublic) continue;

            var ns = Utils.QualifyNamespace(type.Namespace);

            var dt = new DescribedType(new SimpleName(type.Name).Qualify(ns), assembly);
            dt.IsSealed = type.IsSealed;

            assembly.AddType(dt);
        }

        return assembly;
    }

    public static void FillTypes(Assembly ass, CompilerContext context)
    {
        Parallel.ForEach(ass.GetTypes(), type => {
            if (!type.IsPublic) return;

            var t = Utils.ResolveType(context.Binder, type);

            if (type.BaseType != null)
            {
                var bt = Utils.ResolveType(context.Binder, type.BaseType);

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
                var attribute = new DescribedAttribute(Utils.ResolveType(context.Binder, attrType));

                foreach (var prop in attrType.GetProperties())
                {
                    var value = prop.GetValue(attr);

                    attribute.ConstructorArguments.Add(
                        new AttributeArgument(Utils.ResolveType(context.Binder, prop.PropertyType), value));
                }

                t.AddAttribute(attribute);
            }

            AddMembers(type, t, context.Binder);
        });

        foreach (var toadjust in toAdjustParameters)
        {
            ConvertParameter(toadjust.Item1.GetParameters(), toadjust.Item2, context);
        }

        toAdjustParameters.Clear();
    }

    public static void AddMembers(Type type, DescribedType t, TypeResolver resolver)
    {
        Parallel.ForEach(type.GetMembers(), member => {
            if (member is ConstructorInfo ctor && ctor.IsPublic)
            {
                var method = new DescribedMethod(t,
                    new SimpleName(ctor.Name), ctor.IsStatic, Utils.ResolveType(resolver, typeof(void)))
                {
                    IsConstructor = true
                };

                toAdjustParameters.Add((ctor, method));

                foreach (DescribedGenericParameter gp in t.GenericParameters)
                {
                    method.AddGenericParameter(new DescribedGenericParameter(method, new SimpleName(gp.Name.ToString())));
                }

                t.AddMethod(method);
            }
            else if (member is MethodInfo m && m.IsPublic)
            {
                AddMethod(t, resolver, m, toAdjustParameters);
            }
            else if (member is FieldInfo field && field.IsPublic)
            {
                var f = new DescribedField(t, new SimpleName(field.Name),
                    field.IsStatic, Utils.ResolveType(resolver, field.FieldType));

                t.AddField(f);
            }
            else if (member is PropertyInfo prop)
            {
                var p = new DescribedProperty(new SimpleName(prop.Name),
                     Utils.ResolveType(resolver, prop.PropertyType), t);

                t.AddProperty(p);
            }
        });
    }

    public static void AddMethod(DescribedType t, TypeResolver resolver, MethodInfo m,
        ConcurrentBag<(MethodBase, DescribedMethod)> toAdjustParameters, string newName = null)
    {
        var method = new DescribedMethod(t, new SimpleName(newName ?? m.Name),
            m.IsStatic, Utils.ResolveType(resolver, m.ReturnType));

        toAdjustParameters.Add((m, method));

        foreach (var attr in m.GetCustomAttributes())
        {
            method.AddAttribute(new DescribedAttribute(Utils.ResolveType(resolver, attr.GetType())));
        }

        if (m.IsSpecialName)
        {
            method.AddAttribute(new DescribedAttribute(Utils.ResolveType(resolver, typeof(SpecialNameAttribute))));
        }

        t.AddMethod(method);
    }

    public static void ConvertParameter(ParameterInfo[] parameterInfos, DescribedMethod method, CompilerContext context)
    {
        foreach (var p in parameterInfos)
        {
            var type = (IType)Utils.ResolveType(context.Binder, p.ParameterType);

            if (p.ParameterType.IsByRef)
            {
                type = Utils.ResolveType(context.Binder, p.ParameterType.Name.Replace("&", ""), p.ParameterType.Namespace)?.MakePointerType(PointerKind.Reference);
            }
            else if (p.ParameterType.IsArray)
            {
                type = Utils.ResolveType(context.Binder, p.ParameterType.Name.Replace("&", ""), p.ParameterType.Namespace);

                if (type != null)
                {
                    type = context.Environment.MakeArrayType(type, p.ParameterType.GetArrayRank());
                }
            }

            if (type != null)
            {
                method.AddParameter(new Parameter(type, p.Name));
                continue;
            }
        }
    }
}