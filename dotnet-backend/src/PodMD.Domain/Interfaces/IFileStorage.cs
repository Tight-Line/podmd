namespace PodMD.Domain.Interfaces;

public interface IFileStorage
{
    /// <summary>
    /// Uploads a file stream to storage and returns the storage key
    /// </summary>
    /// <param name="fileName">The original file name</param>
    /// <param name="contentType">The MIME content type</param>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="storageKey">The storage key to use</param>
    /// <returns>The storage key used</returns>
    Task<string> UploadAsync(string fileName, string contentType, Stream fileStream, string storageKey);

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    /// <param name="storageKey">The storage key of the file</param>
    /// <returns>The file stream</returns>
    Task<Stream> DownloadAsync(string storageKey);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="storageKey">The storage key of the file</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(string storageKey);

    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    /// <param name="storageKey">The storage key of the file</param>
    /// <returns>True if exists</returns>
    Task<bool> ExistsAsync(string storageKey);
}
