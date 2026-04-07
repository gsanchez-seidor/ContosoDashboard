using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services.Documents;
using ContosoDashboard.Services.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ContosoDashboard.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storageService;
    private readonly IScanService _scanService;
    private readonly DocumentAuthorizationService _authorizationService;
    private readonly IOptions<DocumentStorageOptions> _storageOptions;
    private readonly INotificationService _notificationService;

    public DocumentService(
        ApplicationDbContext context,
        IFileStorageService storageService,
        IScanService scanService,
        DocumentAuthorizationService authorizationService,
        IOptions<DocumentStorageOptions> storageOptions,
        INotificationService notificationService)
    {
        _context = context;
        _storageService = storageService;
        _scanService = scanService;
        _authorizationService = authorizationService;
        _storageOptions = storageOptions;
        _notificationService = notificationService;
    }

    public async Task<UploadDocumentResult> UploadDocumentAsync(
        int userId,
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        UploadDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUploadRequest(fileSizeBytes, originalFileName, request);

        var canUpload = await _authorizationService.CanUploadToProjectAsync(userId, request.ProjectId);
        if (!canUpload)
        {
            throw new InvalidOperationException("You are not authorized to upload to this project.");
        }

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var savedPath = await _storageService.UploadAsync(fileStream, userId, request.ProjectId, extension, cancellationToken);

        fileStream.Position = 0;
        var scan = await _scanService.ScanAsync(fileStream, originalFileName, contentType, cancellationToken);

        var isUnverified = !scan.IsAvailable;
        var warning = isUnverified ? "Upload completed but file is unverified because scanning is unavailable." : null;

        var document = new Document
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Tags = NormalizeTags(request.Tags),
            Category = request.Category,
            FilePath = savedPath,
            FileNameOriginal = originalFileName,
            FileType = contentType,
            FileSizeBytes = fileSizeBytes,
            UploadedByUserId = userId,
            ProjectId = request.ProjectId,
            CreatedDateUtc = DateTime.UtcNow,
            UpdatedDateUtc = DateTime.UtcNow,
            IsUnverified = isUnverified,
            IsDeleted = false,
            VersionToken = Guid.NewGuid().ToString("N")
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        _context.DocumentActivities.Add(new DocumentActivity
        {
            DocumentId = document.DocumentId,
            ActorUserId = userId,
            ActivityType = isUnverified ? "ScanUnavailable" : "Upload",
            CreatedDateUtc = DateTime.UtcNow,
            Details = isUnverified ? scan.Message : "Document uploaded successfully"
        });

        await _context.SaveChangesAsync(cancellationToken);
        await NotifyProjectMembersOnUploadAsync(document, userId, cancellationToken);

        return new UploadDocumentResult
        {
            DocumentId = document.DocumentId,
            Title = document.Title,
            IsUnverified = isUnverified,
            WarningMessage = warning
        };
    }

    public async Task<List<DocumentSummaryDto>> GetMyDocumentsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .Include(d => d.UploadedByUser)
            .Where(d => !d.IsDeleted && d.UploadedByUserId == userId)
            .OrderByDescending(d => d.CreatedDateUtc)
            .Select(ToSummaryProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DocumentSummaryDto>> GetVisibleDocumentsAsync(int userId, DocumentListRequest request, CancellationToken cancellationToken = default)
    {
        var query = await BuildVisibleDocumentsQueryAsync(userId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(d => d.Category == request.Category);
        }

        if (request.ProjectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == request.ProjectId);
        }

        if (request.FromDateUtc.HasValue)
        {
            query = query.Where(d => d.CreatedDateUtc >= request.FromDateUtc.Value);
        }

        if (request.ToDateUtc.HasValue)
        {
            query = query.Where(d => d.CreatedDateUtc <= request.ToDateUtc.Value);
        }

        query = request.Sort.ToLowerInvariant() switch
        {
            "title" => query.OrderBy(d => d.Title),
            "category" => query.OrderBy(d => d.Category).ThenByDescending(d => d.CreatedDateUtc),
            "filesize" => query.OrderByDescending(d => d.FileSizeBytes),
            _ => query.OrderByDescending(d => d.CreatedDateUtc)
        };

        return await query
            .Select(ToSummaryProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DocumentSummaryDto>> SearchDocumentsAsync(int userId, string query, CancellationToken cancellationToken = default)
    {
        var filtered = await BuildVisibleDocumentsQueryAsync(userId, cancellationToken);

        var normalized = query.Trim().ToLowerInvariant();
        filtered = filtered.Where(d =>
            d.Title.ToLower().Contains(normalized) ||
            (d.Description != null && d.Description.ToLower().Contains(normalized)) ||
            (d.Tags != null && d.Tags.ToLower().Contains(normalized)) ||
            d.UploadedByUser.DisplayName.ToLower().Contains(normalized) ||
            (d.Project != null && d.Project.Name.ToLower().Contains(normalized)));

        return await filtered
            .OrderByDescending(d => d.CreatedDateUtc)
            .Select(ToSummaryProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DocumentSummaryDto>> GetProjectDocumentsAsync(int userId, int projectId, CancellationToken cancellationToken = default)
    {
        var visible = await BuildVisibleDocumentsQueryAsync(userId, cancellationToken);
        return await visible
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.CreatedDateUtc)
            .Select(ToSummaryProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DocumentSummaryDto>> GetSharedDocumentsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .Include(d => d.UploadedByUser)
            .Where(d => !d.IsDeleted)
            .Where(d => d.Shares.Any(s => s.SharedWithUserId == userId && s.RevokedDateUtc == null))
            .OrderByDescending(d => d.CreatedDateUtc)
            .Select(ToSummaryProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents
            .Include(d => d.Shares)
            .Include(d => d.UploadedByUser)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted, cancellationToken);

        if (document == null)
        {
            return null;
        }

        var canView = await _authorizationService.CanViewDocumentAsync(document, requestingUserId);
        return canView ? document : null;
    }

    public async Task<DocumentDetailDto?> GetDocumentDetailsAsync(int documentId, int requestingUserId, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentByIdAsync(documentId, requestingUserId, cancellationToken);
        return document == null ? null : ToDetailDto(document);
    }

    public async Task<DocumentDetailDto> UpdateMetadataAsync(int userId, int documentId, UpdateDocumentMetadataRequest request, CancellationToken cancellationToken = default)
    {
        var document = await GetEditableDocumentAsync(userId, documentId, cancellationToken);
        if (document.VersionToken != request.ExpectedVersionToken)
        {
            throw new InvalidOperationException("Version conflict detected. Reload and confirm your changes.");
        }

        document.Title = request.Title.Trim();
        document.Category = request.Category.Trim();
        document.Description = request.Description?.Trim();
        document.Tags = NormalizeTags(request.Tags);
        document.UpdatedDateUtc = DateTime.UtcNow;
        document.VersionToken = Guid.NewGuid().ToString("N");

        await AddActivityAsync(document.DocumentId, userId, "MetadataUpdate", "Updated title/category/description/tags", cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDetailDto(document);
    }

    public async Task<DocumentDetailDto> ReplaceDocumentAsync(
        int userId,
        int documentId,
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        string expectedVersionToken,
        CancellationToken cancellationToken = default)
    {
        var document = await GetEditableDocumentAsync(userId, documentId, cancellationToken);
        if (document.VersionToken != expectedVersionToken)
        {
            throw new InvalidOperationException("Version conflict detected. Reload and confirm your changes.");
        }

        ValidateUploadRequest(fileSizeBytes, originalFileName, new UploadDocumentRequest
        {
            Title = document.Title,
            Category = document.Category
        });

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var newPath = await _storageService.UploadAsync(fileStream, userId, document.ProjectId, extension, cancellationToken);
        await _storageService.DeleteAsync(document.FilePath, cancellationToken);

        fileStream.Position = 0;
        var scan = await _scanService.ScanAsync(fileStream, originalFileName, contentType, cancellationToken);

        document.FilePath = newPath;
        document.FileNameOriginal = originalFileName;
        document.FileType = contentType;
        document.FileSizeBytes = fileSizeBytes;
        document.IsUnverified = !scan.IsAvailable;
        document.UpdatedDateUtc = DateTime.UtcNow;
        document.VersionToken = Guid.NewGuid().ToString("N");

        await AddActivityAsync(document.DocumentId, userId, "Replace", "Replaced document content", cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDetailDto(document);
    }

    public async Task<DownloadDocumentResult> DownloadAsync(int userId, int documentId, bool inlinePreview, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentByIdAsync(documentId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        var isAdmin = await _authorizationService.IsAdminAsync(userId);
        if (document.IsUnverified && !isAdmin)
        {
            throw new UnauthorizedAccessException("Unverified documents are blocked for non-admin users.");
        }

        var stream = await _storageService.DownloadAsync(document.FilePath, cancellationToken);
        await AddActivityAsync(document.DocumentId, userId, inlinePreview ? "Preview" : "Download", null, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new DownloadDocumentResult
        {
            Stream = stream,
            FileName = document.FileNameOriginal,
            ContentType = string.IsNullOrWhiteSpace(document.FileType) ? "application/octet-stream" : document.FileType
        };
    }

    public async Task<bool> SoftDeleteAsync(int userId, int documentId, CancellationToken cancellationToken = default)
    {
        var document = await GetEditableDocumentAsync(userId, documentId, cancellationToken);

        document.IsDeleted = true;
        document.DeletedDateUtc = DateTime.UtcNow;
        document.PurgeAfterUtc = DateTime.UtcNow.AddDays(30);
        document.UpdatedDateUtc = DateTime.UtcNow;

        await AddActivityAsync(document.DocumentId, userId, "SoftDelete", "Document soft-deleted with 30-day retention.", cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ShareDocumentResult> ShareAsync(int userId, int documentId, List<int> recipientUserIds, CancellationToken cancellationToken = default)
    {
        var document = await GetEditableDocumentAsync(userId, documentId, cancellationToken);
        if (recipientUserIds.Count == 0)
        {
            return new ShareDocumentResult { DocumentId = documentId, SharedCount = 0 };
        }

        var isAdmin = await _authorizationService.IsAdminAsync(userId);
        var sharedByDisplayName = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(cancellationToken) ?? "A teammate";

        var distinctRecipients = recipientUserIds.Distinct().Where(id => id > 0 && id != userId).ToList();
        foreach (var recipientId in distinctRecipients)
        {
            if (document.ProjectId.HasValue && !isAdmin)
            {
                var recipientCanUpload = await _authorizationService.CanUploadToProjectAsync(recipientId, document.ProjectId);
                if (!recipientCanUpload)
                {
                    throw new InvalidOperationException("One or more recipients are not authorized for this project document.");
                }
            }

            var existingShare = await _context.DocumentShares.FirstOrDefaultAsync(s =>
                s.DocumentId == document.DocumentId &&
                s.SharedWithUserId == recipientId,
                cancellationToken);

            if (existingShare == null)
            {
                _context.DocumentShares.Add(new DocumentShare
                {
                    DocumentId = document.DocumentId,
                    SharedByUserId = userId,
                    SharedWithUserId = recipientId,
                    CreatedDateUtc = DateTime.UtcNow,
                    RevokedDateUtc = null
                });
            }
            else
            {
                existingShare.RevokedDateUtc = null;
                existingShare.CreatedDateUtc = DateTime.UtcNow;
            }

            await _notificationService.NotifyDocumentSharedAsync(recipientId, document.Title, sharedByDisplayName);
        }

        await AddActivityAsync(document.DocumentId, userId, "Share", $"Shared with {distinctRecipients.Count} users.", cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ShareDocumentResult
        {
            DocumentId = document.DocumentId,
            SharedCount = distinctRecipients.Count
        };
    }

    public async Task<bool> LinkDocumentToTaskAsync(int userId, int taskId, int documentId, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(t => t.TaskId == taskId, cancellationToken);
        if (task == null)
        {
            return false;
        }

        var taskAccess = task.AssignedUserId == userId
            || task.CreatedByUserId == userId
            || task.Project?.ProjectManagerId == userId
            || (task.Project?.ProjectMembers.Any(pm => pm.UserId == userId) ?? false);
        if (!taskAccess)
        {
            return false;
        }

        var document = await GetDocumentByIdAsync(documentId, userId, cancellationToken);
        if (document == null)
        {
            return false;
        }

        if (task.ProjectId.HasValue && document.ProjectId.HasValue && task.ProjectId != document.ProjectId)
        {
            return false;
        }

        var exists = await _context.TaskDocumentLinks.AnyAsync(
            l => l.TaskId == taskId && l.DocumentId == documentId,
            cancellationToken);
        if (exists)
        {
            return true;
        }

        _context.TaskDocumentLinks.Add(new TaskDocumentLink
        {
            TaskId = taskId,
            DocumentId = documentId,
            LinkedByUserId = userId,
            CreatedDateUtc = DateTime.UtcNow
        });

        await AddActivityAsync(documentId, userId, "TaskLink", $"Linked to task {taskId}", cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<TaskDocumentLinkDto>> GetTaskDocumentsAsync(int userId, int taskId, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(t => t.TaskId == taskId, cancellationToken);
        if (task == null)
        {
            return [];
        }

        var taskAccess = task.AssignedUserId == userId
            || task.CreatedByUserId == userId
            || task.Project?.ProjectManagerId == userId
            || (task.Project?.ProjectMembers.Any(pm => pm.UserId == userId) ?? false);
        if (!taskAccess)
        {
            return [];
        }

        return await _context.TaskDocumentLinks
            .AsNoTracking()
            .Where(l => l.TaskId == taskId && !l.Document.IsDeleted)
            .Select(l => new TaskDocumentLinkDto
            {
                TaskId = l.TaskId,
                DocumentId = l.DocumentId,
                Title = l.Document.Title,
                Category = l.Document.Category,
                FileSizeBytes = l.Document.FileSizeBytes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DocumentSummaryDto>> GetRecentDocumentsAsync(int userId, int take = 5, CancellationToken cancellationToken = default)
    {
        var query = await BuildVisibleDocumentsQueryAsync(userId, cancellationToken);
        return await query
            .OrderByDescending(d => d.CreatedDateUtc)
            .Take(Math.Max(1, take))
            .Select(ToSummaryProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetVisibleDocumentCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var query = await BuildVisibleDocumentsQueryAsync(userId, cancellationToken);
        return await query.CountAsync(cancellationToken);
    }

    private void ValidateUploadRequest(long fileSizeBytes, string originalFileName, UploadDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            throw new InvalidOperationException("Category is required.");
        }

        if (fileSizeBytes <= 0)
        {
            throw new InvalidOperationException("File cannot be empty.");
        }

        if (fileSizeBytes > _storageOptions.Value.MaxFileSizeBytes)
        {
            throw new InvalidOperationException("File size exceeds maximum allowed size (25 MB).");
        }

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !_storageOptions.Value.AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }
    }

    private async Task<IQueryable<Document>> BuildVisibleDocumentsQueryAsync(int userId, CancellationToken cancellationToken)
    {
        var baseQuery = _context.Documents
            .AsNoTracking()
            .Include(d => d.UploadedByUser)
            .Include(d => d.Project)
            .Where(d => !d.IsDeleted);

        if (await _authorizationService.IsAdminAsync(userId))
        {
            return baseQuery;
        }

        var projectIds = await _context.ProjectMembers
            .AsNoTracking()
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync(cancellationToken);
        var managedProjectIds = await _context.Projects
            .AsNoTracking()
            .Where(p => p.ProjectManagerId == userId)
            .Select(p => p.ProjectId)
            .ToListAsync(cancellationToken);

        var allProjectIds = projectIds.Union(managedProjectIds).ToList();

        return baseQuery.Where(d =>
            d.UploadedByUserId == userId ||
            (d.ProjectId.HasValue && allProjectIds.Contains(d.ProjectId.Value)) ||
            d.Shares.Any(s => s.SharedWithUserId == userId && s.RevokedDateUtc == null));
    }

    private static System.Linq.Expressions.Expression<Func<Document, DocumentSummaryDto>> ToSummaryProjection()
    {
        return d => new DocumentSummaryDto
        {
            DocumentId = d.DocumentId,
            Title = d.Title,
            Category = d.Category,
            Description = d.Description,
            Tags = d.Tags ?? string.Empty,
            FileSizeBytes = d.FileSizeBytes,
            FileType = d.FileType,
            ProjectId = d.ProjectId,
            CreatedDateUtc = d.CreatedDateUtc,
            UploadedByDisplayName = d.UploadedByUser.DisplayName,
            IsUnverified = d.IsUnverified
        };
    }

    private DocumentDetailDto ToDetailDto(Document document)
    {
        return new DocumentDetailDto
        {
            DocumentId = document.DocumentId,
            Title = document.Title,
            Category = document.Category,
            Description = document.Description,
            Tags = document.Tags ?? string.Empty,
            FileSizeBytes = document.FileSizeBytes,
            FileType = document.FileType,
            ProjectId = document.ProjectId,
            CreatedDateUtc = document.CreatedDateUtc,
            UploadedByDisplayName = document.UploadedByUser.DisplayName,
            IsUnverified = document.IsUnverified,
            SharedWithUserIds = document.Shares.Where(s => s.RevokedDateUtc == null).Select(s => s.SharedWithUserId).ToList(),
            VersionToken = document.VersionToken
        };
    }

    private async Task<Document> GetEditableDocumentAsync(int userId, int documentId, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .Include(d => d.Shares)
            .Include(d => d.UploadedByUser)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        var isAdmin = await _authorizationService.IsAdminAsync(userId);
        if (!isAdmin && document.UploadedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to modify this document.");
        }

        return document;
    }

    private static string NormalizeTags(List<string> tags)
    {
        if (tags.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(",", tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private async Task AddActivityAsync(int documentId, int actorUserId, string activityType, string? details, CancellationToken cancellationToken)
    {
        _context.DocumentActivities.Add(new DocumentActivity
        {
            DocumentId = documentId,
            ActorUserId = actorUserId,
            ActivityType = activityType,
            Details = details,
            CreatedDateUtc = DateTime.UtcNow
        });

        await Task.CompletedTask;
    }

    private async Task NotifyProjectMembersOnUploadAsync(Document document, int uploaderId, CancellationToken cancellationToken)
    {
        if (!document.ProjectId.HasValue)
        {
            return;
        }

        var uploaderName = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserId == uploaderId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(cancellationToken) ?? "A teammate";

        var recipients = await _context.ProjectMembers
            .AsNoTracking()
            .Where(pm => pm.ProjectId == document.ProjectId.Value && pm.UserId != uploaderId)
            .Select(pm => pm.UserId)
            .ToListAsync(cancellationToken);

        foreach (var recipientId in recipients)
        {
            await _notificationService.NotifyProjectDocumentUploadedAsync(recipientId, document.Title, uploaderName);
        }
    }
}
