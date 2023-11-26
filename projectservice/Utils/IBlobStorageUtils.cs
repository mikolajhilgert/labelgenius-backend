namespace projectservice.Utils
{
    public interface IBlobStorageUtils
    {
        Task DeleteContainer(string containerName);
        Task<Dictionary<string, string>> UploadFilesAsync(List<(string originalFileName, byte[] content)> files, string projectId);
        Task<string> GetContainerSASTokenAsync(string containerName);
    }
}
