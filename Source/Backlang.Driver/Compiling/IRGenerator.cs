using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;

namespace Backlang.Driver.Compiling;

public static class IRGenerator
{
    public static void GenerateGetHashCode(CompilerContext context, DescribedType type)
    {
        var gethashcodeMethod = new DescribedBodyMethod(type, new SimpleName("GetHashCode"),
            false, context.Environment.Int32);
        gethashcodeMethod.IsPublic = true;
        gethashcodeMethod.IsOverride = true;

        var graph = Utils.CreateGraphBuilder();

        var block = graph.EntryPoint;

        block.AppendParameter(new BlockParameter(context.Environment.Int32, "hash"));

        var constant = block.AppendInstruction(
            Instruction.CreateLoad(context.Environment.Int32, block.AppendInstruction(
                Instruction.CreateConstant(new IntegerConstant(17), context.Environment.Int32))
            )
        );

        block.AppendInstruction(Instruction.CreateAlloca(context.Environment.Int32));

        foreach (var field in type.Fields)
        {
            var methods = field.FieldType.Methods.Where(_ => _.Name.ToString() == "GetHashCode");
            if (methods.Any())
            {
                var method = methods.First();

                // hash = hash * 23 + field.GetHashCode();
                var loadHash = Instruction.CreateLoadLocal(new Parameter(context.Environment.Int32, "hash"));
                var hash = block.AppendInstruction(loadHash);
                var c = block.AppendInstruction(
                    Instruction.CreateLoad(context.Environment.Int32, block.AppendInstruction(
                        Instruction.CreateConstant(new IntegerConstant(23), context.Environment.Int32))
                    )
                );

                var multiplication = block.AppendInstruction(Instruction.CreateBinaryArithmeticIntrinsic("*", false, context.Environment.Int32, hash, c));

                block.AppendInstruction(Instruction.CreateLoadArg(new Parameter(type)));
                var instruction = Instruction.CreateLoadField(field);

                if (field.FieldType.BaseTypes.Count() > 0 && field.FieldType.BaseTypes[0].FullName.ToString() == "System.ValueType")
                {
                    instruction = Instruction.CreateGetFieldPointer(field, null);
                }

                var ldField = block.AppendInstruction(instruction);
                var call = block.AppendInstruction(Instruction.CreateCall(method, MethodLookup.Virtual, new List<ValueTag>() { ldField }));

                var addition = block.AppendInstruction(Instruction.CreateBinaryArithmeticIntrinsic("+", false, context.Environment.Int32, ldField, multiplication));

                block.AppendInstruction(Instruction.CreateStore(context.Environment.Int32, hash, addition));
            }
        }

        block.AppendInstruction(Instruction.CreateLoadLocal(new Parameter(context.Environment.Int32, "hash")));
        block.Flow = new ReturnFlow();

        gethashcodeMethod.Body = new MethodBody(new Parameter(), new Parameter(type), EmptyArray<Parameter>.Value, graph.ToImmutable());
        type.AddMethod(gethashcodeMethod);
    }

    public static void GenerateToString(CompilerContext context, DescribedType type)
    {
        var toStringMethod = new DescribedBodyMethod(type, new SimpleName("ToString"), false, Utils.ResolveType(context.Binder, typeof(string)));
        toStringMethod.IsPublic = true;
        toStringMethod.IsOverride = true;

        var graph = Utils.CreateGraphBuilder();

        var block = graph.EntryPoint;

        var sbType = Utils.ResolveType(context.Binder, typeof(StringBuilder));
        var varname = Utils.GenerateIdentifier();
        var p = block.AppendParameter(new BlockParameter(sbType, varname));

        var ctor = sbType.Methods.First(_ => _.IsConstructor && _.Parameters.Count == 0);

        var appendLineMethod = context.Binder.FindFunction("System.Text.StringBuilder::AppendLine(System.String)");

        block.AppendInstruction(Instruction.CreateNewObject(ctor, new List<ValueTag>()));
        block.AppendInstruction(Instruction.CreateAlloca(sbType));

        var loadSb = block.AppendInstruction(Instruction.CreateLoadLocal(new Parameter(p.Type, p.Tag.Name)));

        AppendLine(context, block, appendLineMethod, loadSb, $"{type.FullName}:");

        foreach (var field in type.Fields)
        {
            var loadSbf = block.AppendInstruction(Instruction.CreateLoadLocal(new Parameter(p.Type, p.Tag.Name)));

            AppendLine(context, block, appendLineMethod, loadSbf, field.Name + " = ");
            var value = AppendLoadField(block, field);

            var appendMethod = context.Binder.FindFunction($"System.Text.StringBuilder::Append({field.FieldType.FullName})");

            if (appendMethod == null)
            {
                appendMethod = context.Binder.FindFunction("System.Text.StringBuilder::Append(System.Object)");
            }

            block.AppendInstruction(Instruction.CreateCall(appendMethod, MethodLookup.Virtual, new List<ValueTag> { loadSbf, value }));
        }

        var tsM = context.Binder.FindFunction($"System.Text.StringBuilder::ToString()");

        block.AppendInstruction(Instruction.CreateCall(tsM, MethodLookup.Virtual, new List<ValueTag> { loadSb }));

        block.Flow = new ReturnFlow();

        toStringMethod.Body = new MethodBody(new Parameter(), new Parameter(type), EmptyArray<Parameter>.Value, graph.ToImmutable());

        type.AddMethod(toStringMethod);
    }

    public static void GenerateEmptyCtor(CompilerContext context, DescribedType type)
    {
        var ctorMethod = new DescribedBodyMethod(type, new SimpleName(".ctor"), false, Utils.ResolveType(context.Binder, typeof(void)))
        {
            IsConstructor = true
        };

        ctorMethod.IsPublic = true;

        var graph = Utils.CreateGraphBuilder();

        var block = graph.EntryPoint;

        block.Flow = new ReturnFlow();

        ctorMethod.Body = new MethodBody(new Parameter(), Parameter.CreateThisParameter(type), EmptyArray<Parameter>.Value, graph.ToImmutable());

        type.AddMethod(ctorMethod);
    }

    public static void GenerateDefaultCtor(CompilerContext context, DescribedType type)
    {
        var ctorMethod = new DescribedBodyMethod(type, new SimpleName(".ctor"), false, Utils.ResolveType(context.Binder, typeof(void)))
        {
            IsConstructor = true
        };

        ctorMethod.IsPublic = true;

        foreach (var field in type.Fields)
        {
            ctorMethod.AddParameter(new Parameter(field.FieldType, field.Name.ToString().ToLower()));
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

        ctorMethod.Body = new MethodBody(new Parameter(), Parameter.CreateThisParameter(type), EmptyArray<Parameter>.Value, graph.ToImmutable());

        type.AddMethod(ctorMethod);
    }

    private static void AppendThis(BasicBlockBuilder block, IType type)
    {
        block.AppendInstruction(Instruction.CreateLoadArg(Parameter.CreateThisParameter(type)));
    }

    private static NamedInstructionBuilder AppendLoadField(BasicBlockBuilder block, IField field)
    {
        AppendThis(block, field.ParentType);
        var value = block.AppendInstruction(Instruction.CreateLoadField(field));
        return value;
    }

    private static void AppendLine(CompilerContext context, BasicBlockBuilder block, IMethod appendLineMethod, NamedInstructionBuilder argLoad, string valueStr)
    {
        var va = block.AppendInstruction(
                        Instruction.CreateConstant(new StringConstant(valueStr), context.Environment.String));
        var value = block.AppendInstruction(Instruction.CreateLoad(context.Environment.String, va));

        block.AppendInstruction(Instruction.CreateCall(appendLineMethod, MethodLookup.Virtual, new List<ValueTag> { argLoad, value }));
    }
}