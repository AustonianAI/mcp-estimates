using System.Text.Json.Serialization;

namespace ConstructionEstimation.Core.Models;

public class Estimate
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string EstimateNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public EstimateStatus Status { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Client Client { get; set; } = null!;
    [JsonIgnore]
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

public enum EstimateStatus
{
    Draft,
    Sent,
    Approved,
    Rejected
}

