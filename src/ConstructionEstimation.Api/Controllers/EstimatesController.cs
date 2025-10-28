using ConstructionEstimation.Core.Models;
using ConstructionEstimation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConstructionEstimation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstimatesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EstimatesController> _logger;

    public EstimatesController(AppDbContext context, ILogger<EstimatesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/estimates?clientId={clientId}
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Estimate>>> GetEstimates([FromQuery] Guid? clientId)
    {
        var query = _context.Estimates
            .Include(e => e.Client)
            .AsQueryable();

        if (clientId.HasValue)
        {
            query = query.Where(e => e.ClientId == clientId.Value);
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    // GET: api/estimates/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Estimate>> GetEstimate(Guid id)
    {
        var estimate = await _context.Estimates
            .Include(e => e.Client)
            .Include(e => e.Invoices)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (estimate == null)
        {
            return NotFound(new { message = $"Estimate with ID {id} not found." });
        }

        return estimate;
    }

    // POST: api/estimates
    [HttpPost]
    public async Task<ActionResult<Estimate>> CreateEstimate(Estimate estimate)
    {
        // Validate that the client exists
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == estimate.ClientId);
        if (!clientExists)
        {
            return BadRequest(new { message = $"Client with ID {estimate.ClientId} not found." });
        }

        estimate.Id = Guid.NewGuid();
        estimate.CreatedAt = DateTime.UtcNow;
        estimate.UpdatedAt = DateTime.UtcNow;

        _context.Estimates.Add(estimate);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEstimate), new { id = estimate.Id }, estimate);
    }

    // PUT: api/estimates/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEstimate(Guid id, Estimate estimate)
    {
        if (id != estimate.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in request body." });
        }

        var existingEstimate = await _context.Estimates.FindAsync(id);
        if (existingEstimate == null)
        {
            return NotFound(new { message = $"Estimate with ID {id} not found." });
        }

        // Validate that the client exists if it's being changed
        if (existingEstimate.ClientId != estimate.ClientId)
        {
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == estimate.ClientId);
            if (!clientExists)
            {
                return BadRequest(new { message = $"Client with ID {estimate.ClientId} not found." });
            }
        }

        existingEstimate.ClientId = estimate.ClientId;
        existingEstimate.EstimateNumber = estimate.EstimateNumber;
        existingEstimate.Title = estimate.Title;
        existingEstimate.Description = estimate.Description;
        existingEstimate.TotalAmount = estimate.TotalAmount;
        existingEstimate.Status = estimate.Status;
        existingEstimate.ValidUntil = estimate.ValidUntil;
        existingEstimate.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await EstimateExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/estimates/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEstimate(Guid id)
    {
        var estimate = await _context.Estimates.FindAsync(id);
        if (estimate == null)
        {
            return NotFound(new { message = $"Estimate with ID {id} not found." });
        }

        _context.Estimates.Remove(estimate);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> EstimateExists(Guid id)
    {
        return await _context.Estimates.AnyAsync(e => e.Id == id);
    }
}

