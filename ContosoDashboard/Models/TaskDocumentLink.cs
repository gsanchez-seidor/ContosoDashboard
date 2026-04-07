using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class TaskDocumentLink
{
    [Key]
    public int TaskDocumentLinkId { get; set; }

    [Required]
    public int TaskId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public int LinkedByUserId { get; set; }

    [Required]
    public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TaskId))]
    public virtual TaskItem Task { get; set; } = null!;

    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;

    [ForeignKey(nameof(LinkedByUserId))]
    public virtual User LinkedByUser { get; set; } = null!;
}
