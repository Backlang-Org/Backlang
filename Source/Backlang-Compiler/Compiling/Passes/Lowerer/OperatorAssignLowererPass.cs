using Backlang_Compiler.Parsing.AST;

namespace Backlang_Compiler.Compiling.Passes.Lowerer
{
    public class OperatorAssignLowererPass : IPass
    {
        public SyntaxNode Process(SyntaxNode obj, PassManager passManager)
        {
            return obj switch
            {
                //AddAssign addass => new Assignment(addass.Left, addass.Left + addass.Right),
                //SubtractAssign subass => new Assignment(subass.Left, subass.Left - subass.Right),
                _ => obj
            };
        }
    }
}