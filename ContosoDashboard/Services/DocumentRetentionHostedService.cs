using ContosoDashboard.Data;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public class DocumentRetentionHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentRetentionHostedService> _logger;

    public DocumentRetentionHostedService(IServiceProvider serviceProvider, ILogger<DocumentRetentionHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PurgeExpiredDocumentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during document retention purge cycle.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task PurgeExpiredDocumentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        var now = DateTime.UtcNow;
        var toPurge = await db.Documents
            .Where(d => d.IsDeleted && d.PurgeAfterUtc.HasValue && d.PurgeAfterUtc <= now)
            .ToListAsync(cancellationToken);

        if (!toPurge.Any())
        {
            return;
        }

        foreach (var document in toPurge)
        {
            await storage.DeleteAsync(document.FilePath, cancellationToken);
        }

        db.Documents.RemoveRange(toPurge);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purged {Count} expired documents.", toPurge.Count);
    }
}
