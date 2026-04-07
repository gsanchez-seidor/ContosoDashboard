using System.Security.Claims;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using ContosoDashboard.Services.Documents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace ContosoDashboard.Pages;

public partial class Documents : ComponentBase
{
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Parameter]
    [SupplyParameterFromQuery(Name = "projectId")]
    public int? ProjectIdFromQuery { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "taskId")]
    public int? TaskIdFromQuery { get; set; }

    protected List<DocumentSummaryDto>? DocumentItems { get; set; }
    protected UploadFormModel Form { get; set; } = new();
    protected string[] Categories { get; } = DocumentCategories.All;

    protected string? SuccessMessage { get; set; }
    protected string? ErrorMessage { get; set; }
    protected string? WarningMessage { get; set; }

    protected bool IsUploading { get; set; }
    protected string InputFileKey { get; set; } = Guid.NewGuid().ToString("N");
    protected string? SelectedFileName { get; set; }
    protected long SelectedFileSize { get; set; }
    protected string SearchText { get; set; } = string.Empty;
    protected string SelectedSort { get; set; } = "uploadDate";
    protected string? SelectedCategoryFilter { get; set; }
    protected int? SelectedProjectFilter { get; set; }

    private IBrowserFile? _selectedFile;
    private int _currentUserId;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var claim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claim, out _currentUserId) || _currentUserId <= 0)
        {
            ErrorMessage = "Unable to resolve current user.";
            return;
        }

        if (ProjectIdFromQuery.HasValue)
        {
            Form.ProjectId = ProjectIdFromQuery.Value;
            SelectedProjectFilter = ProjectIdFromQuery.Value;
        }

        await LoadDocumentsAsync();
    }

    protected async Task OnFileSelected(InputFileChangeEventArgs args)
    {
        _selectedFile = args.File;
        SelectedFileName = _selectedFile.Name;
        SelectedFileSize = _selectedFile.Size;
        await Task.CompletedTask;
    }

    protected async Task UploadAsync()
    {
        SuccessMessage = null;
        ErrorMessage = null;
        WarningMessage = null;

        if (_selectedFile == null)
        {
            ErrorMessage = "Please select a file.";
            return;
        }

        IsUploading = true;

        try
        {
            var fileName = _selectedFile.Name;
            var fileSize = _selectedFile.Size;
            var contentType = _selectedFile.ContentType;

            await using var memoryStream = new MemoryStream();
            await using (var fileStream = _selectedFile.OpenReadStream(25 * 1024 * 1024))
            {
                await fileStream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;

            var result = await DocumentService.UploadDocumentAsync(
                _currentUserId,
                memoryStream,
                fileName,
                contentType,
                fileSize,
                new UploadDocumentRequest
                {
                    Title = Form.Title,
                    Category = Form.Category,
                    Description = Form.Description,
                    ProjectId = Form.ProjectId,
                    Tags = ParseTags(Form.TagsInput)
                });

            SuccessMessage = $"Document '{result.Title}' uploaded successfully.";
            WarningMessage = result.WarningMessage;

            _selectedFile = null;
            SelectedFileName = null;
            SelectedFileSize = 0;
            InputFileKey = Guid.NewGuid().ToString("N");
            Form = new UploadFormModel();

            await LoadDocumentsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsUploading = false;
            StateHasChanged();
        }
    }

    private async Task LoadDocumentsAsync()
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            DocumentItems = await DocumentService.SearchDocumentsAsync(_currentUserId, SearchText);
            return;
        }

        DocumentItems = await DocumentService.GetVisibleDocumentsAsync(_currentUserId, new DocumentListRequest
        {
            Category = SelectedCategoryFilter,
            ProjectId = SelectedProjectFilter,
            Sort = SelectedSort
        });
    }

    protected async Task ApplyFiltersAsync()
    {
        await LoadDocumentsAsync();
    }

    protected async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedSort = "uploadDate";
        SelectedCategoryFilter = null;
        SelectedProjectFilter = ProjectIdFromQuery;
        await LoadDocumentsAsync();
    }

    protected static string FormatSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        var kb = bytes / 1024d;
        if (kb < 1024)
        {
            return $"{kb:F1} KB";
        }

        return $"{kb / 1024d:F1} MB";
    }

    private static List<string> ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return [];
        }

        return tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    protected class UploadFormModel
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = DocumentCategories.ProjectDocuments;
        public string? Description { get; set; }
        public int? ProjectId { get; set; }
        public string? TagsInput { get; set; }
    }
}
