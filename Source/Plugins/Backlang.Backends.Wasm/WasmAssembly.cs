using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Loyc.Collections;
using WebAssembly;
using WebAssembly.Instructions;

namespace Backlang.Backends.Wasm;

public class WasmAssembly : ITargetAssembly
{
    private AssemblyContentDescription contents;

    public WasmAssembly(AssemblyContentDescription contents)
    {
        this.contents = contents;
    }

    public void WriteTo(Stream output)
    {
        var module = new Module();

        module.Memories.Add(new Memory(1, 1));

        foreach (var t in contents.Assembly.Types)
        {
            for (var i = 0; i < t.Methods.Count; i++)
            {
                var m = (DescribedBodyMethod)t.Methods[i];
                var type = new WebAssemblyType();

                if (m.ReturnParameter.Type.Name.ToString() != "Void")
                {
                    type.Returns = new[] { GetWasmType(m.ReturnParameter.Type) };
                }

                module.Types.Add(type);

                module.Functions.Add(new Function
                {
                    Type = (uint)module.Types.Count - 1,
                });

                var locals = GetLocals(m);

                module.Codes.Add(new FunctionBody(CompileBody(m.Body, locals)) { Locals = locals.Values.ToList() });

                if (m == contents.EntryPoint)
                {
                    module.Start = (uint)module.Functions.Count - 1;
                }
            }
        }

        module.WriteToBinary(output);
    }

    private Dictionary<string, Local> GetLocals(DescribedBodyMethod m)
    {
        var locals = new Dictionary<string, Local>();

        foreach (var local in m.Body.Implementation
            .BasicBlocks.SelectMany(_ => _.Parameters))
        {
            locals.Add(local.Tag.Name, new Local { Type = GetWasmType(local.Type), Count = 1 });
        }

        return locals;
    }

    private WebAssembly.Instruction[] CompileBody(MethodBody body, Dictionary<string, Local> locals)
    {
        var instructions = new List<WebAssembly.Instruction>();
        foreach (var block in body.Implementation.BasicBlocks)
        {
            instructions.AddRange(CompileBlock(block, locals));
        }

        return instructions.ToArray();
    }

    private IEnumerable<WebAssembly.Instruction> CompileBlock(BasicBlock block, Dictionary<string, Local> locals)
    {
        foreach (var instr in block.NamedInstructions)
        {
            if (instr.Prototype is LoadPrototype ld)
            {
                var constant = block.Graph.GetInstruction(instr.Instruction.Arguments[0]);

                var value = ((ConstantPrototype)constant.Prototype).Value;

                if (value is IntegerConstant ic)
                {
                    if (ic.Spec.Size == 4)
                    {
                        if (ic.Spec.IsSigned)
                        {
                            yield return new Int32Constant(ic.ToInt32());
                        }
                        else
                        {
                            yield return new Int32Constant(ic.ToUInt32());
                        }
                    }
                }
            }
            else if (instr.Prototype is LoadLocalPrototype ll)
            {
                yield return new LocalGet((uint)locals.Keys.IndexOf(ll.Parameter.Name.ToString()));
            }
            else if (instr.Prototype is StorePrototype aa)
            {
                yield return new LocalSet((uint)locals.Keys.IndexOf(""));
            }
        }

        if (block.Flow is ReturnFlow rf)
        {
            yield return new WebAssembly.Instructions.End();
        }
    }

    private WebAssemblyValueType GetWasmType(IType type)
    {
        if (type.Name.ToString() == "Int32")
        {
            return WebAssemblyValueType.Int32;
        }
        else if (type.Name.ToString() == "Int64")
        {
            return WebAssemblyValueType.Int64;
        }

        return WebAssemblyValueType.Int32;
    }
}