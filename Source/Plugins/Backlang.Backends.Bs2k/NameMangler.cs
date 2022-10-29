using Furesoft.Core.CodeDom.Compiler.Core;
using System.Globalization;
using System.Text;

namespace Backlang.Backends.Bs2k;

public class NameMangler
{
    public static string Mangle(IMethod method)
    {
        var sb = new StringBuilder();

        var ns = method.FullName.Slice(0, method.FullName.PathLength - 1).ToString().Replace(".", "%").ToLower();

        sb.Append("$\"");

        if (ns != "%freefunctions")
        {
            sb.Append(ns);
        }

        sb.Append(method.FullName.FullyUnqualifiedName.ToString());

        foreach (var param in method.Parameters)
        {
            sb.Append('$').Append(MangleTypeName(param.Type));
        }

        sb.Append('"');

        return sb.ToString();
    }

    private static string MangleTypeName(IType type)
    {
        var myTI = CultureInfo.InvariantCulture.TextInfo;

        return myTI.ToTitleCase(type.Name.ToString());
    }
}