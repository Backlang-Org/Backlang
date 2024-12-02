using Backlang.Codeanalysis.Properties;
using System.Resources;

namespace Backlang.Codeanalysis.Core;

public readonly struct LocalizableString
{
    public readonly ErrorID ErrorID;
    public readonly string[] Arguments;
    public readonly string FallbackValue;

    private static readonly ResourceManager _resourceManager =
        new("Backlang.CodeAnalysisProperties.Resources", typeof(Resources).Assembly);

    public LocalizableString(ErrorID errorID, params string[] arguments)
    {
        ErrorID = errorID;
        Arguments = arguments;
        FallbackValue = null;
    }

    public LocalizableString(ErrorID errorID) : this()
    {
        ErrorID = errorID;
        Arguments = new[] { "NO_VALUE" };
    }

    public LocalizableString(string fallbackValue) : this()
    {
        FallbackValue = fallbackValue;
    }

    public static implicit operator string(LocalizableString lstr)
    {
        if (!string.IsNullOrEmpty(lstr.FallbackValue))
        {
            return lstr.FallbackValue;
        }

        var resourceID = $"BL({(int)lstr.ErrorID:D4})";

        return string.Format(resourceID + ": " + _resourceManager.GetString(resourceID), lstr.Arguments);
    }

    public static implicit operator LocalizableString(ErrorID id)
    {
        return new LocalizableString(id);
    }

    public static implicit operator LocalizableString(string message)
    {
        return new LocalizableString(message);
    }
}