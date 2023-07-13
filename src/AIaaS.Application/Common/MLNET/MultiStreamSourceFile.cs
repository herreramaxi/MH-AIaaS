using Microsoft.ML.Data;

namespace AIaaS.Application.Common.Models
{
    public class MultiStreamSourceFile : IMultiStreamSource
    {
        private MemoryStream _stream;

        public MultiStreamSourceFile(MemoryStream stream)
        {
            _stream = stream;
        }

        public int Count => 1;

        public string GetPathOrNull(int index)
        {
            return string.Empty;
        }

        public Stream Open(int index)
        {
            return _stream;
        }

        public TextReader OpenTextReader(int index)
        {
            throw new NotImplementedException();
        }
    }
}
