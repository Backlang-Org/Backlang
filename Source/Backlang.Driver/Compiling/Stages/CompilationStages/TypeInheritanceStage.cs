using Backlang.Contracts.Scoping.Items;
using Flo;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Backlang.Driver.Compiling.Stages;

public sealed partial class TypeInheritanceStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context,
        Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            var modulename = ConversionUtils.GetModuleName(tree);

            foreach (var node in tree.Body)
            {
                if (node.Calls(CodeSymbols.Struct) || node.Calls(CodeSymbols.Class) ||
                    node.Calls(CodeSymbols.Interface))
                {
                    ConvertTypeOrInterface(context, node, modulename, context.GlobalScope);
                }
                else if (node.Calls(CodeSymbols.Fn))
                {
                    ConvertFreeFunction(context, node, modulename, context.GlobalScope);
                }
                else if (node.Calls(CodeSymbols.Enum))
                {
                    ConvertEnum(context, node, modulename);
                }
                else if (node.Calls(Symbols.Union))
                {
                    ConvertUnion(context, node, modulename);
                }
            }
        }

        return await next.Invoke(context);
    }

    public static void ConvertTypeMembers(LNode members, DescribedType type, CompilerContext context,
        QualifiedName modulename, Scope scope)
    {
        foreach (var member in members.Args)
        {
            if (member.Name == CodeSymbols.Var)
            {
                ConvertFields(type, context, member, modulename, scope);
            }
            else if (member.Calls(CodeSymbols.Fn))
            {
                type.AddMethod(ConvertFunction(context, type, member, modulename, scope, hasBody: false));
            }
            else if (member.Calls(CodeSymbols.Property))
            {
                type.AddProperty(ConvertProperty(context, type, member, modulename));
            }
        }
    }

    public static void ConvertAnnotations(LNode st, IMember type,
        CompilerContext context, QualifiedName modulename, AttributeTargets targets,
        Action<DescribedAttribute, IMember> applyAttributeCallback)
    {
        for (var i = 0; i < st.Attrs.Count; i++)
        {
            var annotation = st.Attrs[i];
            if (annotation.Calls(Symbols.Annotation))
            {
                annotation = annotation.Attrs[0];

                if (annotation.Name == LNode.Missing.Name)
                {
                    continue;
                }

                var fullname = ConversionUtils.GetQualifiedName(annotation.Target);

                if (!fullname.FullyUnqualifiedName.ToString().EndsWith("Attribute"))
                {
                    fullname = ConversionUtils.AppendAttributeToName(fullname);
                }

                var resolvedType = ResolveTypeWithModule(annotation.Target, context, modulename, fullname);

                if (resolvedType == null)
                {
                    context.AddError(annotation, $"{annotation.Name.Name} cannot be found");
                    continue;
                }

                var customAttribute = new DescribedAttribute(resolvedType);

                //ToDo: add arguments to custom attribute

                var attrUsage = (DescribedAttribute)resolvedType.Attributes
                    .GetAll()
                    .FirstOrDefault(
                        _ => _.AttributeType.FullName.ToString() == typeof(AttributeUsageAttribute).FullName);

                if (attrUsage != null)
                {
                    var target = attrUsage.ConstructorArguments.FirstOrDefault(_ => _.Value is AttributeTargets);
                    var targetValue = (AttributeTargets)target.Value;

                    if (targetValue.HasFlag(AttributeTargets.All) || targets.HasFlag(targetValue))
                    {
                        applyAttributeCallback(customAttribute, type);
                    }
                    else
                    {
                        context.AddError(st, "Cannot apply Attribute");
                    }
                }
            }
        }
    }

    public static DescribedProperty ConvertProperty(CompilerContext context, DescribedType type, LNode member,
        QualifiedName modulename)
    {
        var property = new DescribedProperty(new SimpleName(member.Args[3].Args[0].Name.Name),
            ResolveTypeWithModule(member.Args[0], context, modulename), type);

        ConversionUtils.SetAccessModifier(member, property);

        if (member.Args[1] != LNode.Missing)
        {
            // getter defined
            var getter = new DescribedPropertyMethod(new SimpleName($"get_{property.Name}"), type);
            ConversionUtils.SetAccessModifier(member.Args[1], getter, property.GetAccessModifier());
            property.Getter = getter;
        }

        if (member.Args[2] != LNode.Missing)
        {
            if (member.Args[2].Name == Symbols.Init)
            {
                // initonly setter defined
                var initOnlySetter = new DescribedPropertyMethod(new SimpleName($"init_{property.Name}"), type);
                initOnlySetter.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Private));
                ConversionUtils.SetAccessModifier(member.Args[2], initOnlySetter, property.GetAccessModifier());
                property.InitOnlySetter = initOnlySetter;
            }
            else
            {
                // setter defined
                var setter = new DescribedPropertyMethod(new SimpleName($"set_{property.Name}"), type);
                setter.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Private));
                ConversionUtils.SetAccessModifier(member.Args[2], setter, property.GetAccessModifier());
                property.Setter = setter;
            }
        }

        return property;
    }

    private static void ConvertEnum(CompilerContext context, LNode node, QualifiedName modulename)
    {
        if (node is var (_, (_, nameNode, typeNode, membersNode)))
        {
            var name = nameNode.Name;
            var members = membersNode;

            var type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(name.Name).Qualify(modulename))
                .FirstOrDefault();

            var i = -1;
            foreach (var member in members.Args)
            {
                if (member.Name == CodeSymbols.Var)
                {
                    IType mtype;
                    if (member.Args[0] == LNode.Missing)
                    {
                        mtype = context.Environment.Int32;
                    }
                    else
                    {
                        mtype = ResolveTypeWithModule(member.Args[0], context, modulename);
                    }

                    if (member is var (_, mt, (_, mname, mvalue)))
                    {
                        if (mvalue == LNode.Missing)
                        {
                            i++;
                        }
                        else
                        {
                            i = (int)mvalue.Args[0].Value;
                        }

                        var field = new DescribedField(type, new SimpleName(mname.Name.Name), true, mtype);
                        field.InitialValue = i;

                        type.AddField(field);
                    }
                }
            }

            var valueField = new DescribedField(type, new SimpleName("value__"), false, context.Environment.Int32);
            valueField.AddAttribute(
                new DescribedAttribute(Utils.ResolveType(context.Binder, typeof(SpecialNameAttribute))));

            type.AddField(valueField);
        }
    }

    private static void ConvertFields(DescribedType type, CompilerContext context, LNode member,
        QualifiedName modulename, Scope scope)
    {
        var ftype = member.Args[0].Args[0].Args[0];

        var mtype = ResolveTypeWithModule(member.Args[0], context, modulename);

        var mvar = member.Args[1];
        var mname = mvar.Args[0].Name;
        var mvalue = mvar.Args[1];

        var field = new DescribedField(type, new SimpleName(mname.Name), false, mtype);

        if (mvalue != LNode.Missing)
        {
            field.InitialValue = mvalue.Args[0].Value;
        }

        var isMutable = member.Attrs.Any(_ => _.Name == Symbols.Mutable);
        if (isMutable)
        {
            field.AddAttribute(Attributes.Mutable);
        }

        var isStatic = member.Attrs.Any(_ => _.Name == CodeSymbols.Static);
        if (isStatic)
        {
            field.IsStatic = true;
        }

        if (scope.Add(new FieldScopeItem { Name = mname.Name, Field = field }))
        {
            type.AddField(field);
        }
        else
        {
            context.AddError(member, $"Field {mname} is already defined.");
        }
    }

    private static void ConvertTypeOrInterface(CompilerContext context, LNode node, QualifiedName modulename,
        Scope scope)
    {
        var name = ConversionUtils.GetQualifiedName(node.Args[0]);
        var inheritances = node.Args[1];
        var members = node.Args[2];

        if (!scope.TryGet<TypeScopeItem>(name.FullName, out var typeItem))
        {
            context.AddError(node, $"Type {typeItem.Name} is not found");
            return;
        }

        var subScope = typeItem.SubScope;
        var type = (DescribedType)typeItem.Type;

        ConvertAnnotations(node, type, context, modulename,
            AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
            (attr, t) => ((DescribedType)t).AddAttribute(attr));

        foreach (var inheritance in inheritances.Args)
        {
            var btype = (DescribedType)ResolveTypeWithModule(inheritance, context, modulename);

            if (btype != null)
            {
                if (!btype.IsSealed)
                {
                    type.AddBaseType(btype);
                }
                else
                {
                    context.AddError(inheritance, $"Cannot inherit from sealed Type {inheritance}");
                }
            }
        }

        ConvertTypeMembers(members, type, context, modulename, subScope);
    }

    private static void ConvertUnion(CompilerContext context, LNode node, QualifiedName modulename)
    {
        var type = new DescribedType(new SimpleName(node.Args[0].Name.Name).Qualify(modulename), context.Assembly);
        type.AddBaseType(Utils.ResolveType(context.Binder, typeof(ValueType)));

        var attributeType = Utils.ResolveType(context.Binder, typeof(StructLayoutAttribute));

        var attribute = new DescribedAttribute(attributeType);
        attribute.ConstructorArguments.Add(
            new AttributeArgument(
                Utils.ResolveType(context.Binder, typeof(LayoutKind)),
                LayoutKind.Explicit)
        );

        type.AddAttribute(attribute);

        ConvertAnnotations(node, type, context, modulename, AttributeTargets.Class,
            (attr, t) => ((DescribedType)t).AddAttribute(attr));

        foreach (var member in node.Args[1].Args)
        {
            if (member.Name == CodeSymbols.Var)
            {
                var ftype = member.Args[0].Args[0].Args[0];

                var mtype = ResolveTypeWithModule(ftype, context, modulename);

                var mvar = member.Args[1];
                var mname = mvar.Args[0].Name;
                var mvalue = mvar.Args[1];

                var field = new DescribedField(type, new SimpleName(mname.Name), false, mtype);

                attributeType = Utils.ResolveType(context.Binder, typeof(FieldOffsetAttribute));
                attribute = new DescribedAttribute(attributeType);
                attribute.ConstructorArguments.Add(
                    new AttributeArgument(
                        mtype,
                        mvalue.Args[0].Value)
                );

                field.AddAttribute(attribute);

                type.AddField(field);
            }
        }

        context.Assembly.AddType(type);
    }
}