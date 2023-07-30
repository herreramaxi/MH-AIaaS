namespace AIaaS.Domain.Interfaces
{
    public interface IDataViewFile
    {
        public long Size { get; set; }
        string S3Key { get; }
    }
}
