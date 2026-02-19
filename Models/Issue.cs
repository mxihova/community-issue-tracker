using System.ComponentModel.DataAnnotations;

namespace Community_Issue_Tracker.Models
{
    public class Issue
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        // Now Category is structured using enum
        [Required]
        public IssueCategory Category { get; set; }

        // NEW: Priority field
        [Required]
        public IssuePriority Priority { get; set; }

        public IssueStatus Status { get; set; } = IssueStatus.Open;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedByUserId { get; set; }
    }

    public enum IssueCategory
    {
        Potholes,
        Lighting,
        Sanitation,
        Safety,
        Other
    }

    public enum IssuePriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum IssueStatus
    {
        Open,
        InProgress,
        Resolved,
        Closed
    }
}
