namespace ContosoDashboard.Services.Storage;

public class DocumentStorageOptions
{
    public const string SectionName = "DocumentStorage";

    public string UploadRootPath { get; set; } = "AppData/uploads";

    public long MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024;

    public List<string> AllowedExtensions { get; set; } =
    [
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".ppt",
        ".pptx",
        ".txt",
        ".jpeg",
        ".jpg",
        ".png"
    ];
}
