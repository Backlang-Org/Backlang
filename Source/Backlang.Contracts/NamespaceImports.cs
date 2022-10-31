using Backlang.Driver;
using Backlang.Codeanalysis.Core;

namespace Backlang.Contracts;

public class NamespaceImports
{
    public readonly List<QualifiedName> ImportedNamespaces = new();

    public void ImportNamespace(LNode importStatement, CompilerContext context)
    {
        if (importStatement is ("#import", var ns))
        {
            var qualifiedNs = ConversionUtils.GetQualifiedName(ns);

            if (ImportedNamespaces.Contains(qualifiedNs))
            {
                context.AddError(importStatement, new(ErrorID.NamespaceAlreadyImported, qualifiedNs.ToString()));
                return;
            }

            ImportedNamespaces.Add(qualifiedNs);
        }
    }


}
