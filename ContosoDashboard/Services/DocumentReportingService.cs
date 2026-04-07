using ContosoDashboard.Data;
using ContosoDashboard.Services.Documents;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public interface IDocumentReportingService
{
    Task<List<DocumentActivityReportItem>> GetActivitySummaryAsync(DateTime? fromUtc = null, DateTime? toUtc = null);
}

public class DocumentReportingService : IDocumentReportingService
{
    private readonly ApplicationDbContext _context;

    public DocumentReportingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DocumentActivityReportItem>> GetActivitySummaryAsync(DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var query = _context.DocumentActivities.AsNoTracking().AsQueryable();

        if (fromUtc.HasValue)
        {
            query = query.Where(a => a.CreatedDateUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(a => a.CreatedDateUtc <= toUtc.Value);
        }

        return await query
            .GroupBy(a => a.ActivityType)
            .Select(g => new DocumentActivityReportItem
            {
                ActivityType = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(r => r.Count)
            .ToListAsync();
    }
}
