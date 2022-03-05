namespace Backlang_Compiler.Compiling.Passes.Lowerer
{
    public class IncrementOperatorLowererPass : IPass
    {
        public CodeObject Process(CodeObject obj, PassManager passManager)
        {
            if (obj is PostIncrement incr)
            {
                return new Block(incr.Expression, new Assignment(incr.Expression, incr.Expression + 1));
            }
            else if (obj is PostDecrement decr)
            {
                return new Block(decr.Expression, new Assignment(decr.Expression, decr.Expression - 1));
            }
            else
            {
                return obj;
            }
        }
    }
}
