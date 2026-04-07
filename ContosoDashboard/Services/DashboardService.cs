using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDashboardService
{
    Task<DashboardSummary> GetDashboardSummaryAsync(int userId);
    Task<List<Announcement>> GetActiveAnnouncementsAsync();
    Task<List<DashboardDocumentItem>> GetRecentDocumentsAsync(int userId, int take = 5);
}

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync(int userId)
    {
        var now = DateTime.UtcNow;

        var summary = new DashboardSummary
        {
            TotalActiveTasks = await _context.Tasks
                .CountAsync(t => t.AssignedUserId == userId && t.Status != Models.TaskStatus.Completed),

            TasksDueToday = await _context.Tasks
                .CountAsync(t => t.AssignedUserId == userId 
                    && t.DueDate.HasValue 
                    && t.DueDate.Value.Date == now.Date
                    && t.Status != Models.TaskStatus.Completed),

            ActiveProjects = await _context.Projects
                .Where(p => p.ProjectManagerId == userId || p.ProjectMembers.Any(pm => pm.UserId == userId))
                .Where(p => p.Status == ProjectStatus.Active)
                .CountAsync(),

            UnreadNotifications = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead),

            VisibleDocuments = await BuildVisibleDocumentsQuery(userId)
                .CountAsync()
        };

        return summary;
    }

    public async Task<List<Announcement>> GetActiveAnnouncementsAsync()
    {
        var now = DateTime.UtcNow;

        return await _context.Announcements
            .Include(a => a.CreatedByUser)
            .Where(a => a.IsActive 
                && a.PublishDate <= now 
                && (!a.ExpiryDate.HasValue || a.ExpiryDate.Value > now))
            .OrderByDescending(a => a.PublishDate)
            .Take(5)
            .ToListAsync();
    }

    public async Task<List<DashboardDocumentItem>> GetRecentDocumentsAsync(int userId, int take = 5)
    {
        return await BuildVisibleDocumentsQuery(userId)
            .OrderByDescending(d => d.CreatedDateUtc)
            .Take(Math.Max(1, take))
            .Select(d => new DashboardDocumentItem
            {
                DocumentId = d.DocumentId,
                Title = d.Title,
                UploadedByDisplayName = d.UploadedByUser.DisplayName,
                CreatedDateUtc = d.CreatedDateUtc,
                IsUnverified = d.IsUnverified
            })
            .ToListAsync();
    }

    private IQueryable<Document> BuildVisibleDocumentsQuery(int userId)
    {
        var isAdmin = _context.Users.Any(u => u.UserId == userId && u.Role == UserRole.Administrator);
        var query = _context.Documents
            .AsNoTracking()
            .Include(d => d.UploadedByUser)
            .Where(d => !d.IsDeleted);

        if (isAdmin)
        {
            return query;
        }

        var projectIds = _context.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId);
        var managedProjectIds = _context.Projects
            .Where(p => p.ProjectManagerId == userId)
            .Select(p => p.ProjectId);

        return query.Where(d =>
            d.UploadedByUserId == userId ||
            (d.ProjectId.HasValue && projectIds.Contains(d.ProjectId.Value)) ||
            (d.ProjectId.HasValue && managedProjectIds.Contains(d.ProjectId.Value)) ||
            d.Shares.Any(s => s.SharedWithUserId == userId && s.RevokedDateUtc == null));
    }
}

public class DashboardSummary
{
    public int TotalActiveTasks { get; set; }
    public int TasksDueToday { get; set; }
    public int ActiveProjects { get; set; }
    public int UnreadNotifications { get; set; }
    public int VisibleDocuments { get; set; }
}

public class DashboardDocumentItem
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string UploadedByDisplayName { get; set; } = string.Empty;
    public DateTime CreatedDateUtc { get; set; }
    public bool IsUnverified { get; set; }
}
