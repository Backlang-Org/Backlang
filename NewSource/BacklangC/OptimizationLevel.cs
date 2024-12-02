using System.Text;
using DistIL.Passes;

namespace BacklangC;

public class OptimizationLevel(string level)
{
    public string Level { get; } = level;
    public List<OptimizationPass> Passes { get; } = new();

    public void AddPass(OptimizationPass pass)
    {
        Passes.Add(pass);
    }

    public void AddPass(IMethodPass pass)
    {
        var mp = new OptimizationPass(ConvertToKebabCase(pass.GetType().Name))
        {
            Pass = pass
        };

        AddPass(mp);
    }

    private static string ConvertToKebabCase(string input)
    {
        var result = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            var currentChar = input[i];

            if (char.IsUpper(currentChar) && i > 0)
            {
                result.Append('-');
            }

            result.Append(char.ToLower(currentChar));
        }

        return result.ToString();
    }
}