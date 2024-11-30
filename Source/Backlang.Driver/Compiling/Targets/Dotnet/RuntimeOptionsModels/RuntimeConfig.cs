using Newtonsoft.Json;

namespace Backlang.Driver.Compiling.Targets.Dotnet.RuntimeOptionsModels;

public class RuntimeConfig
{
    [JsonProperty("runtimeOptions")] public RuntimeOptions RuntimeOptions { get; set; } = new();

    public static void Save(string path, string name, CompilerCliOptions options)
    {
        path = Path.Combine(path, name + ".runtimeconfig.json");

        var data = new RuntimeConfig();
        data.RuntimeOptions.Tfm = options.TargetFramework;
        data.RuntimeOptions.Framework.Name = "Microsoft.NETCore.App";
        data.RuntimeOptions.Framework.Version = "7.0.0";

        var json = JsonConvert.SerializeObject(data, Formatting.Indented);

        File.WriteAllText(path, json);
    }

    public static string GetVersion(string frameworkMoniker)
    {
        return frameworkMoniker.Replace("net", "");
    }
}