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
using Furesoft.Core.CodeDom.Compiler.Transforms;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc;
using Loyc.Syntax;
using System.Runtime.CompilerServices;

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
                context.Environment.TryMakeSignedIntegerType(32, out var elementType); // IntermediateStage.GetType(node.Args[0], context);

                var local = block.AppendInstruction(
                     Instruction.CreateAlloca(elementType));

                var decl = node.Args[1];
                if (decl.Args[1].Args[0].HasValue)
                {
                    block.AppendInstruction(
                       Instruction.CreateStore(
                           elementType,
                           local,
                           block.AppendInstruction(
                               ConvertExpression(elementType, decl.Args[1].Value))));
                }
            }
            else if (node.Name == (Symbol)"print")
            {
                var method = context.writeMethods.FirstOrDefault();
                var constant = block.AppendInstruction(
                    ConvertExpression(
                        GetLiteralType(node.Args[0].Value, context.Binder),
                    node.Args[0].Value.ToString()));

                var str = block.AppendInstruction(
                    Instruction.CreateLoad(
                        GetLiteralType(node.Args[0].Value, context.Binder), constant));

                block.AppendInstruction(Instruction.CreateCall(method, MethodLookup.Static, new ValueTag[] { str }));
            }
            else if (node.Calls(CodeSymbols.Return))
            {
                block.Flow =
                    new ReturnFlow(Instruction.CreateConstant(NullConstant.Instance, null));
            }
        }

        // Finish up the method body.
        return new MethodBody(
            new Parameter(parentType),
            new Parameter(parentType),
            EmptyArray<Parameter>.Value,
            graph.ToImmutable());
    }

    public static DescribedBodyMethod ConvertFunction(CompilerContext context, DescribedType type, LNode function)
    {
        string methodName = function.Args[1].Name.Name;

        var method = new DescribedBodyMethod(type,
            new QualifiedName(methodName).FullyUnqualifiedName,
            function.Attrs.Contains(LNode.Id(CodeSymbols.Static)), ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void)));

        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Operator)))
        {
            method.AddAttribute(new DescribedAttribute(ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(SpecialNameAttribute))));
        }

        var modifier = AccessModifierAttribute.Create(AccessModifier.Public);
        if (function.Attrs.Contains(LNode.Id(CodeSymbols.Private)))
        {
            modifier = AccessModifierAttribute.Create(AccessModifier.Private);
        }

        method.AddAttribute(modifier);

        AddParameters(method, function, context);
        SetReturnType(method, function, context);

        if (methodName == "new" && method.IsStatic)
        {
            method.IsConstructor = true;
        }

        var sourceBody = CompileBody(function, context, type);

        var body = sourceBody.WithImplementation(
         sourceBody.Implementation.Transform(
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
             DeadValueElimination.Instance));

        method.Body = body;

        return method;
    }

    public static IType GetLiteralType(object value, TypeResolver resolver)
    {
        if (value is string) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(string));
        if (value is IdNode id) { } //todo: symbol table

        return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(void));
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            ConvertTypesOrInterface(context, tree);

            ConvertFreeFunctions(context, tree);

            ConvertEnums(context, tree);
        }

        return await next.Invoke(context);
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

    private static void ConvertEnums(CompilerContext context, Codeanalysis.Parsing.AST.CompilationUnit tree)
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

                    if(mvalue == LNode.Missing)
                    {
                        i++;
                    } else
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

    private static Instruction ConvertExpression(IType elementType, object value)
    {
        if (value is uint i)
        {
            return Instruction.CreateConstant(
                                           new IntegerConstant(i, IntegerSpec.UInt32),
                                           elementType);
        }
        else if (value is string str)
        {
            return Instruction.CreateConstant(
                                           new StringConstant(str),
                                           elementType);
        }

        return Instruction.CreateConstant(
                                           new IntegerConstant(0, IntegerSpec.UInt32),
                                           elementType);
    }

    private static void ConvertFreeFunctions(CompilerContext context, Codeanalysis.Parsing.AST.CompilationUnit tree)
    {
        var ff = tree.Body.Where(_ => _.IsCall && _.Name == CodeSymbols.Fn);

        foreach (var function in ff)
        {
            DescribedType type;

            if (!context.Assembly.Types.Any(_ => _.FullName.FullName == $"{context.Assembly.Name}.{Names.ProgramClass}"))
            {
                type = new DescribedType(new SimpleName(Names.ProgramClass).Qualify(context.Assembly.Name), context.Assembly);
                context.Assembly.AddType(type);
            }
            else
            {
                type = (DescribedType)context.Assembly.Types.First(_ => _.FullName.FullName == $"{context.Assembly.Name}.{Names.ProgramClass}");
            }

            var method = ConvertFunction(context, type, function);

            type.AddMethod(method);
        }
    }

    private static void ConvertInterfaceMethods(LNode methods, DescribedType type, CompilerContext context)
    {
        foreach (var function in methods.Args)
        {
            if (function.Calls(CodeSymbols.Fn))
            {
                string methodName = function.Args[1].Name.Name;
                var method = new DescribedBodyMethod(type,
                    new QualifiedName(methodName).FullyUnqualifiedName,
                    function.Attrs.Contains(LNode.Id(CodeSymbols.Static)), ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void)));
                method.Body = null;

                var modifier = AccessModifierAttribute.Create(AccessModifier.Public);
                if (function.Attrs.Contains(LNode.Id(CodeSymbols.Private)))
                {
                    modifier = AccessModifierAttribute.Create(AccessModifier.Private);
                }

                method.AddAttribute(modifier);

                AddParameters(method, function, context);
                SetReturnType(method, function, context);

                type.AddMethod(method);
            }
        }
    }

    private static Parameter ConvertParameter(LNode p, CompilerContext context)
    {
        var type = IntermediateStage.GetType(p.Args[0], context);
        var assignment = p.Args[1];

        var name = assignment.Args[0].Name;

        return new Parameter(type, name.ToString());
    }

    private static void ConvertStructMembers(LNode members, DescribedType type, CompilerContext context)
    {
        foreach (var member in members.Args)
        {
            if (member.Name == CodeSymbols.Var)
            {
                var mtype = IntermediateStage.GetType(member.Args[0], context);

                var mvar = member.Args[1];
                var mname = mvar.Args[0].Name;
                var mvalue = mvar.Args[1];

                var field = new DescribedField(type, new SimpleName(mname.Name), false, mtype);

                if(mvalue != LNode.Missing)
                {
                    field.InitialValue = mvalue.Args[0].Value;
                }

                type.AddField(field);
            }
        }
    }

    private static void ConvertTypesOrInterface(CompilerContext context, Codeanalysis.Parsing.AST.CompilationUnit tree)
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

            if (st.Name != CodeSymbols.Interface)
            {
                ConvertStructMembers(members, type, context);
            }
            else
            {
                ConvertInterfaceMethods(members, type, context);
            }
        }
    }

    private static void SetReturnType(DescribedBodyMethod method, LNode function, CompilerContext context)
    {
        var retType = function.Args[0];
        method.ReturnParameter = new Parameter(IntermediateStage.GetType(retType, context));
    }
}