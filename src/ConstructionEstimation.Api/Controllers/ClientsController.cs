using ConstructionEstimation.Core.Models;
using ConstructionEstimation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConstructionEstimation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(AppDbContext context, ILogger<ClientsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/clients
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Client>>> GetClients()
    {
        return await _context.Clients
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // GET: api/clients/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetClient(Guid id)
    {
        var client = await _context.Clients
            .Include(c => c.Estimates)
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound(new { message = $"Client with ID {id} not found." });
        }

        return client;
    }

    // POST: api/clients
    [HttpPost]
    public async Task<ActionResult<Client>> CreateClient(Client client)
    {
        client.Id = Guid.NewGuid();
        client.CreatedAt = DateTime.UtcNow;
        client.UpdatedAt = DateTime.UtcNow;

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
    }

    // PUT: api/clients/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient(Guid id, Client client)
    {
        if (id != client.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in request body." });
        }

        var existingClient = await _context.Clients.FindAsync(id);
        if (existingClient == null)
        {
            return NotFound(new { message = $"Client with ID {id} not found." });
        }

        existingClient.Name = client.Name;
        existingClient.Email = client.Email;
        existingClient.Phone = client.Phone;
        existingClient.Address = client.Address;
        existingClient.City = client.City;
        existingClient.State = client.State;
        existingClient.ZipCode = client.ZipCode;
        existingClient.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ClientExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/clients/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(Guid id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            return NotFound(new { message = $"Client with ID {id} not found." });
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> ClientExists(Guid id)
    {
        return await _context.Clients.AnyAsync(e => e.Id == id);
    }
}

