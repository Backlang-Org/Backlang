using Newtonsoft.Json;

namespace Backlang.Driver.Compiling.Targets.Dotnet.RuntimeOptionsModels;

public class FrameworkOptions
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }
}