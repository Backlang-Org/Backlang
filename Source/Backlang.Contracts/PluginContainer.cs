using System.ComponentModel.Composition;

namespace Backlang.Driver;

public sealed class PluginContainer
{
    [ImportMany(typeof(ICompilationTarget))]
    public List<ICompilationTarget> Targets { get; set; }
}