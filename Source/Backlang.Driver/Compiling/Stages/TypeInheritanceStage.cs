using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc;
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

        ["u16"] = "UInt16",
        ["u32"] = "UInt32",
        ["u64"] = "UInt64",

        ["f16"] = typeof(Half).Name,
        ["f32"] = typeof(float).Name,
        ["f64"] = typeof(double).Name,

        ["char"] = "Char",
        ["string"] = "String",
        ["none"] = "Void",
    }.ToImmutableDictionary();

    public static MethodBody CompileBody(LNode function, CompilerContext context, IType parentType, QualifiedName? modulename)
    {
        var graph = new FlowGraphBuilder();
        // Use a permissive exception delayability model to make the optimizer's
        // life easier.
        graph.AddAnalysis(
            new ConstantAnalysis<ExceptionDelayability>(
                PermissiveExceptionDelayability.Instance));

        // Grab the entry point block.
        var block = graph.EntryPoint;

        foreach (var node in function.Args[3].Args)
        {
            if (!node.IsCall) continue;

            if (node.Name == CodeSymbols.Var)
            {
                AppendVariableDeclaration(context, block, node, modulename);
            }
            else if (node.Name == (Symbol)"print")
            {
                AppendPrint(context, block, node);
            }
            else if (node.Calls(CodeSymbols.Return))
            {
                if (node.ArgCount == 1)
                {
                    var valueNode = node.Args[0].Args[0];
                    var rt = ConvertConstant(GetLiteralType(valueNode.Value, context.Binder), valueNode.Value);

                    block.Flow = new ReturnFlow(rt);
                }
            }
            else if (node.Calls(CodeSymbols.Throw))
            {
                var valueNode = node.Args[0].Args[0];
                var constant = block.AppendInstruction(ConvertConstant(
                    GetLiteralType(valueNode.Value, context.Binder), valueNode.Value));

                var msg = block.AppendInstruction(Instruction.CreateLoad(GetLiteralType(valueNode.Value, context.Binder), constant));

                if (node.Args[0].Name.Name == "#string")
                {
                    var exceptionType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(Exception));
                    var exceptionCtor = exceptionType.Methods.FirstOrDefault(_ => _.IsConstructor && _.Parameters.Count == 1);

                    block.AppendInstruction(Instruction.CreateNewObject(exceptionCtor, new List<ValueTag> { msg }));
                }

                block.Flow = UnreachableFlow.Instance;
            }
        }

        return new MethodBody(
            new Parameter(parentType),
            new Parameter(parentType),
            EmptyArray<Parameter>.Value,
            graph.ToImmutable());
    }

    public static DescribedBodyMethod ConvertFunction(CompilerContext context, DescribedType type,
        LNode function, QualifiedName modulename, string methodName = null, bool hasBody = true)
    {
        if (methodName == null) methodName = GetMethodName(function);

        var method = new DescribedBodyMethod(type,
            new QualifiedName(methodName).FullyUnqualifiedName,
            function.Attrs.Contains(LNode.Id(CodeSymbols.Static)), ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void)));

        Utils.SetAccessModifier(function, method);

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

        if (methodName == "new" && method.IsStatic)
        {
            method.IsConstructor = true;
        }

        MethodBody body = null;
        if (hasBody)
        {
            body = CompileBody(function, context, type, modulename);
        }

        /*
        body = body.WithImplementation(
                body.Implementation.Transform(
                AllocaToRegister.Instance,
                CopyPropagation.Instance,
                new ConstantPropagation(),
                GlobalValueNumbering.Instance,
                CopyPropagation.Instance,
                DeadValueElimination.Instance,
                MemoryAccessElimination.Instance,
                CopyPropagation.Instance,
                new ConstantPropagation(),
                DeadValueElimination.Instance,
                ReassociateOperators.Instance,
                DeadValueElimination.Instance
            ));
        */
        method.Body = body;

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
        }
    }

    public static IType GetLiteralType(object value, TypeResolver resolver)
    {
        if (value is string) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(string));
        else if (value is char) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(char));
        else if (value is bool) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(bool));
        else if (value is byte) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(byte));
        else if (value is short) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(short));
        else if (value is ushort) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(ushort));
        else if (value is int) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(int));
        else if (value is uint) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(uint));
        else if (value is long) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(long));
        else if (value is ulong) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(ulong));
        else if (value is float) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(float));
        else if (value is double) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(double));
        else if (value is IdNode id) { } //todo: symbol table

        return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(void));
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

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            var modulename = Utils.GetModuleName(tree);

            foreach (var node in tree.Body)
            {
                ConvertTypesOrInterface(context, node, modulename);

                ConvertFreeFunctions(context, node, modulename);

                ConvertEnums(context, node, modulename);

                ConvertUnion(context, node, modulename);
            }
        }

        return await next.Invoke(context);
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

    private static void AppendPrint(CompilerContext context, BasicBlockBuilder block, LNode node)
    {
        var argTypes = new List<IType>();
        var callTags = new List<ValueTag>();

        foreach (var arg in node.Args)
        {
            var type = GetLiteralType(arg.Args[0].Value, context.Binder);
            argTypes.Add(type);

            var constant = block.AppendInstruction(
            ConvertConstant(type, arg.Args[0].Value));

            block.AppendInstruction(Instruction.CreateLoad(type, constant));

            callTags.Add(constant);
        }

        var method = GetMatchingPrintMethod(context, argTypes);

        var call = Instruction.CreateCall(method, MethodLookup.Static, callTags);

        block.AppendInstruction(call);
    }

    private static IMethod GetMatchingPrintMethod(CompilerContext context, List<IType> argTypes)
    {
        foreach (var m in context.writeMethods)
        {
            if (m.Parameters.Count == argTypes.Count)
            {
                if (MatchesParameters(m, argTypes))
                    return m;
            }
        }

        return null;
    }

    private static bool MatchesParameters(IMethod m, List<IType> argTypes)
    {
        bool matches = false;
        for (int i = 0; i < m.Parameters.Count; i++)
        {
            if (m.Parameters[i].Type.FullName.ToString() == argTypes[i].FullName.ToString())
            {
                matches = (matches || i == 0) && m.Parameters[i].Type.FullName.ToString() == argTypes[i].FullName.ToString();
            }
        }

        return matches;
    }

    private static void AppendVariableDeclaration(CompilerContext context, BasicBlockBuilder block, LNode node, QualifiedName? modulename)
    {
        var decl = node.Args[1];

        var name = Utils.GetQualifiedName(node.Args[0].Args[0].Args[0]);
        var elementType = (DescribedType)context.Binder.ResolveTypes(name.Qualify(modulename.Value)).FirstOrDefault();

        if (elementType == null)
        {
            elementType = (DescribedType)context.Binder.ResolveTypes(name).FirstOrDefault();

            if (elementType == null)
            {
                elementType = (DescribedType)IntermediateStage.GetType(node.Args[0].Args[0].Args[0], context);
            }
        }

        var instruction = Instruction.CreateAlloca(elementType);
        var local = block.AppendInstruction(instruction);

        block.AppendParameter(new BlockParameter(elementType, decl.Args[0].Name.Name));

        if (decl.Args[1].Args[0].HasValue)
        {
            block.AppendInstruction(
               Instruction.CreateStore(
                   elementType,
                   local,
                   block.AppendInstruction(
                       ConvertConstant(elementType, decl.Args[1].Args[0].Value))));
        }
    }

    private static Instruction ConvertConstant(IType elementType, object value)
    {
        Constant constant;
        switch (value)
        {
            case uint v:
                constant = new IntegerConstant(v);
                break;

            case int v:
                constant = new IntegerConstant(v);
                break;

            case long v:
                constant = new IntegerConstant(v);
                break;

            case ulong v:
                constant = new IntegerConstant(v);
                break;

            case byte v:
                constant = new IntegerConstant(v);
                break;

            case short v:
                constant = new IntegerConstant(v);
                break;

            case ushort v:
                constant = new IntegerConstant(v);
                break;

            case float v:
                constant = new Float32Constant(v);
                break;

            case double v:
                constant = new Float64Constant(v);
                break;

            case string v:
                constant = new StringConstant(v);
                break;

            case char v:
                constant = new IntegerConstant(v);
                break;

            case bool v:
                constant = BooleanConstant.Create(v);
                break;

            default:
                constant = NullConstant.Instance;
                break;
        }

        return Instruction.CreateConstant(constant,
                                           elementType);
    }

    private static void ConvertEnums(CompilerContext context, LNode node, QualifiedName modulename)
    {
        if (!(node.IsCall && node.Name == CodeSymbols.Enum)) return;

        var name = node.Args[0].Name;
        var members = node.Args[2];

        var type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(name.Name).Qualify(modulename)).First();

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

                var mname = member.Args[1].Args[0].Name;
                var mvalue = member.Args[1].Args[1];

                if (mvalue == LNode.Missing)
                {
                    i++;
                }
                else
                {
                    i = (int)mvalue.Args[0].Value;
                }

                var field = new DescribedField(type, new SimpleName(mname.Name), true, mtype);
                field.InitialValue = i;

                type.AddField(field);
            }
        }

        var valueField = new DescribedField(type, new SimpleName("value__"), false, context.Environment.Int32);
        valueField.AddAttribute(new DescribedAttribute(ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(SpecialNameAttribute))));

        type.AddField(valueField);
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

    private static void ConvertFreeFunctions(CompilerContext context, LNode node, QualifiedName modulename)
    {
        if (!(node.IsCall && node.Name == CodeSymbols.Fn)) return;

        DescribedType type;

        if (!context.Assembly.Types.Any(_ => _.FullName.FullName == $"{context.Assembly.Name}.{Names.ProgramClass}"))
        {
            type = new DescribedType(new SimpleName(Names.ProgramClass).Qualify(string.Empty), context.Assembly);
            type.IsStatic = true;

            context.Assembly.AddType(type);
        }
        else
        {
            type = (DescribedType)context.Assembly.Types.First(_ => _.FullName.FullName == $"{context.Assembly.Name}.{Names.ProgramClass}");
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

    private static void ConvertTypesOrInterface(CompilerContext context, LNode node, QualifiedName modulename)
    {
        if (!(node.IsCall &&
            (node.Name == CodeSymbols.Struct || node.Name == CodeSymbols.Class || node.Name == CodeSymbols.Interface))) return;

        var name = Utils.GetQualifiedName(node.Args[0]);
        var inheritances = node.Args[1];
        var members = node.Args[2];

        var type = (DescribedType)context.Binder.ResolveTypes(name.Qualify(modulename)).FirstOrDefault();

        ConvertAnnotation(node, type, context, modulename);

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
        if (!(node.IsCall && node.Name == Symbols.Union)) return;

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

        ConvertAnnotation(node, type, context, modulename);

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
        var retType = function.Args[0].Args[0].Args[0];
        var fullName = Utils.GetQualifiedName(retType);

        var rtype = ResolveTypeWithModule(retType, context, modulename, fullName);

        method.ReturnParameter = new Parameter(rtype);
    }
}