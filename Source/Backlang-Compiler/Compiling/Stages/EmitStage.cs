using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;
using Flo;

namespace Backlang_Compiler.Compiling.Stages;

public class EmitStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            var functions = tree.Body.Body.OfType<FunctionDeclaration>();

            if (functions.Any())
            {
                var entryFn = functions.FirstOrDefault(f => f.Name.Text == "main");

                if (entryFn != null)
                {
                    var emitter = new Emitter();
                    var asmEmitter = new AssemblyEmitter(emitter);

                    foreach (var node in entryFn.Body.Body)
                    {
                        if (node is AssemblerBlockStatement asm)
                        {
                            var body = asmEmitter.Emit(asm);

                            File.WriteAllBytes(context.OutputFilename, body);
                        }
                    }

                    return await next.Invoke(context);
                }
            }
        }

        return await next.Invoke(context);
    }
}