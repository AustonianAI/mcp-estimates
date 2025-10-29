using ConstructionEstimation.Core.Models;
using ConstructionEstimation.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ConstructionEstimation.McpServer.Tools;

public class EstimateTools
{
    private readonly AppDbContext _context;

    public EstimateTools(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> ListEstimates(string? clientIdStr = null)
    {
        try
        {
            var query = _context.Estimates.AsQueryable();

            if (!string.IsNullOrEmpty(clientIdStr) && Guid.TryParse(clientIdStr, out var clientId))
            {
                query = query.Where(e => e.ClientId == clientId);
            }

            var estimates = await query
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new
                {
                    e.Id,
                    e.ClientId,
                    e.EstimateNumber,
                    e.Title,
                    e.Description,
                    e.TotalAmount,
                    Status = e.Status.ToString(),
                    e.ValidUntil,
                    e.CreatedAt
                })
                .ToListAsync();

            return JsonSerializer.Serialize(estimates, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in ListEstimates: {ex.Message}");
            return JsonSerializer.Serialize(new { error = "Failed to retrieve estimates", details = ex.Message });
        }
    }

    public async Task<string> GetEstimateDetails(string estimateIdStr)
    {
        try
        {
            if (!Guid.TryParse(estimateIdStr, out var estimateId))
            {
                return JsonSerializer.Serialize(new { error = "Invalid estimate ID format" });
            }

            var estimate = await _context.Estimates
                .Where(e => e.Id == estimateId)
                .Select(e => new
                {
                    e.Id,
                    e.ClientId,
                    ClientName = e.Client.Name,
                    e.EstimateNumber,
                    e.Title,
                    e.Description,
                    e.TotalAmount,
                    Status = e.Status.ToString(),
                    e.ValidUntil,
                    e.CreatedAt,
                    e.UpdatedAt,
                    RelatedInvoices = e.Invoices.Select(i => new
                    {
                        i.Id,
                        i.InvoiceNumber,
                        i.Amount,
                        Status = i.Status.ToString()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (estimate == null)
            {
                return JsonSerializer.Serialize(new { error = "Estimate not found" });
            }

            return JsonSerializer.Serialize(estimate, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetEstimateDetails: {ex.Message}");
            return JsonSerializer.Serialize(new { error = "Failed to retrieve estimate details", details = ex.Message });
        }
    }

    public async Task<string> CreateEstimate(
        string clientIdStr,
        string title,
        string description,
        string totalAmountStr,
        string status = "Draft")
    {
        try
        {
            if (!Guid.TryParse(clientIdStr, out var clientId))
            {
                return JsonSerializer.Serialize(new { error = "Invalid client ID format" });
            }

            if (!decimal.TryParse(totalAmountStr, out var totalAmount))
            {
                return JsonSerializer.Serialize(new { error = "Invalid total amount format" });
            }

            // Validate client exists
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == clientId);
            if (!clientExists)
            {
                return JsonSerializer.Serialize(new { error = "Client not found" });
            }

            // Parse status
            if (!Enum.TryParse<EstimateStatus>(status, true, out var estimateStatus))
            {
                estimateStatus = EstimateStatus.Draft;
            }

            // Generate estimate number
            var year = DateTime.UtcNow.Year;
            var lastEstimate = await _context.Estimates
                .Where(e => e.EstimateNumber.StartsWith($"EST-{year}-"))
                .OrderByDescending(e => e.EstimateNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastEstimate != null)
            {
                var parts = lastEstimate.EstimateNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            var estimateNumber = $"EST-{year}-{nextNumber:D3}";

            // Create estimate
            var estimate = new Estimate
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                EstimateNumber = estimateNumber,
                Title = title,
                Description = description,
                TotalAmount = totalAmount,
                Status = estimateStatus,
                ValidUntil = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Estimates.Add(estimate);
            await _context.SaveChangesAsync();

            var result = new
            {
                success = true,
                message = "Estimate created successfully",
                estimate = new
                {
                    estimate.Id,
                    estimate.EstimateNumber,
                    estimate.Title,
                    estimate.Description,
                    estimate.TotalAmount,
                    Status = estimate.Status.ToString(),
                    estimate.ValidUntil,
                    estimate.CreatedAt
                }
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in CreateEstimate: {ex.Message}");
            return JsonSerializer.Serialize(new { error = "Failed to create estimate", details = ex.Message });
        }
    }

    public async Task<string> UpdateEstimate(
        string estimateIdStr,
        string? title = null,
        string? description = null,
        string? totalAmountStr = null,
        string? status = null,
        string? validUntilStr = null)
    {
        try
        {
            if (!Guid.TryParse(estimateIdStr, out var estimateId))
            {
                return JsonSerializer.Serialize(new { error = "Invalid estimate ID format" });
            }

            // Find the estimate
            var estimate = await _context.Estimates.FindAsync(estimateId);
            if (estimate == null)
            {
                return JsonSerializer.Serialize(new { error = "Estimate not found" });
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(title))
            {
                estimate.Title = title;
            }

            if (!string.IsNullOrEmpty(description))
            {
                estimate.Description = description;
            }

            if (!string.IsNullOrEmpty(totalAmountStr))
            {
                if (!decimal.TryParse(totalAmountStr, out var totalAmount))
                {
                    return JsonSerializer.Serialize(new { error = "Invalid total amount format" });
                }
                estimate.TotalAmount = totalAmount;
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (!Enum.TryParse<EstimateStatus>(status, true, out var estimateStatus))
                {
                    return JsonSerializer.Serialize(new { error = $"Invalid status. Valid values are: {string.Join(", ", Enum.GetNames<EstimateStatus>())}" });
                }
                estimate.Status = estimateStatus;
            }

            if (!string.IsNullOrEmpty(validUntilStr))
            {
                if (!DateTime.TryParse(validUntilStr, out var validUntil))
                {
                    return JsonSerializer.Serialize(new { error = "Invalid validUntil date format" });
                }
                estimate.ValidUntil = validUntil;
            }

            // Update the timestamp
            estimate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new
            {
                success = true,
                message = "Estimate updated successfully",
                estimate = new
                {
                    estimate.Id,
                    estimate.EstimateNumber,
                    estimate.Title,
                    estimate.Description,
                    estimate.TotalAmount,
                    Status = estimate.Status.ToString(),
                    estimate.ValidUntil,
                    estimate.CreatedAt,
                    estimate.UpdatedAt
                }
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in UpdateEstimate: {ex.Message}");
            return JsonSerializer.Serialize(new { error = "Failed to update estimate", details = ex.Message });
        }
    }

    public async Task<string> GetEstimateStatistics()
    {
        try
        {
            var estimates = await _context.Estimates.ToListAsync();

            if (!estimates.Any())
            {
                return JsonSerializer.Serialize(new { message = "No estimates found" });
            }

            var stats = new
            {
                TotalEstimates = estimates.Count,
                TotalValue = estimates.Sum(e => e.TotalAmount),
                AverageValue = estimates.Average(e => e.TotalAmount),
                StatusBreakdown = estimates.GroupBy(e => e.Status)
                    .Select(g => new
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        TotalValue = g.Sum(e => e.TotalAmount)
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList(),
                RecentEstimates = estimates
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(5)
                    .Select(e => new
                    {
                        e.EstimateNumber,
                        e.Title,
                        e.TotalAmount,
                        Status = e.Status.ToString(),
                        e.CreatedAt
                    })
                    .ToList()
            };

            return JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetEstimateStatistics: {ex.Message}");
            return JsonSerializer.Serialize(new { error = "Failed to retrieve estimate statistics", details = ex.Message });
        }
    }
}

