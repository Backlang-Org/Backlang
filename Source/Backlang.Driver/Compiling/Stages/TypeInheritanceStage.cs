using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc.Syntax;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Backlang.Driver.Compiling.Stages;

public sealed class TypeInheritanceStage : IHandler<CompilerContext, CompilerContext>
{
    private static readonly ImmutableDictionary<string, string> Aliases = new Dictionary<string, string>()
    {
        ["bool"] = "Boolean",

        ["i8"] = "Byte",
        ["i16"] = "Int16",
        ["i32"] = "Int32",
        ["i64"] = "Int64",

        ["u8"] = "Byte",
        ["u16"] = "UInt16",
        ["u32"] = "UInt32",
        ["u64"] = "UInt64",

        ["f16"] = "Half",
        ["f32"] = "Single",
        ["f64"] = "Double",

        ["char"] = "Char",
        ["string"] = "String",
        ["none"] = "Void",
    }.ToImmutableDictionary();

    public static DescribedBodyMethod ConvertFunction(CompilerContext context, DescribedType type,
        LNode function, QualifiedName modulename, string methodName = null, bool hasBody = true)
    {
        if (methodName == null) methodName = GetMethodName(function);

        var returnType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void));

        var method = new DescribedBodyMethod(type,
            new QualifiedName(methodName).FullyUnqualifiedName,
            function.Attrs.Contains(LNode.Id(CodeSymbols.Static)), returnType);

        Utils.SetAccessModifier(function, method);

        ConvertAnnotations(function, method, context, modulename,
            AttributeTargets.Method, (attr, t) => ((DescribedBodyMethod)t).AddAttribute(attr));

        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Operator)))
        {
            method.AddAttribute(new DescribedAttribute(ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(SpecialNameAttribute))));
        }
        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Override)))
        {
            method.IsOverride = true;
        }
        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Extern)))
        {
            method.IsExtern = true;
        }
        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Abstract)))
        {
            method.AddAttribute(FlagAttribute.Abstract);
        }

        AddParameters(method, function, context, modulename);
        SetReturnType(method, function, context, modulename);

        if (methodName == ".ctor")
        {
            method.IsConstructor = true;
        }
        else if (methodName == ".dtor")
        {
            method.IsDestructor = true;
        }

        if (hasBody)
        {
            context.BodyCompilations.Add(new(function, context, method, modulename));
        }

        if (type.Methods.Any(_ => _.FullName.FullName.Equals(method.FullName.FullName)))
        {
            context.AddError(function, "Function '" + method.FullName + "' is already defined.");
            return null;
        }

        return method;
    }

    public static void ConvertTypeMembers(LNode members, DescribedType type, CompilerContext context, QualifiedName modulename)
    {
        foreach (var member in members.Args)
        {
            if (member.Name == CodeSymbols.Var)
            {
                ConvertFields(type, context, member, modulename);
            }
            else if (member.Calls(CodeSymbols.Fn))
            {
                type.AddMethod(ConvertFunction(context, type, member, modulename, hasBody: false));
            }
            else if (member.Calls(CodeSymbols.Property))
            {
                type.AddProperty(ConvertProperty(context, type, member));
            }
        }
    }

    public static DescribedType ResolveTypeWithModule(LNode typeNode, CompilerContext context, QualifiedName modulename, QualifiedName fullName)
    {
        var resolvedType = context.Binder.ResolveTypes(fullName).FirstOrDefault();

        if (resolvedType == null)
        {
            resolvedType = context.Binder.ResolveTypes(fullName.Qualify(modulename)).FirstOrDefault();

            if (resolvedType == null)
            {
                var primitiveName = GetNameOfPrimitiveType(context.Binder, fullName.FullyUnqualifiedName.ToString());

                if (!primitiveName.HasValue)
                {
                    resolvedType = null;
                }
                else
                {
                    resolvedType = context.Binder.ResolveTypes(primitiveName.Value).FirstOrDefault();
                }

                if (resolvedType == null)
                {
                    context.AddError(typeNode, $"Type {fullName} cannot be found");
                }
            }
        }

        return (DescribedType)resolvedType;
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

                var fullname = Utils.GetQualifiedName(annotation.Target);

                if (!fullname.FullyUnqualifiedName.ToString().EndsWith("Attribute"))
                {
                    fullname = AppendAttributeToName(fullname);
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
                    .FirstOrDefault(_ => _.AttributeType.FullName.ToString() == typeof(AttributeUsageAttribute).FullName);

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

    public static DescribedProperty ConvertProperty(CompilerContext context, DescribedType type, LNode member)
    {
        var property = new DescribedProperty(new SimpleName(member.Args[3].Args[0].Name.Name), IntermediateStage.GetType(member.Args[0], context), type);

        Utils.SetAccessModifier(member, property);

        if (member.Args[1] != LNode.Missing)
        {
            // getter defined
            var getter = new DescribedPropertyMethod(new SimpleName($"get_{property.Name}"), type);
            Utils.SetAccessModifier(member.Args[1], getter, property.GetAccessModifier());
            property.Getter = getter;
        }

        if (member.Args[2] != LNode.Missing)
        {
            if (member.Args[2].Name == Symbols.init)
            {
                // initonly setter defined
                var initOnlySetter = new DescribedPropertyMethod(new SimpleName($"init_{property.Name}"), type);
                initOnlySetter.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Private));
                Utils.SetAccessModifier(member.Args[2], initOnlySetter, property.GetAccessModifier());
                property.InitOnlySetter = initOnlySetter;
            }
            else
            {
                // setter defined
                var setter = new DescribedPropertyMethod(new SimpleName($"set_{property.Name}"), type);
                setter.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Private));
                Utils.SetAccessModifier(member.Args[2], setter, property.GetAccessModifier());
                property.Setter = setter;
            }
        }

        return property;
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            var modulename = Utils.GetModuleName(tree);

            foreach (var node in tree.Body)
            {
                if (node.Calls(CodeSymbols.Struct) || node.Calls(CodeSymbols.Class) || node.Calls(CodeSymbols.Interface))
                {
                    ConvertTypeOrInterface(context, node, modulename);
                }
                else if (node.Calls(CodeSymbols.Fn))
                {
                    ConvertFreeFunction(context, node, modulename);
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

    private static QualifiedName AppendAttributeToName(QualifiedName fullname)
    {
        var qualifier = fullname.Slice(0, fullname.PathLength - 1);

        return new SimpleName(fullname.FullyUnqualifiedName.ToString() + "Attribute").Qualify(qualifier);
    }

    private static string GetMethodName(LNode function)
    {
        return function.Args[1].Args[0].Args[0].Name.Name;
    }

    private static void AddParameters(DescribedBodyMethod method, LNode function, CompilerContext context, QualifiedName modulename)
    {
        var param = function.Args[2];

        foreach (var p in param.Args)
        {
            var pa = ConvertParameter(p, context, modulename);
            method.AddParameter(pa);
        }
    }

    private static void ConvertEnum(CompilerContext context, LNode node, QualifiedName modulename)
    {
        if (node is (_, (_, var nameNode, var typeNode, var membersNode)))
        {
            var name = nameNode.Name;
            var members = membersNode;

            var type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(name.Name).Qualify(modulename)).FirstOrDefault();

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
                        mtype = IntermediateStage.GetType(member.Args[0], context);
                    }

                    if (member is (_, var mt, (_, var mname, var mvalue)))
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
            valueField.AddAttribute(new DescribedAttribute(ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(SpecialNameAttribute))));

            type.AddField(valueField);
        }
    }

    private static void ConvertFields(DescribedType type, CompilerContext context, LNode member, QualifiedName modulename)
    {
        var ftype = member.Args[0].Args[0].Args[0];
        var fullname = Utils.GetQualifiedName(ftype);

        var mtype = ResolveTypeWithModule(ftype, context, modulename, fullname);

        var mvar = member.Args[1];
        var mname = mvar.Args[0].Name;
        var mvalue = mvar.Args[1];

        var field = new DescribedField(type, new SimpleName(mname.Name), false, mtype);

        if (mvalue != LNode.Missing)
        {
            field.InitialValue = mvalue.Args[0].Value;
        }
        if (member.Attrs.Any(_ => _.Name == Symbols.Mutable))
        {
            field.AddAttribute(Attributes.Mutable);
        }

        type.AddField(field);
    }

    private static void ConvertFreeFunction(CompilerContext context, LNode node, QualifiedName modulename)
    {
        DescribedType type;

        if (!context.Assembly.Types.Any(_ => _.FullName.FullName == $".{Names.ProgramClass}"))
        {
            type = new DescribedType(new SimpleName(Names.ProgramClass).Qualify(string.Empty), context.Assembly);
            type.IsStatic = true;
            type.IsPublic = true;

            context.Assembly.AddType(type);
        }
        else
        {
            type = (DescribedType)context.Assembly.Types.First(_ => _.FullName.FullName == $".{Names.ProgramClass}");
        }

        string methodName = GetMethodName(node);
        if (methodName == "main") methodName = "Main";

        var method = ConvertFunction(context, type, node, modulename, methodName: methodName);

        if (method != null) type.AddMethod(method);
    }

    private static Parameter ConvertParameter(LNode p, CompilerContext context, QualifiedName modulename)
    {
        var ptype = p.Args[0].Args[0].Args[0];
        var fullname = Utils.GetQualifiedName(ptype);

        var type = ResolveTypeWithModule(ptype, context, modulename, fullname);
        var assignment = p.Args[1];

        var name = assignment.Args[0].Name;

        var param = new Parameter(type, name.ToString());

        if (!assignment.Args[1].Args.IsEmpty)
        {
            param.HasDefault = true;
            param.DefaultValue = assignment.Args[1].Args[0].Value;
        }

        return param;
    }

    private static void ConvertTypeOrInterface(CompilerContext context, LNode node, QualifiedName modulename)
    {
        var name = Utils.GetQualifiedName(node.Args[0]);
        var inheritances = node.Args[1];
        var members = node.Args[2];

        var type = (DescribedType)context.Binder.ResolveTypes(name.Qualify(modulename)).FirstOrDefault();

        ConvertAnnotations(node, type, context, modulename,
            AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
            (attr, t) => ((DescribedType)t).AddAttribute(attr));

        foreach (var inheritance in inheritances.Args)
        {
            var fullName = Utils.GetQualifiedName(inheritance);
            var btype = ResolveTypeWithModule(inheritance, context, modulename, fullName);

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

        ConvertTypeMembers(members, type, context, modulename);
    }

    private static void ConvertUnion(CompilerContext context, LNode node, QualifiedName modulename)
    {
        var type = new DescribedType(new SimpleName(node.Args[0].Name.Name).Qualify(modulename), context.Assembly);
        type.AddBaseType(ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(ValueType)));

        var attributeType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(StructLayoutAttribute));

        var attribute = new DescribedAttribute(attributeType);
        attribute.ConstructorArguments.Add(
            new AttributeArgument(
                ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(LayoutKind)),
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
                var fullname = Utils.GetQualifiedName(ftype);

                var mtype = ResolveTypeWithModule(ftype, context, modulename, fullname);

                var mvar = member.Args[1];
                var mname = mvar.Args[0].Name;
                var mvalue = mvar.Args[1];

                var field = new DescribedField(type, new SimpleName(mname.Name), false, mtype);

                attributeType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(FieldOffsetAttribute));
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

    private static QualifiedName? GetNameOfPrimitiveType(TypeResolver binder, string name)
    {
        if (Aliases.ContainsKey(name))
        {
            name = Aliases[name];
        }

        var primitiveType = ClrTypeEnvironmentBuilder.ResolveType(binder, name, "System");

        if (primitiveType is not null)
        {
            return primitiveType.FullName;
        }

        return null;
    }

    private static void SetReturnType(DescribedBodyMethod method, LNode function, CompilerContext context, QualifiedName modulename)
    {
        if (function is (_, (_, (_, var retType))))
        {
            var fullName = Utils.GetQualifiedName(retType);

            var rtype = ResolveTypeWithModule(retType, context, modulename, fullName);

            method.ReturnParameter = new Parameter(rtype);
        }
    }
}