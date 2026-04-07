namespace ContosoDashboard.Services;

// Training default scanner: returns unavailable so uploads are marked unverified.
public class DefaultScanService : IScanService
{
    public Task<ScanResult> ScanAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ScanResult
        {
            IsAvailable = false,
            IsClean = true,
            Message = "Scanner unavailable in training mode."
        });
    }
}
