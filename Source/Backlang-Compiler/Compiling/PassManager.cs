using Backlang_Compiler.Parsing.AST;

namespace Backlang_Compiler.Compiling;

public class PassManager
{
    private readonly List<IPass> Passes = new();

    public void AddPass<T>()
        where T : IPass, new()
    {
        Passes.Add(new T());
    }

    public SyntaxNode Process(SyntaxNode obj)
    {
        var result = new Block();

        if (obj is Block body)
        {
            foreach (var t in body.Body)
            {
                if (t is Block blk) result.Body.Add(ProcessBlock(blk));

                Process(result, t);
            }

            body.Body = result.Body;
        }

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

    private void Process(Block result, SyntaxNode t)
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