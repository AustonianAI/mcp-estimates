using ConstructionEstimation.Core.Models;
using ConstructionEstimation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConstructionEstimation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(AppDbContext context, ILogger<InvoicesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/invoices?clientId={clientId}&estimateId={estimateId}
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices(
        [FromQuery] Guid? clientId,
        [FromQuery] Guid? estimateId)
    {
        var query = _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Estimate)
            .AsQueryable();

        if (clientId.HasValue)
        {
            query = query.Where(i => i.ClientId == clientId.Value);
        }

        if (estimateId.HasValue)
        {
            query = query.Where(i => i.EstimateId == estimateId.Value);
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    // GET: api/invoices/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Invoice>> GetInvoice(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Estimate)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
        {
            return NotFound(new { message = $"Invoice with ID {id} not found." });
        }

        return invoice;
    }

    // POST: api/invoices
    [HttpPost]
    public async Task<ActionResult<Invoice>> CreateInvoice(Invoice invoice)
    {
        // Validate that the client exists
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == invoice.ClientId);
        if (!clientExists)
        {
            return BadRequest(new { message = $"Client with ID {invoice.ClientId} not found." });
        }

        // Validate that the estimate exists if provided
        if (invoice.EstimateId.HasValue)
        {
            var estimateExists = await _context.Estimates.AnyAsync(e => e.Id == invoice.EstimateId.Value);
            if (!estimateExists)
            {
                return BadRequest(new { message = $"Estimate with ID {invoice.EstimateId} not found." });
            }
        }

        invoice.Id = Guid.NewGuid();
        invoice.CreatedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    // PUT: api/invoices/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInvoice(Guid id, Invoice invoice)
    {
        if (id != invoice.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in request body." });
        }

        var existingInvoice = await _context.Invoices.FindAsync(id);
        if (existingInvoice == null)
        {
            return NotFound(new { message = $"Invoice with ID {id} not found." });
        }

        // Validate that the client exists if it's being changed
        if (existingInvoice.ClientId != invoice.ClientId)
        {
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == invoice.ClientId);
            if (!clientExists)
            {
                return BadRequest(new { message = $"Client with ID {invoice.ClientId} not found." });
            }
        }

        // Validate that the estimate exists if it's being changed or added
        if (invoice.EstimateId.HasValue && existingInvoice.EstimateId != invoice.EstimateId)
        {
            var estimateExists = await _context.Estimates.AnyAsync(e => e.Id == invoice.EstimateId.Value);
            if (!estimateExists)
            {
                return BadRequest(new { message = $"Estimate with ID {invoice.EstimateId} not found." });
            }
        }

        existingInvoice.EstimateId = invoice.EstimateId;
        existingInvoice.ClientId = invoice.ClientId;
        existingInvoice.InvoiceNumber = invoice.InvoiceNumber;
        existingInvoice.Description = invoice.Description;
        existingInvoice.Amount = invoice.Amount;
        existingInvoice.Status = invoice.Status;
        existingInvoice.DueDate = invoice.DueDate;
        existingInvoice.PaidDate = invoice.PaidDate;
        existingInvoice.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await InvoiceExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/invoices/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoice(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
        {
            return NotFound(new { message = $"Invoice with ID {id} not found." });
        }

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> InvoiceExists(Guid id)
    {
        return await _context.Invoices.AnyAsync(e => e.Id == id);
    }
}

