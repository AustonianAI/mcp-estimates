using ConstructionEstimation.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ConstructionEstimation.McpServer.Tools;

public class InvoiceTools
{
    private readonly AppDbContext _context;

    public InvoiceTools(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> ListInvoices(string? clientIdStr = null, string? estimateIdStr = null)
    {
        try
        {
            var query = _context.Invoices.AsQueryable();

            if (!string.IsNullOrEmpty(clientIdStr) && Guid.TryParse(clientIdStr, out var clientId))
            {
                query = query.Where(i => i.ClientId == clientId);
            }

            if (!string.IsNullOrEmpty(estimateIdStr) && Guid.TryParse(estimateIdStr, out var estimateId))
            {
                query = query.Where(i => i.EstimateId == estimateId);
            }

            var invoices = await query
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new
                {
                    i.Id,
                    i.ClientId,
                    i.EstimateId,
                    i.InvoiceNumber,
                    i.Description,
                    i.Amount,
                    Status = i.Status.ToString(),
                    i.DueDate,
                    i.PaidDate,
                    i.CreatedAt
                })
                .ToListAsync();

            return JsonSerializer.Serialize(invoices, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in ListInvoices: {ex.Message}");
            return JsonSerializer.Serialize(new { error = "Failed to retrieve invoices", details = ex.Message });
        }
    }

    public async Task<string> GetClientFinancialSummary(string clientIdStr)
    {
        try
        {
            if (!Guid.TryParse(clientIdStr, out var clientId))
            {
                return JsonSerializer.Serialize(new { error = "Invalid client ID format" });
            }

            var client = await _context.Clients
                .Where(c => c.Id == clientId)
                .FirstOrDefaultAsync();

            if (client == null)
            {
                return JsonSerializer.Serialize(new { error = "Client not found" });
            }

            var estimates = await _context.Estimates
                .Where(e => e.ClientId == clientId)
                .ToListAsync();

            var invoices = await _context.Invoices
                .Where(i => i.ClientId == clientId)
                .ToListAsync();

            var summary = new
            {
                ClientId = clientId,
                ClientName = client.Name,
                Estimates = new
                {
                    Total = estimates.Count,
                    TotalValue = estimates.Sum(e => e.TotalAmount),
                    ByStatus = estimates.GroupBy(e => e.Status)
                        .Select(g => new
                        {
                            Status = g.Key.ToString(),
                            Count = g.Count(),
                            TotalValue = g.Sum(e => e.TotalAmount)
                        })
                        .ToList()
                },
                Invoices = new
                {
                    Total = invoices.Count,
                    TotalBilled = invoices.Sum(i => i.Amount),
                    TotalPaid = invoices.Where(i => i.Status == Core.Models.InvoiceStatus.Paid).Sum(i => i.Amount),
                    TotalOutstanding = invoices.Where(i => i.Status != Core.Models.InvoiceStatus.Paid).Sum(i => i.Amount),
                    ByStatus = invoices.GroupBy(i => i.Status)
                        .Select(g => new
                        {
                            Status = g.Key.ToString(),
                            Count = g.Count(),
                            TotalAmount = g.Sum(i => i.Amount)
                        })
                        .ToList(),
                    OverdueInvoices = invoices
                        .Where(i => i.Status == Core.Models.InvoiceStatus.Overdue)
                        .Select(i => new
                        {
                            i.InvoiceNumber,
                            i.Amount,
                            i.DueDate,
                            DaysOverdue = i.DueDate.HasValue
                                ? (DateTime.UtcNow - i.DueDate.Value).Days
                                : 0
                        })
                        .ToList()
                },
                RecentActivity = new
                {
                    RecentEstimates = estimates
                        .OrderByDescending(e => e.CreatedAt)
                        .Take(3)
                        .Select(e => new
                        {
                            e.EstimateNumber,
                            e.Title,
                            e.TotalAmount,
                            Status = e.Status.ToString(),
                            e.CreatedAt
                        })
                        .ToList(),
                    RecentInvoices = invoices
                        .OrderByDescending(i => i.CreatedAt)
                        .Take(3)
                        .Select(i => new
                        {
                            i.InvoiceNumber,
                            i.Amount,
                            Status = i.Status.ToString(),
                            i.DueDate,
                            i.CreatedAt
                        })
                        .ToList()
                }
            };

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetClientFinancialSummary: {ex.Message}");
            return JsonSerializer.Serialize(new { error = "Failed to retrieve financial summary", details = ex.Message });
        }
    }
}

