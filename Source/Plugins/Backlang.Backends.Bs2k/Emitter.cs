using Backlang.Contracts;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Text;
using MethodBody = Furesoft.Core.CodeDom.Compiler.MethodBody;

namespace Backlang.Backends.Bs2k;

public class Emitter
{
    private readonly IMethod _mainMethod;
    private readonly StringBuilder _builder = new();

    private readonly Dictionary<int, string> _stringConstants = new();

    public Emitter(IMethod mainMethod)
    {
        _mainMethod = mainMethod;
    }

    public void EmitFunctionDefinition(DescribedBodyMethod method)
    {
        var signature = NameMangler.Mangle(method);

        Emit($"{signature}:", null, 0);

        if (method == _mainMethod)
        {
            Emit("add sp, 0, R0", "reserve stack space for local variables");
            Emit("copy sp, R0", "save current stack pointer into R0 (this is the new stack frame base pointer)");
            Emit("add sp, 0, R0", "reserve stack space for main function");
        }

        Emit("");

        EmitMethodBody(method, method.Body);

        Emit("");

        if (method == _mainMethod)
        {
            Emit("halt");
            Emit("");
        }
    }

    public void EmitResource(string name, byte[] data)
    {
        Emit($"res{name}: .words[{string.Join(",", data)}]");
    }

    public void EmitStringConstants(IType program)
    {
        foreach (DescribedBodyMethod method in program.Methods)
        {
            var instructions = method.Body.Implementation.BasicBlocks.SelectMany(_ => _.NamedInstructions);
            var strings = instructions.Where(_ => _.Prototype is ConstantPrototype cP && cP.Value is StringConstant);

            foreach (var str in strings)
            {
                var constant = (ConstantPrototype)str.Prototype;
                var value = (StringConstant)constant.Value;
                var strValue = value.Value;
                var hashCode = strValue.GetHashCode();

                Emit($"str{hashCode}: .string \"{strValue}\"", null, 0);
                _stringConstants.Add(hashCode, strValue);
            }
        }

        Emit("\n", null, 0);
    }

    public override string ToString() => _builder.ToString();

    public void Emit(string instruction, string comment = null, int indentlevel = 1)
    {
        _builder.Append(new string('\t', indentlevel));

        if (comment == null)
        {
            _builder.AppendLine(instruction);
            return;
        }

        _builder.AppendLine($"{instruction} // {comment}");
    }

    private void EmitBinary(IntrinsicPrototype arith, int indentlevel)
    {
        Emit("pop R2", $"store rhs for {arith.Name}-operator in R1", indentlevel);
        Emit("pop R1", $"store lhs for {arith.Name}-operator in R1", indentlevel);

        switch (arith.Name)
        {
            case "arith.+": Emit("add R1, R2, R3", "push result onto stack", indentlevel); break;
            case "arith.*": Emit("mult R1, R2, R3, R4", "multiply values", indentlevel); break;
            case "arith.|": Emit("or R1, R2, R3", null, indentlevel); break;
            case "arith.&": Emit("and R1, R2, R3", null, indentlevel); break;
            case "arith.^": Emit("xor R1, R2, R3", null, indentlevel); break;
            case "arith.==": Emit("comp_eq R1, R2, R3", null, indentlevel); break;
            case "arith.!=": Emit("comp_neq R1, R2, R3", null, indentlevel); break;
            case "arith.<": Emit("comp_lt R1, R2, R3", null, indentlevel); break;
            case "arith.<=": Emit("comp_le R1, R2, R3", null, indentlevel); break;
            case "arith.>": Emit("comp_gt R1, R2, R3", null, indentlevel); break;
            case "arith.>=": Emit("comp_ge R1, R2, R3", null, indentlevel); break;
            default:
                break;
        }

        Emit("push R3", "push result onto stack", indentlevel);

        Emit("");
    }

    //Todo: Fix intrinsic double emit constant
    private void EmitConstant(ConstantPrototype consProto, int indentlevel)
    {
        if (consProto.Value is IntegerConstant ic)
        {
            Emit("//push immeditate onto stack", null, indentlevel);
            Emit($"copy {ic.ToInt32()}, R1", null, indentlevel);
            Emit("push R1", null, indentlevel);

            Emit("");
        }
    }

    private void EmitVariableDeclaration(NamedInstruction item, AllocaPrototype allocA, int indentlevel)
    {
    }

    //ToDo: Rewrite call to match coders code
    private void EmitCall(Instruction instruction, FlowGraph implementation, int indentlevel)
    {
        var prototype = (CallPrototype)instruction.Prototype;
    }

    private void EmitMethodBody(DescribedBodyMethod method, MethodBody body)
    {
        foreach (var block in method.Body.Implementation.BasicBlocks)
        {
            EmitBlock(block, 1);
        }
    }

    private void EmitBlock(BasicBlock block, int indentlevel)
    {
        if (!string.IsNullOrEmpty(block.Tag.Name) && !block.IsEntryPoint)
        {
            Emit($"{block.Tag.Name}:", null, indentlevel++);
        }

        foreach (var item in block.NamedInstructions)
        {
            EmitItem(item, block, indentlevel);
        }

        EmitBlockFlow(block, indentlevel);
    }

    private void EmitItem(NamedInstruction item, BasicBlock block, int indentlevel)
    {
        var instruction = item.Instruction;

        if (instruction.Prototype is AllocaPrototype allocA)
        {
            EmitVariableDeclaration(item, allocA, indentlevel);
            return;
        }

        EmitInstruction(block, instruction, indentlevel);
    }

    private void EmitInstruction(BasicBlock block, Instruction instruction, int indentlevel)
    {
        switch (instruction.Prototype)
        {
            case CallPrototype callPrototype:
                {
                    if (IntrinsicHelper.IsIntrinsicType(typeof(Intrinsics), callPrototype))
                    {
                        var intrinsic = IntrinsicHelper.InvokeIntrinsic(typeof(Intrinsics),
                                        callPrototype.Callee, instruction, block).ToString();

                        Emit("//emited from intrinsic", null, indentlevel);
                        foreach (var intr in intrinsic.Split("\n"))
                        {
                            Emit(intr, null, indentlevel);
                        }

                        return;
                    }

                    var callee = callPrototype.Callee
                            .ParentType.FullName.FullyUnqualifiedName.ToString() == Names.FreeFunctions
                           ? callPrototype.Callee.Name.ToString() : callPrototype.Callee.FullName.ToString();

                    Emit($"// Calling '{callee}'", null, indentlevel);
                    EmitCall(instruction, block.Graph, indentlevel);
                    break;
                }

            case NewObjectPrototype newObjectPrototype:
                break;

            case LoadPrototype loadPrototype:
                var consProto = (ConstantPrototype)block.Graph.GetInstruction(instruction.Arguments[0]).Prototype;
                EmitConstant(consProto, indentlevel);
                break;

            case IntrinsicPrototype arith:
                EmitBinary(arith, indentlevel);
                break;
        }
    }

    private void EmitBlockFlow(BasicBlock block, int indentlevel)
    {
        if (block.Flow is ReturnFlow rf)
        {
            if (rf.HasReturnValue)
            {
                EmitInstruction(block, rf.ReturnValue, indentlevel);
            }

            Emit("copy R0, sp", "clear current stack frame", indentlevel);
            Emit("pop R0", "restore previous stack frame", indentlevel);
            Emit("return", null, indentlevel);
        }
        else if (block.Flow is JumpFlow jf)
        {
            Emit("jump " + jf.Branch.Target.Name, null, indentlevel);
        }
        else if (block.Flow is JumpConditionalFlow jcf)
        {
            Emit("jump_gt R1, " + jcf.Branch.Target.Name, null, indentlevel);
        }
    }
}