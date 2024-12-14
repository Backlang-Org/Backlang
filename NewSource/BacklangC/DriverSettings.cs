using CommandLine;

namespace BacklangC;

public class DriverSettings : LanguageSdk.Templates.Core.DriverSettings
{
    [Option('m')]
    public IEnumerable<string>? MacroReferences { get; set; } = new List<string>();
}