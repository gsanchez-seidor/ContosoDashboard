namespace ContosoDashboard.Services;

public interface IScanService
{
    Task<ScanResult> ScanAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
}

public class ScanResult
{
    public bool IsClean { get; set; }
    public bool IsAvailable { get; set; }
    public string? Message { get; set; }
}
