using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Backlang.Contracts;

public sealed class PluginContainer
{
    [ImportMany(typeof(ICompilationTarget))]
    public List<ICompilationTarget> Targets { get; set; }

    [ImportMany(typeof(IResourcePreprocessor))]
    public List<IResourcePreprocessor> Preprocessors { get; set; }

    public static PluginContainer Load()
    {
        var pluginDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Backlang", "Plugins");

        if (!Directory.Exists(pluginDir))
        {
            Directory.CreateDirectory(pluginDir);

            return null;
        }

        var catalog = new DirectoryCatalog(pluginDir);
        var container = new CompositionContainer(catalog);

        var plugins = new PluginContainer();
        container.ComposeParts(plugins);

        return plugins;
    }
}