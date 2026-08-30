using FinalProject_SeventhSem.Domain.Common;

namespace FinalProject_SeventhSem.Domain.Entities;

public class ResourceRating : BaseEntity
{
    public int ResourceId { get; set; }
    public int StudentId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }
    public DateTime RatedAt { get; set; } = DateTime.UtcNow;

    public Resource Resource { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
