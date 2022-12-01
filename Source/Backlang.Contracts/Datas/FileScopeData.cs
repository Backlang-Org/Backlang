namespace Backlang.Contracts;

public struct FileScopeData
{
    public FileScopeData()
    {
        ImportetNamespaces = new();
    }

    public Dictionary<string, NamespaceImports> ImportetNamespaces { get; set; }
}
