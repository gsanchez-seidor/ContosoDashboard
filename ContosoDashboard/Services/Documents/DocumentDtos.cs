using Microsoft.AspNetCore.Http;

namespace ContosoDashboard.Services.Documents;

public class UploadDocumentApiRequest
{
    public IFormFile File { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ProjectId { get; set; }
    public string? Tags { get; set; }
}

public class UploadDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ProjectId { get; set; }
    public List<string> Tags { get; set; } = [];
}

public class UploadDocumentResult
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsUnverified { get; set; }
    public string? WarningMessage { get; set; }
}

public class DocumentSummaryDto
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Tags { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileType { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public DateTime CreatedDateUtc { get; set; }
    public string UploadedByDisplayName { get; set; } = string.Empty;
    public bool IsUnverified { get; set; }
}

public class DocumentDetailDto : DocumentSummaryDto
{
    public List<int> SharedWithUserIds { get; set; } = [];
    public string VersionToken { get; set; } = string.Empty;
}

public class DocumentListRequest
{
    public string? Category { get; set; }
    public int? ProjectId { get; set; }
    public DateTime? FromDateUtc { get; set; }
    public DateTime? ToDateUtc { get; set; }
    public string Sort { get; set; } = "uploadDate";
}

public class UpdateDocumentMetadataRequest
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = [];
    public string ExpectedVersionToken { get; set; } = string.Empty;
}

public class ReplaceDocumentApiRequest
{
    public IFormFile File { get; set; } = null!;
    public string ExpectedVersionToken { get; set; } = string.Empty;
}

public class ShareDocumentApiRequest
{
    public List<int> RecipientUserIds { get; set; } = [];
}

public class ShareDocumentResult
{
    public int DocumentId { get; set; }
    public int SharedCount { get; set; }
}

public class DownloadDocumentResult
{
    public Stream Stream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
}

public class TaskDocumentLinkDto
{
    public int TaskId { get; set; }
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}

public class DocumentActivityReportItem
{
    public string ActivityType { get; set; } = string.Empty;
    public int Count { get; set; }
}
