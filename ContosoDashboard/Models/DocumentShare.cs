using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentShare
{
    [Key]
    public int DocumentShareId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public int SharedByUserId { get; set; }

    [Required]
    public int SharedWithUserId { get; set; }

    [Required]
    public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedDateUtc { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;

    [ForeignKey(nameof(SharedByUserId))]
    public virtual User SharedByUser { get; set; } = null!;

    [ForeignKey(nameof(SharedWithUserId))]
    public virtual User SharedWithUser { get; set; } = null!;
}
