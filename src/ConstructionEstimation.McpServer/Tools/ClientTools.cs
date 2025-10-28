using ConstructionEstimation.Core.Models;
using ConstructionEstimation.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ConstructionEstimation.McpServer.Tools;

public class ClientTools
{
    private readonly AppDbContext _context;

    public ClientTools(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> ListClients()
    {
        try
        {
            var clients = await _context.Clients
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Phone,
                    c.City,
                    c.State
                })
                .ToListAsync();

            return JsonSerializer.Serialize(clients, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in ListClients: {ex.Message}");
            return JsonSerializer.Serialize(new { error = "Failed to retrieve clients", details = ex.Message });
        }
    }

    public async Task<string> GetClientDetails(string clientIdStr)
    {
        try
        {
            if (!Guid.TryParse(clientIdStr, out var clientId))
            {
                return JsonSerializer.Serialize(new { error = "Invalid client ID format" });
            }

            var client = await _context.Clients
                .Where(c => c.Id == clientId)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Phone,
                    c.Address,
                    c.City,
                    c.State,
                    c.ZipCode,
                    c.CreatedAt,
                    c.UpdatedAt,
                    Estimates = c.Estimates.Select(e => new
                    {
                        e.Id,
                        e.EstimateNumber,
                        e.Title,
                        e.TotalAmount,
                        Status = e.Status.ToString(),
                        e.CreatedAt
                    }).ToList(),
                    Invoices = c.Invoices.Select(i => new
                    {
                        i.Id,
                        i.InvoiceNumber,
                        i.Description,
                        i.Amount,
                        Status = i.Status.ToString(),
                        i.DueDate,
                        i.PaidDate
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (client == null)
            {
                return JsonSerializer.Serialize(new { error = "Client not found" });
            }

            return JsonSerializer.Serialize(client, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetClientDetails: {ex.Message}");
            return JsonSerializer.Serialize(new { error = "Failed to retrieve client details", details = ex.Message });
        }
    }
}

