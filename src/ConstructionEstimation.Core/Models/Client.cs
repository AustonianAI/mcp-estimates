using System.Text.Json.Serialization;

namespace ConstructionEstimation.Core.Models;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public ICollection<Estimate> Estimates { get; set; } = new List<Estimate>();
    [JsonIgnore]
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

