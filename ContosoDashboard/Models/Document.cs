using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class Document
{
    [Key]
    public int DocumentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? Tags { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = DocumentCategories.Other;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileNameOriginal { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileType { get; set; } = string.Empty;

    [Required]
    public long FileSizeBytes { get; set; }

    [Required]
    public int UploadedByUserId { get; set; }

    public int? ProjectId { get; set; }

    [Required]
    public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedDateUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsUnverified { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    public DateTime? DeletedDateUtc { get; set; }

    public DateTime? PurgeAfterUtc { get; set; }

    [Required]
    [MaxLength(100)]
    public string VersionToken { get; set; } = Guid.NewGuid().ToString("N");

    [ForeignKey(nameof(UploadedByUserId))]
    public virtual User UploadedByUser { get; set; } = null!;

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    public virtual ICollection<DocumentShare> Shares { get; set; } = new List<DocumentShare>();
    public virtual ICollection<DocumentActivity> Activities { get; set; } = new List<DocumentActivity>();
    public virtual ICollection<TaskDocumentLink> TaskLinks { get; set; } = new List<TaskDocumentLink>();
}
