namespace AIaaS.Application.Common.ExtensionMethods
{
    public static class MemoryStreamExtensions
    {
        public static MemoryStream ToMemoryStream(this Stream stream)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

    }
}
