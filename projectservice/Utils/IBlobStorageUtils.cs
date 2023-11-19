namespace projectservice.Utils
{
    public interface IBlobStorageUtils
    {
        Task<Uri> UploadFileIfNotExistsAsync(byte[] fileData, string fileName, string projectName);
        Task<bool> DeleteFileAsync(string fileName);
    }
}
