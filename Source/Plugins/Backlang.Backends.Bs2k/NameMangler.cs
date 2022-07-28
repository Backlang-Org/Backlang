using Furesoft.Core.CodeDom.Compiler.Core;
using System.Globalization;
using System.Text;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public class NameMangler
{
    public static string Mangle(IMethod method)
    {
        var sb = new StringBuilder();

        var ns = method.FullName.Slice(0, method.FullName.PathLength - 1).ToString().Replace(".", "%").ToLower();

        if (ns != "%program")
        {
            sb.Append(ns);
        }

        sb.Append('$').Append(method.FullName.FullyUnqualifiedName.ToString());

        foreach (var param in method.Parameters)
        {
            sb.Append('$').Append(MangleTypeName(param.Type));
        }

        return sb.ToString();
    }

    private static string MangleTypeName(IType type)
    {
        var myTI = CultureInfo.InvariantCulture.TextInfo;

        return myTI.ToTitleCase(type.Name.ToString());
    }
}