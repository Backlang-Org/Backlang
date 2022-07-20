using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Text;
using MethodBody = Furesoft.Core.CodeDom.Compiler.MethodBody;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public class Emitter
{
    private readonly IMethod _mainMethod;
    private StringBuilder _builder = new();

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
            Emit("copy sp, R0", "save current stack pointer into R0 (this is the new stack frame base pointer)");
        }

        Emit("");

        EmitMethodBody(method, method.Body);

        Emit("");

        if (method == _mainMethod)
        {
            Emit("halt");
        }
        else
        {
            Emit("copy R0, sp", "clear current stack frame");
            Emit("pop R0", "restore previous stack frame");
            Emit("return");
        }
    }

    public override string ToString() => _builder.ToString();

    public void Emit(string instruction, string comment = null, int indentlevel = 1)
    {
        if (indentlevel == 1)
        {
            _builder.Append('\t');
        }

        if (comment == null)
        {
            _builder.AppendLine(instruction);
            return;
        }

        _builder.AppendLine($"{instruction} // {comment}");
    }

    private void EmitBinary(IntrinsicPrototype arith)
    {
        Emit("pop R2", $"store rhs for {arith.Name}-operator in R1");
        Emit("pop R1", $"store lhs for {arith.Name}-operator in R1");

        switch (arith.Name)
        {
            case "arith.+": Emit("add R1, R2, R3", "push result onto stack"); break;
            case "arith.*": Emit("mult R1, R2, R3, R4", "multiply values"); break;
            case "arith.|": Emit("or R1, R2, R3"); break;
            case "arith.&": Emit("and R1, R2, R3"); break;
            case "arith.^": Emit("xor R1, R2, R3"); break;
            default:
                break;
        }

        Emit("push R3", "push result onto stack");

        Emit("");
    }

    //Todo: Fix intrinsic double emit constant
    private void EmitConstant(ConstantPrototype consProto)
    {
        if (consProto.Value is IntegerConstant ic)
        {
            Emit("//push immeditate onto stack");
            Emit($"copy {ic.ToInt32()}, R1");
            Emit("push R1");

            Emit("");
        }
    }

    private void EmitVariableDeclaration(NamedInstruction item, AllocaPrototype allocA)
    {
    }

    private void EmitCall(Instruction instruction, FlowGraph implementation)
    {
        var prototype = (CallPrototype)instruction.Prototype;

        Emit($"copy {NameMangler.Mangle(prototype.Callee)}, R1", "get address of label");
        Emit("push R1", "push address of label onto stack");
        Emit("pop R5", "get jump address");

        Emit("copy sp, R6", "store address of return address placeholder");
        Emit("add sp, 4, sp", "reserve stack space for the return address placeholder");

        Emit("push R0", "store current stack frame base pointer for later");
        Emit("copy sp, R7", "this will be the new stack frame base pointer");

        Emit("", "evaluate arguments in current stack frame");
        /*
        for (const auto&argument : expression.arguments) {
            argument->accept(*this);
        }
        */

        Emit("copy R7, R0", "set the new stack frame base pointer");
        Emit("");

        Emit("add ip, 24, R1", "calculate return address");
        Emit("copy R1, *R6", "fill return address placeholder");
        Emit("jump R5", "call function");

        // after the call the return value is inside R1
        Emit("push R1", "push return value onto stack");
    }

    private void EmitMethodBody(DescribedBodyMethod method, MethodBody body)
    {
        foreach (var block in method.Body.Implementation.BasicBlocks)
        {
            EmitBlock(block);
        }
    }

    private void EmitBlock(BasicBlock block)
    {
        if (!string.IsNullOrEmpty(block.Tag.Name) && !block.IsEntryPoint)
        {
            Emit($"{block.Tag.Name}:", null, 0);
        }

        foreach (var item in block.NamedInstructions)
        {
            var instruction = item.Instruction;

            if (instruction.Prototype is CallPrototype callPrototype)
            {
                if (IntrinsicHelper.IsIntrinsicType(typeof(Intrinsics), callPrototype))
                {
                    var intrinsic = IntrinsicHelper.InvokeIntrinsic(typeof(Intrinsics),
                                    callPrototype.Callee, instruction, block).ToString();

                    Emit("//emited from intrinsic");
                    foreach (var intr in intrinsic.Split("\n"))
                    {
                        Emit(intr);
                    }

                    continue;
                }

                Emit($"// Calling '{callPrototype.Callee.FullName}'");
                EmitCall(instruction, block.Graph);
            }
            else if (instruction.Prototype is NewObjectPrototype newObjectPrototype)
            {
                //EmitNewObject(newObjectPrototype);
            }
            else if (instruction.Prototype is ConstantPrototype consProto)
            {
                EmitConstant(consProto);
            }
            else if (instruction.Prototype is AllocaPrototype allocA)
            {
                EmitVariableDeclaration(item, allocA);
            }
            else if (instruction.Prototype is IntrinsicPrototype arith)
            {
                EmitBinary(arith);
            }
        }
    }
}