using Backlang_Compiler.Parsing.AST;
using Backlang_Compiler.Compiling;

namespace Backlang_Compiler.Compiling;

public class PassManager
{
    private readonly List<IPass> Passes = new();

    public void AddPass<T>()
        where T : IPass, new()
    {
        Passes.Add(new T());
    }

    public CompilationUnit Process(CompilationUnit obj)
    {
        var result = new Block();

        foreach (var t in obj.Body.Body)
        {
            if (t is Block blk) result.Body.Add(ProcessBlock(blk));

            Process(result, t);
        }

        obj.Body = result;

        return obj;
    }

    public Block ProcessBlock(Block block)
    {
        var result = new Block();

        foreach (var t in block.Body)
        {
            if (t is Block blk)
            {
                result.Body.Add(ProcessBlock(blk));
                continue;
            }

            Process(result, t);
        }

        return result;
    }

    private void Process(Block result, CompilationUnit t)
    {
        foreach (var pass in Passes)
        {
            var processedObj = pass.Process(t, this);
            if (processedObj is Block blk)
            {
                result.Body.AddRange(blk.Body);
            }
            else
            {
                result.Body.Add(processedObj);
            }
        }
    }
}
