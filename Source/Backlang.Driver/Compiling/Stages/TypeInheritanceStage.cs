using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Driver.Compiling.Typesystem;
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Backlang.Driver.Compiling.Stages;

public sealed class TypeInheritanceStage : IHandler<CompilerContext, CompilerContext>
{
    public static MethodBody CompileBody(LNode function, CompilerContext context, IType parentType)
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
                AppendVariableDeclaration(context, block, node);
            }
            else if (node.Name == (Symbol)"print")
            {
                AppendPrint(context, block, node);
            }
            else if (node.Calls(CodeSymbols.Return))
            {
                var valueNode = node.Args[0].Args[0];
                var rt = ConvertConstant(GetLiteralType(valueNode.Value, context.Binder), valueNode.Value);

                block.Flow =
                    new ReturnFlow(rt);
            }
            else if (node.Calls(CodeSymbols.Throw))
            {
                var valueNode = node.Args[0].Args[0];
                var constant = block.AppendInstruction(ConvertConstant(
                    GetLiteralType(node.Args[0].Args[0].Value, context.Binder), node.Args[0].Args[0].Value));

                var str = block.AppendInstruction(Instruction.CreateLoad(GetLiteralType(node.Args[0].Args[0].Value, context.Binder), constant));

                block.Flow = UnreachableFlow.Instance;
            }
        }

        return new MethodBody(
            new Parameter(parentType),
            new Parameter(parentType),
            EmptyArray<Parameter>.Value,
            graph.ToImmutable());
    }

    public static DescribedBodyMethod ConvertFunction(CompilerContext context, DescribedType type, LNode function, string methodName = null, bool hasBody = true)
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

        AddParameters(method, function, context);
        SetReturnType(method, function, context);

        if (methodName == "new" && method.IsStatic)
        {
            method.IsConstructor = true;
        }

        MethodBody body = null;
        if (hasBody)
        {
            body = CompileBody(function, context, type);
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

    public static void ConvertTypeMembers(LNode members, DescribedType type, CompilerContext context)
    {
        foreach (var member in members.Args)
        {
            if (member.Name == CodeSymbols.Var)
            {
                ConvertFields(type, context, member);
            }
            else if (member.Calls(CodeSymbols.Fn))
            {
                type.AddMethod(ConvertFunction(context, type, member, hasBody: false));
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

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            ConvertTypesOrInterface(context, tree);

            ConvertFreeFunctions(context, tree);

            ConvertEnums(context, tree);

            ConvertUnions(context, tree);
        }

        return await next.Invoke(context);
    }

    private static string GetMethodName(LNode function)
    {
        return function.Args[1].Args[0].Args[0].Name.Name;
    }

    private static void AddParameters(DescribedBodyMethod method, LNode function, CompilerContext context)
    {
        var param = function.Args[2];

        foreach (var p in param.Args)
        {
            var pa = ConvertParameter(p, context);
            method.AddParameter(pa);
        }
    }

    private static void AppendPrint(CompilerContext context, BasicBlockBuilder block, LNode node)
    {
        var method = context.writeMethods.FirstOrDefault();
        var constant = block.AppendInstruction(
            ConvertConstant(
                GetLiteralType(node.Args[0].Args[0].Value, context.Binder),
            node.Args[0].Args[0].Value));

        var str = block.AppendInstruction(
            Instruction.CreateLoad(
                GetLiteralType(node.Args[0].Args[0].Value, context.Binder), constant));

        block.AppendInstruction(Instruction.CreateCall(method, MethodLookup.Static, new ValueTag[] { str }));
    }

    private static void AppendVariableDeclaration(CompilerContext context, BasicBlockBuilder block, LNode node)
    {
        var decl = node.Args[1];

        //ToDo: Fix IR Typ resolving
        var types = context.Binder.ResolveTypes(GetNameOfPrimitiveType(context.Binder, node.Args[0].Args[0].Args[0].Name.ToString().Replace("#", "")));
        var elementType = types.First();

        var instruction = Instruction.CreateAlloca(elementType);
        var local = block.AppendInstruction(instruction);

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

    private static void ConvertEnums(CompilerContext context, CompilationUnit tree)
    {
        var enums = tree.Body.Where(_ => _.IsCall && _.Name == CodeSymbols.Enum);

        foreach (var enu in enums)
        {
            var name = enu.Args[0].Name;
            var members = enu.Args[2];

            var type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(name.Name).Qualify(context.Assembly.Name)).First();

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
    }

    private static void ConvertFields(DescribedType type, CompilerContext context, LNode member)
    {
        var mtype = IntermediateStage.GetType(member.Args[0], context);

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

    private static void ConvertFreeFunctions(CompilerContext context, CompilationUnit tree)
    {
        var ff = tree.Body.Where(_ => _.IsCall && _.Name == CodeSymbols.Fn);

        foreach (var function in ff)
        {
            DescribedType type;

            if (!context.Assembly.Types.Any(_ => _.FullName.FullName == $"{context.Assembly.Name}.{Names.ProgramClass}"))
            {
                type = new DescribedType(new SimpleName(Names.ProgramClass).Qualify(context.Assembly.Name), context.Assembly);
                type.IsStatic = true;

                context.Assembly.AddType(type);
            }
            else
            {
                type = (DescribedType)context.Assembly.Types.First(_ => _.FullName.FullName == $"{context.Assembly.Name}.{Names.ProgramClass}");
            }

            string methodName = GetMethodName(function);
            if (methodName == "main") methodName = "Main";

            var method = ConvertFunction(context, type, function, methodName: methodName);

            if (method != null) type.AddMethod(method);
        }
    }

    private static Parameter ConvertParameter(LNode p, CompilerContext context)
    {
        var type = IntermediateStage.GetType(p.Args[0], context);
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

    private static void ConvertTypesOrInterface(CompilerContext context, CompilationUnit tree)
    {
        var types = tree.Body.Where(_ => _.IsCall && (_.Name == CodeSymbols.Struct || _.Name == CodeSymbols.Class || _.Name == CodeSymbols.Interface));

        foreach (var st in types)
        {
            var name = st.Args[0].Name;
            var inheritances = st.Args[1];
            var members = st.Args[2];

            var type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(name.Name).Qualify(context.Assembly.Name)).First();

            foreach (var inheritance in inheritances.Args)
            {
                type.AddBaseType(context.Binder.ResolveTypes(new SimpleName(inheritance.Name.Name).Qualify(context.Assembly.Name)).First());
            }

            ConvertTypeMembers(members, type, context);
        }
    }

    private static void ConvertUnions(CompilerContext context, CompilationUnit tree)
    {
        var types = tree.Body.Where(_ => _.IsCall && _.Name == Symbols.Union);

        foreach (var node in types)
        {
            var type = new DescribedType(new SimpleName(node.Args[0].Name.Name).Qualify(context.Assembly.FullName.FullName), context.Assembly);
            type.AddBaseType(ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(ValueType)));

            var attributeType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(StructLayoutAttribute));

            var attribute = new DescribedAttribute(attributeType);
            attribute.ConstructorArguments.Add(
                new AttributeArgument(
                    ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(LayoutKind)),
                    LayoutKind.Explicit)
                );

            type.AddAttribute(attribute);

            foreach (var member in node.Args[1].Args)
            {
                if (member.Name == CodeSymbols.Var)
                {
                    var mtype = IntermediateStage.GetType(member.Args[0], context);

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
    }

    private static QualifiedName GetNameOfPrimitiveType(TypeResolver binder, string name)
    {
        var aliases = new Dictionary<string, string>()
        {
            ["bool"] = "Boolean",

            ["i8"] = "Byte",
            ["i16"] = "Int16",
            ["i32"] = "Int32",
            ["i64"] = "Int64",

            ["u16"] = "UInt16",
            ["u32"] = "UInt32",
            ["u64"] = "UInt64",

            ["char"] = "Char",
            ["string"] = "String",
            ["none"] = "Void",
        };

        if (aliases.ContainsKey(name))
        {
            name = aliases[name];
        }

        return ClrTypeEnvironmentBuilder.ResolveType(binder, name, "System").FullName;
    }

    private static void SetReturnType(DescribedBodyMethod method, LNode function, CompilerContext context)
    {
        var retType = function.Args[0];
        method.ReturnParameter = new Parameter(IntermediateStage.GetType(retType, context));
    }
}