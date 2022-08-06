using Backlang.ResourcePreprocessor.Mif.MifFormat.AST;
using Backlang.ResourcePreprocessor.Mif.MifFormat.AST.DataRules;

namespace Backlang.ResourcePreprocessor.Mif.MifFormat;

public class MifTranslator
{
    public static byte[] Translate(MifFile file)
    {
        var wordWidth = (int)file.Options["WIDTH"];
        var wordCount = (int)file.Options["DEPTH"];

        var byteCount = wordWidth <= 8 ? wordCount : wordCount * wordWidth;
        var buffer = new byte[byteCount];

        foreach (var rule in file.DataRules)
        {
            if (rule is SimpleDataRule sdr)
            {
                SetBytes(wordWidth, buffer, sdr.Address, sdr.Value);
            }
            else if (rule is RangeDataRule rdr)
            {
                var addrRange = Enumerable.Range(rdr.From, Math.Abs(rdr.From - rdr.To) + 1);

                foreach (var addr in addrRange)
                {
                    SetBytes(wordWidth, buffer, addr, rdr.Value);
                }
            }
        }

        return buffer;
    }

    private static void SetBytes(int wordWidth, byte[] buffer, int address, long value)
    {
        var bytes = GetBytes(value, wordWidth);

        Array.Copy(bytes, 0, buffer, address, bytes.Length);
    }

    private static byte[] GetBytes(long value, int wordWidth)
    {
        var bytes = BitConverter.GetBytes(value);
        int byteCount = wordWidth / 8;

        return bytes.Take(byteCount).ToArray();
    }
}