using ContosoDashboard.Models;
using ContosoDashboard.Services.Documents;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<UploadDocumentResult> UploadDocumentAsync(int userId, Stream fileStream, string originalFileName, string contentType, long fileSizeBytes, UploadDocumentRequest request, CancellationToken cancellationToken = default);
    Task<List<DocumentSummaryDto>> GetMyDocumentsAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<DocumentSummaryDto>> GetVisibleDocumentsAsync(int userId, DocumentListRequest request, CancellationToken cancellationToken = default);
    Task<List<DocumentSummaryDto>> SearchDocumentsAsync(int userId, string query, CancellationToken cancellationToken = default);
    Task<List<DocumentSummaryDto>> GetProjectDocumentsAsync(int userId, int projectId, CancellationToken cancellationToken = default);
    Task<List<DocumentSummaryDto>> GetSharedDocumentsAsync(int userId, CancellationToken cancellationToken = default);
    Task<DocumentDetailDto?> GetDocumentDetailsAsync(int documentId, int requestingUserId, CancellationToken cancellationToken = default);
    Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId, CancellationToken cancellationToken = default);
    Task<DocumentDetailDto> UpdateMetadataAsync(int userId, int documentId, UpdateDocumentMetadataRequest request, CancellationToken cancellationToken = default);
    Task<DocumentDetailDto> ReplaceDocumentAsync(int userId, int documentId, Stream fileStream, string originalFileName, string contentType, long fileSizeBytes, string expectedVersionToken, CancellationToken cancellationToken = default);
    Task<DownloadDocumentResult> DownloadAsync(int userId, int documentId, bool inlinePreview, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int userId, int documentId, CancellationToken cancellationToken = default);
    Task<ShareDocumentResult> ShareAsync(int userId, int documentId, List<int> recipientUserIds, CancellationToken cancellationToken = default);
    Task<bool> LinkDocumentToTaskAsync(int userId, int taskId, int documentId, CancellationToken cancellationToken = default);
    Task<List<TaskDocumentLinkDto>> GetTaskDocumentsAsync(int userId, int taskId, CancellationToken cancellationToken = default);
    Task<List<DocumentSummaryDto>> GetRecentDocumentsAsync(int userId, int take = 5, CancellationToken cancellationToken = default);
    Task<int> GetVisibleDocumentCountAsync(int userId, CancellationToken cancellationToken = default);
}
