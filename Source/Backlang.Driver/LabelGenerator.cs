namespace Backlang.Driver;

public static class LabelGenerator
{
    private static int labelCounter;

    public static string NewLabel(string name)
    {
        return $"{name}_{labelCounter++}";
    }
}