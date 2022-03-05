using Backlang_Compiler.Compiling.Passes.Lowerer;
using Backlang_Compiler.Parsing.AST;
using Flo;

namespace Backlang_Compiler.Compiling.Stages
{
    public class LowererStage : IHandler<CompilerContext, CompilerContext>
    {
        private PassManager _optimization = new();

        public LowererStage()
        {
            _optimization.AddPass<OperatorAssignLowererPass>();
        }

        public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
        {
            for (int i = 0; i < context.Trees.Count; i++)
            {
                context.Trees[i].Body = (Block)_optimization.Process(context.Trees[i].Body);
            }

            return await next.Invoke(context);
        }
    }
}