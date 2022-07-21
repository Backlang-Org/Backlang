using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Driver.Compiling.Targets.Dotnet;
using Flo;
using Furesoft.Core.CodeDom.Compiler;
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

namespace Backlang.Driver.Compiling.Stages;

public sealed class ImplementationStage : IHandler<CompilerContext, CompilerContext>
{
    public static MethodBody CompileBody(LNode function, CompilerContext context, IMethod method,
        QualifiedName? modulename)
    {
        var graph = Utils.CreateGraphBuilder();
        var block = graph.EntryPoint;

        AppendBlock(function.Args[3], block, context, method, modulename);

        return new MethodBody(
            method.ReturnParameter,
            new Parameter(method.ParentType),
            EmptyArray<Parameter>.Value,
            graph.ToImmutable());
    }

    public static IType GetLiteralType(LNode value, TypeResolver resolver)
    {
        if (value.Calls(CodeSymbols.String)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(string));
        else if (value.Calls(CodeSymbols.Char)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(char));
        else if (value.Calls(CodeSymbols.Bool)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(bool));
        else if (value.Calls(CodeSymbols.Int8)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(byte));
        else if (value.Calls(CodeSymbols.Int16)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(short));
        else if (value.Calls(CodeSymbols.UInt16)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(ushort));
        else if (value.Calls(CodeSymbols.Int32)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(int));
        else if (value.Calls(CodeSymbols.UInt32)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(uint));
        else if (value.Calls(CodeSymbols.Int64)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(long));
        else if (value.Calls(CodeSymbols.UInt64)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(ulong));
        else if (value.Calls(Symbols.Float16)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(Half));
        else if (value.Calls(Symbols.Float32)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(float));
        else if (value.Calls(Symbols.Float64)) return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(double));
        else if (value is IdNode id) { } //todo: symbol table

        return ClrTypeEnvironmentBuilder.ResolveType(resolver, typeof(void));
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            var modulename = Utils.GetModuleName(tree);

            foreach (var node in tree.Body)
            {
                ConvertMethodBodies(context);
                CollectImplementations(context, node, modulename);
                ImplementDefaultConstructors(context, node, modulename);
            }
        }

        return await next.Invoke(context);
    }

    private static void ConvertMethodBodies(CompilerContext context)
    {
        foreach (var bodyCompilation in context.BodyCompilations)
        {
            bodyCompilation.method.Body =
                CompileBody(bodyCompilation.function, bodyCompilation.context,
                bodyCompilation.method, bodyCompilation.modulename);
        }
    }

    private static void AppendBlock(LNode blkNode, BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename)
    {
        foreach (var node in blkNode.Args)
        {
            if (!node.IsCall) continue;

            if (node.Calls(Symbols.Block))
            {
                if (node.ArgCount == 0) continue;

                AppendBlock(node, block.Graph.AddBasicBlock(), context, method, modulename);
            }

            if (node.Name == CodeSymbols.Var)
            {
                AppendVariableDeclaration(context, method, block, node, modulename);
            }
            else if (node.Name == (Symbol)"print")
            {
                AppendCall(context, block, node, context.writeMethods, "Write");
            }
            else if (node.Calls(CodeSymbols.Return))
            {
                if (node.ArgCount == 1)
                {
                    var valueNode = node.Args[0];

                    AppendExpression(block, valueNode, (DescribedType)context.Environment.Int32, method); //ToDo: Deduce Type

                    block.Flow = new ReturnFlow();
                }
                else
                {
                    block.Flow = new ReturnFlow();
                }
            }
            else if (node.Calls(CodeSymbols.Throw))
            {
                var valueNode = node.Args[0].Args[0];
                var constant = block.AppendInstruction(ConvertConstant(
                    GetLiteralType(valueNode, context.Binder), valueNode.Value));

                var msg = block.AppendInstruction(Instruction.CreateLoad(GetLiteralType(valueNode, context.Binder), constant));

                if (node.Args[0].Name.Name == "#string")
                {
                    var exceptionType = ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(Exception));
                    var exceptionCtor = exceptionType.Methods.FirstOrDefault(_ => _.IsConstructor && _.Parameters.Count == 1);

                    block.AppendInstruction(Instruction.CreateNewObject(exceptionCtor, new List<ValueTag> { msg }));
                }

                block.Flow = UnreachableFlow.Instance;
            }
            else if (node.Calls(Symbols.ColonColon))
            {
                var callee = node.Args[1];
                var typename = Utils.GetQualifiedName(node.Args[0]);

                var type = (DescribedType)context.Binder.ResolveTypes(typename).FirstOrDefault();

                AppendCall(context, block, callee, type.Methods, callee.Name.Name);
            }
            else
            {
                //ToDo: continue implementing static function call in same type
                var type = method.ParentType;
                var calleeName = node.Target;
                var callee = type.Methods.FirstOrDefault(_ => _.IsStatic && _.Name.ToString() == calleeName.Name.Name);

                if (callee != null)
                {
                    AppendCall(context, block, node, type.Methods);
                }
                else
                {
                    context.AddError(node, $"Cannot find static function '{calleeName.Name.Name}'");
                }
            }
        }
    }

    private static IMethod GetMatchingMethod(CompilerContext context, List<IType> argTypes, IEnumerable<IMethod> methods, string methodname)
    {
        foreach (var m in methods)
        {
            if (m.Name.ToString() != methodname) continue;

            if (m.Parameters.Count == argTypes.Count)
            {
                if (MatchesParameters(m, argTypes))
                    return m;
            }
        }

        return null;
    }

    private static bool MatchesParameters(IMethod method, List<IType> argTypes)
    {
        var methodParams = string.Join(',', method.Parameters.Select(_ => _.Type.FullName.ToString()));
        var monocecilParams = string.Join(',', argTypes.Select(_ => _.FullName.ToString()));

        return methodParams.Equals(monocecilParams, StringComparison.Ordinal);
    }

    private static void AppendCall(CompilerContext context, BasicBlockBuilder block,
        LNode node, IEnumerable<IMethod> methods, string methodName = null)
    {
        var argTypes = new List<IType>();
        var callTags = new List<ValueTag>();

        foreach (var arg in node.Args)
        {
            var type = GetLiteralType(arg, context.Binder);
            argTypes.Add(type);

            var constant = block.AppendInstruction(
            ConvertConstant(type, arg.Args[0].Value));

            block.AppendInstruction(Instruction.CreateLoad(type, constant));

            callTags.Add(constant);
        }

        if (methodName == null)
        {
            methodName = node.Name.Name;
        }

        var method = GetMatchingMethod(context, argTypes, methods, methodName);

        var call = Instruction.CreateCall(method, MethodLookup.Static, callTags);

        block.AppendInstruction(call);
    }

    private static void AppendVariableDeclaration(CompilerContext context, IMethod method, BasicBlockBuilder block, LNode node, QualifiedName? modulename)
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

        var varname = decl.Args[0].Name.Name;
        if (!block.Parameters.Select(_ => _.Tag.Name).Contains(varname))
        {
            block.AppendParameter(new BlockParameter(elementType, varname));
        }
        else
        {
            context.AddError(decl.Args[0], $"{varname} already declared");
        }

        AppendExpression(block, decl.Args[1], elementType, method);

        block.AppendInstruction(Instruction.CreateAlloca(elementType));
    }

    private static NamedInstructionBuilder AppendExpression(BasicBlockBuilder block, LNode node, DescribedType elementType, IMethod method)
    {
        if (node.ArgCount == 1 && node.Args[0].HasValue)
        {
            var constant = ConvertConstant(elementType, node.Args[0].Value);
            var value = block.AppendInstruction(constant);

            return block.AppendInstruction(Instruction.CreateLoad(elementType, value));
        }
        else if (node.ArgCount == 2)
        {
            var lhs = AppendExpression(block, node.Args[0], elementType, method);
            var rhs = AppendExpression(block, node.Args[1], elementType, method);

            return block.AppendInstruction(Instruction.CreateBinaryArithmeticIntrinsic(node.Name.Name.Substring(1), false, elementType, lhs, rhs));
        }
        else if (node.IsId)
        {
            var par = method.Parameters.Where(_ => _.Name.ToString() == node.Name.Name);

            if (!par.Any())
            {
                var localPrms = block.Parameters.Where(_ => _.Tag.Name.ToString() == node.Name.Name);
                if (localPrms.Any())
                {
                    return block.AppendInstruction(Instruction.CreateLoadLocal(new Parameter(localPrms.First().Type, localPrms.First().Tag.Name)));
                }
            }
            else
            {
                return block.AppendInstruction(Instruction.CreateLoadArg(par.First()));
            }
        }

        return null;
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

    private static void ImplementDefaultConstructors(CompilerContext context, LNode st, QualifiedName modulename)
    {
        if (!(st.IsCall && st.Name == CodeSymbols.Struct)) return;

        var name = st.Args[0].Name;
        var type = (DescribedType)context.Binder.ResolveTypes(new SimpleName(name.Name).Qualify(modulename)).FirstOrDefault();

        if (!type.Methods.Any(_ => _.Name.ToString() == "new" && _.Parameters.Count == type.Fields.Count))
        {
            var ctorMethod = new DescribedBodyMethod(type, new SimpleName("new"), true, ClrTypeEnvironmentBuilder.ResolveType(context.Binder, typeof(void)))
            {
                IsConstructor = true
            };

            ctorMethod.AddAttribute(AccessModifierAttribute.Create(AccessModifier.Public));

            foreach (var field in type.Fields)
            {
                ctorMethod.AddParameter(new Parameter(field.FieldType, field.Name));
            }

            var graph = Utils.CreateGraphBuilder();

            var block = graph.EntryPoint;

            for (var i = 0; i < ctorMethod.Parameters.Count; i++)
            {
                var p = ctorMethod.Parameters[i];
                var f = type.Fields[i];

                block.AppendInstruction(Instruction.CreateLoadArg(new Parameter(type))); //this ptr

                block.AppendInstruction(Instruction.CreateLoadArg(p));
                block.AppendInstruction(Instruction.CreateStoreFieldPointer(f));
            }

            block.Flow = new ReturnFlow();

            ctorMethod.Body = new MethodBody(new Parameter(), new Parameter(type), EmptyArray<Parameter>.Value, graph.ToImmutable());

            type.AddMethod(ctorMethod);
        }
    }

    private static void CollectImplementations(CompilerContext context, LNode st, QualifiedName modulename)
    {
        if (!(st.IsCall && st.Name == Symbols.Implementation)) return;

        var typenode = st.Args[0].Args[0].Args[0].Args[0];
        var fullname = Utils.GetQualifiedName(typenode);
        var targetType = TypeInheritanceStage.ResolveTypeWithModule(typenode, context, modulename, fullname);

        var body = st.Args[0].Args[1].Args;

        foreach (var node in body)
        {
            if (node.Name == CodeSymbols.Fn)
            {
                if (targetType.Parent.Assembly == context.Assembly)
                {
                    var fn = TypeInheritanceStage.ConvertFunction(context, targetType, node, modulename);
                    targetType.AddMethod(fn);
                }
                else
                {
                    var fn = TypeInheritanceStage.ConvertFunction(context, context.ExtensionsType, node, modulename);

                    fn.IsStatic = true;

                    var thisParameter = new Parameter(targetType, "this");
                    var param = (IList<Parameter>)fn.Parameters;

                    param.Insert(0, thisParameter);

                    var extType = ClrTypeEnvironmentBuilder
                        .ResolveType(context.Binder, typeof(ExtensionAttribute));

                    fn.AddAttribute(new DescribedAttribute(extType));

                    context.ExtensionsType.AddMethod(fn);
                }
            }
        }
    }
}