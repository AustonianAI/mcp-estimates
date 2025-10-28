namespace ConstructionEstimation.Core.Models;

public class Invoice
{
    public Guid Id { get; set; }
    public Guid? EstimateId { get; set; }
    public Guid ClientId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Estimate? Estimate { get; set; }
    public Client Client { get; set; } = null!;
}

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    Overdue
}

