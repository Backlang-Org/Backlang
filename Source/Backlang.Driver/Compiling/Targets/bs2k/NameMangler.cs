using Furesoft.Core.CodeDom.Compiler.Core;
using System.Text;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public class NameMangler
{
    public static string Mangle(IMethod method)
    {
        var sb = new StringBuilder();

        var ns = method.FullName.Slice(0, method.FullName.PathLength - 1).ToString().Replace(".", "%");

        sb.Append(ns).Append("$").Append(method.FullName.FullyUnqualifiedName.ToString());

        foreach (var param in method.Parameters)
        {
            sb.Append("$").Append(MangleTypeName(param.Type));
        }

        return sb.ToString();
    }

    private static string MangleTypeName(IType type)
    {
        if (type.FullName.Qualifier.ToString() == "System")
        {
            return type.Name.ToString().ToUpper();
        }

        return string.Empty;
    }
}