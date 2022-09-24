using Backlang.ResourcePreprocessor.Mif;

namespace MifTest
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var strm = File.OpenRead("Sample.mif");
            var result = (MemoryStream)new MifPreprocessor().Preprocess(strm);

            File.WriteAllBytes("Sample.bin", result.ToArray());
        }
    }
}