using Backlang.Contracts;
using System.ComponentModel.Composition;

namespace Backlang.ResourcePreprocessor.Mif
{
    [Export(typeof(IResourcePreprocessor))]
    public class MifPreprocessor : IResourcePreprocessor
    {
        public string Extension => ".mif";

        public Stream Preprocess(Stream strm)
        {
            var file = MifFormat.MifParser.Parse(new StreamReader(strm).ReadToEnd());
            var raw = MifFormat.MifTranslator.Translate(file);

            return new MemoryStream(raw);
        }
    }
}