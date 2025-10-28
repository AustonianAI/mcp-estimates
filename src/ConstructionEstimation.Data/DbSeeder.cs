using ConstructionEstimation.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ConstructionEstimation.Data;

public static class DbSeeder
{
    public static void SeedData(AppDbContext context)
    {
        // Check if database already has data
        if (context.Clients.Any())
        {
            return; // Database has been seeded
        }

        // Create sample clients
        var clients = new List<Client>
        {
            new Client
            {
                Id = Guid.NewGuid(),
                Name = "Smith Residence",
                Email = "john.smith@email.com",
                Phone = "(555) 123-4567",
                Address = "123 Oak Street",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Client
            {
                Id = Guid.NewGuid(),
                Name = "Johnson Commercial Properties",
                Email = "contact@johnsonproperties.com",
                Phone = "(555) 234-5678",
                Address = "456 Business Park Dr",
                City = "Chicago",
                State = "IL",
                ZipCode = "60601",
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                UpdatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new Client
            {
                Id = Guid.NewGuid(),
                Name = "Martinez Family Home",
                Email = "maria.martinez@email.com",
                Phone = "(555) 345-6789",
                Address = "789 Maple Avenue",
                City = "Naperville",
                State = "IL",
                ZipCode = "60540",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Client
            {
                Id = Guid.NewGuid(),
                Name = "Riverside Restaurant Group",
                Email = "info@riversidegroup.com",
                Phone = "(555) 456-7890",
                Address = "321 River Road",
                City = "Peoria",
                State = "IL",
                ZipCode = "61602",
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                UpdatedAt = DateTime.UtcNow.AddDays(-60)
            }
        };

        context.Clients.AddRange(clients);
        context.SaveChanges();

        // Create sample estimates
        var estimates = new List<Estimate>
        {
            new Estimate
            {
                Id = Guid.NewGuid(),
                ClientId = clients[0].Id,
                EstimateNumber = "EST-2025-001",
                Title = "Kitchen Remodel",
                Description = "Complete kitchen renovation including new cabinets, countertops, appliances, and flooring. Includes electrical and plumbing updates.",
                TotalAmount = 45000.00m,
                Status = EstimateStatus.Approved,
                ValidUntil = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Estimate
            {
                Id = Guid.NewGuid(),
                ClientId = clients[0].Id,
                EstimateNumber = "EST-2025-002",
                Title = "Master Bathroom Addition",
                Description = "Add new master bathroom with walk-in shower, double vanity, and heated flooring.",
                TotalAmount = 32000.00m,
                Status = EstimateStatus.Sent,
                ValidUntil = DateTime.UtcNow.AddDays(45),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Estimate
            {
                Id = Guid.NewGuid(),
                ClientId = clients[1].Id,
                EstimateNumber = "EST-2025-003",
                Title = "Office Building Renovation",
                Description = "Complete renovation of 3,000 sq ft office space including new flooring, lighting, HVAC updates, and conference room build-out.",
                TotalAmount = 125000.00m,
                Status = EstimateStatus.Approved,
                ValidUntil = DateTime.UtcNow.AddDays(60),
                CreatedAt = DateTime.UtcNow.AddDays(-40),
                UpdatedAt = DateTime.UtcNow.AddDays(-35)
            },
            new Estimate
            {
                Id = Guid.NewGuid(),
                ClientId = clients[2].Id,
                EstimateNumber = "EST-2025-004",
                Title = "Deck Construction",
                Description = "Build new 400 sq ft composite deck with integrated lighting and railings.",
                TotalAmount = 18500.00m,
                Status = EstimateStatus.Draft,
                ValidUntil = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Estimate
            {
                Id = Guid.NewGuid(),
                ClientId = clients[3].Id,
                EstimateNumber = "EST-2025-005",
                Title = "Restaurant Kitchen Upgrade",
                Description = "Commercial kitchen upgrade including new exhaust system, stainless steel prep tables, and electrical upgrades.",
                TotalAmount = 85000.00m,
                Status = EstimateStatus.Sent,
                ValidUntil = DateTime.UtcNow.AddDays(20),
                CreatedAt = DateTime.UtcNow.AddDays(-55),
                UpdatedAt = DateTime.UtcNow.AddDays(-50)
            },
            new Estimate
            {
                Id = Guid.NewGuid(),
                ClientId = clients[2].Id,
                EstimateNumber = "EST-2025-006",
                Title = "Basement Finishing",
                Description = "Finish 800 sq ft basement with new drywall, flooring, lighting, and bathroom addition.",
                TotalAmount = 42000.00m,
                Status = EstimateStatus.Rejected,
                ValidUntil = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-12)
            }
        };

        context.Estimates.AddRange(estimates);
        context.SaveChanges();

        // Create sample invoices
        var invoices = new List<Invoice>
        {
            new Invoice
            {
                Id = Guid.NewGuid(),
                ClientId = clients[0].Id,
                EstimateId = estimates[0].Id,
                InvoiceNumber = "INV-2025-001",
                Description = "Kitchen Remodel - Initial Payment (50%)",
                Amount = 22500.00m,
                Status = InvoiceStatus.Paid,
                DueDate = DateTime.UtcNow.AddDays(-15),
                PaidDate = DateTime.UtcNow.AddDays(-18),
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-18)
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                ClientId = clients[0].Id,
                EstimateId = estimates[0].Id,
                InvoiceNumber = "INV-2025-002",
                Description = "Kitchen Remodel - Progress Payment (25%)",
                Amount = 11250.00m,
                Status = InvoiceStatus.Sent,
                DueDate = DateTime.UtcNow.AddDays(10),
                PaidDate = null,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                ClientId = clients[1].Id,
                EstimateId = estimates[2].Id,
                InvoiceNumber = "INV-2025-003",
                Description = "Office Building Renovation - Deposit",
                Amount = 37500.00m,
                Status = InvoiceStatus.Paid,
                DueDate = DateTime.UtcNow.AddDays(-30),
                PaidDate = DateTime.UtcNow.AddDays(-32),
                CreatedAt = DateTime.UtcNow.AddDays(-35),
                UpdatedAt = DateTime.UtcNow.AddDays(-32)
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                ClientId = clients[1].Id,
                EstimateId = estimates[2].Id,
                InvoiceNumber = "INV-2025-004",
                Description = "Office Building Renovation - Progress Payment #1",
                Amount = 43750.00m,
                Status = InvoiceStatus.Paid,
                DueDate = DateTime.UtcNow.AddDays(-10),
                PaidDate = DateTime.UtcNow.AddDays(-8),
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                ClientId = clients[1].Id,
                EstimateId = estimates[2].Id,
                InvoiceNumber = "INV-2025-005",
                Description = "Office Building Renovation - Final Payment",
                Amount = 43750.00m,
                Status = InvoiceStatus.Sent,
                DueDate = DateTime.UtcNow.AddDays(15),
                PaidDate = null,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                ClientId = clients[3].Id,
                EstimateId = null,
                InvoiceNumber = "INV-2025-006",
                Description = "Emergency Plumbing Repair - Kitchen Line",
                Amount = 1850.00m,
                Status = InvoiceStatus.Overdue,
                DueDate = DateTime.UtcNow.AddDays(-5),
                PaidDate = null,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                ClientId = clients[2].Id,
                EstimateId = null,
                InvoiceNumber = "INV-2025-007",
                Description = "Consultation and Site Assessment",
                Amount = 500.00m,
                Status = InvoiceStatus.Draft,
                DueDate = DateTime.UtcNow.AddDays(30),
                PaidDate = null,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        context.Invoices.AddRange(invoices);
        context.SaveChanges();

        Console.WriteLine("✅ Database seeded successfully with sample data!");
        Console.WriteLine($"   - {clients.Count} clients");
        Console.WriteLine($"   - {estimates.Count} estimates");
        Console.WriteLine($"   - {invoices.Count} invoices");
    }
}

