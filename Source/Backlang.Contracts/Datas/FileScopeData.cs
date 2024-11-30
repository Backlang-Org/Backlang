namespace Backlang.Contracts;

public struct FileScopeData
{
    public FileScopeData()
    {
        ImportetNamespaces = new Dictionary<string, NamespaceImports>();
    }

    public Dictionary<string, NamespaceImports> ImportetNamespaces { get; set; }
}