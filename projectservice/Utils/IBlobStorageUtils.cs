namespace projectservice.Utils
{
    public interface IBlobStorageUtils
    {
        Task<Uri> UploadFileAsync(byte[] fileData, string fileName, string projectName);
    }
}
