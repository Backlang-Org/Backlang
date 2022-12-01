using Newtonsoft.Json;

namespace Backlang.Driver.Compiling.Targets.Dotnet.RuntimeOptionsModels;

public class RuntimeOptions
{
    [JsonProperty("tfm")]
    public string Tfm { get; set; }

    [JsonProperty("framework")]
    public FrameworkOptions Framework { get; set; } = new();
}