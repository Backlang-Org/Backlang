using Backlang.Contracts;
using Backlang.ResourcePreprocessor.Mif.MifFormat;
using System.ComponentModel.Composition;

namespace Backlang.ResourcePreprocessor.Mif;

[Export(typeof(IResourcePreprocessor))]
public class MifPreprocessor : IResourcePreprocessor
{
    public string Extension => ".mif";

    public Stream Preprocess(Stream strm)
    {
        var file = MifParser.Parse(new StreamReader(strm).ReadToEnd());
        var raw = MifTranslator.Translate(file);

        return new MemoryStream(raw);
    }
}