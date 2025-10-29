using System.Text.Json;

namespace ConstructionEstimation.McpServer.Tools;

public class SchemaTools
{
    public Task<string> GetDatabaseSchema()
    {
        try
        {
            var schema = new
            {
                openapi = "3.0.0",
                info = new
                {
                    title = "Construction Estimation API Schema",
                    description = "Complete database schema for the Construction Estimation system",
                    version = "1.0.0"
                },
                components = new
                {
                    schemas = new
                    {
                        Client = new
                        {
                            type = "object",
                            description = "A client for construction projects",
                            required = new[] { "name", "email" },
                            properties = new
                            {
                                id = new
                                {
                                    type = "string",
                                    format = "uuid",
                                    description = "Unique identifier for the client",
                                    readOnly = true
                                },
                                name = new
                                {
                                    type = "string",
                                    description = "Client name",
                                    example = "ABC Construction"
                                },
                                email = new
                                {
                                    type = "string",
                                    format = "email",
                                    description = "Client email address",
                                    example = "contact@abc.com"
                                },
                                phone = new
                                {
                                    type = "string",
                                    description = "Client phone number",
                                    example = "555-1234"
                                },
                                address = new
                                {
                                    type = "string",
                                    description = "Street address",
                                    example = "123 Main St"
                                },
                                city = new
                                {
                                    type = "string",
                                    description = "City",
                                    example = "Springfield"
                                },
                                state = new
                                {
                                    type = "string",
                                    description = "State or province",
                                    example = "IL"
                                },
                                zipCode = new
                                {
                                    type = "string",
                                    description = "ZIP or postal code",
                                    example = "62701"
                                },
                                createdAt = new
                                {
                                    type = "string",
                                    format = "date-time",
                                    description = "When the client was created",
                                    readOnly = true
                                },
                                updatedAt = new
                                {
                                    type = "string",
                                    format = "date-time",
                                    description = "When the client was last updated",
                                    readOnly = true
                                }
                            },
                            relationships = new
                            {
                                estimates = new
                                {
                                    type = "one-to-many",
                                    entity = "Estimate",
                                    description = "All estimates associated with this client"
                                },
                                invoices = new
                                {
                                    type = "one-to-many",
                                    entity = "Invoice",
                                    description = "All invoices associated with this client"
                                }
                            }
                        },
                        Estimate = new
                        {
                            type = "object",
                            description = "A construction project estimate",
                            required = new[] { "clientId", "title", "totalAmount" },
                            properties = new
                            {
                                id = new
                                {
                                    type = "string",
                                    format = "uuid",
                                    description = "Unique identifier for the estimate",
                                    readOnly = true
                                },
                                clientId = new
                                {
                                    type = "string",
                                    format = "uuid",
                                    description = "ID of the client this estimate is for"
                                },
                                estimateNumber = new
                                {
                                    type = "string",
                                    description = "Auto-generated estimate number (format: EST-YYYY-###)",
                                    example = "EST-2025-001",
                                    readOnly = true
                                },
                                title = new
                                {
                                    type = "string",
                                    description = "Short title for the estimate",
                                    example = "Kitchen Remodel"
                                },
                                description = new
                                {
                                    type = "string",
                                    description = "Detailed description of the work",
                                    example = "Complete kitchen renovation including cabinets, countertops, and appliances"
                                },
                                totalAmount = new
                                {
                                    type = "number",
                                    format = "decimal",
                                    description = "Total estimated cost",
                                    example = 25000.00,
                                    minimum = 0
                                },
                                status = new
                                {
                                    type = "string",
                                    description = "Current status of the estimate",
                                    @enum = new[] { "Draft", "Sent", "Approved", "Rejected" },
                                    example = "Draft"
                                },
                                validUntil = new
                                {
                                    type = "string",
                                    format = "date-time",
                                    description = "Date until which the estimate is valid",
                                    nullable = true
                                },
                                createdAt = new
                                {
                                    type = "string",
                                    format = "date-time",
                                    description = "When the estimate was created",
                                    readOnly = true
                                },
                                updatedAt = new
                                {
                                    type = "string",
                                    format = "date-time",
                                    description = "When the estimate was last updated",
                                    readOnly = true
                                }
                            },
                            relationships = new
                            {
                                client = new
                                {
                                    type = "many-to-one",
                                    entity = "Client",
                                    description = "The client this estimate belongs to"
                                },
                                invoices = new
                                {
                                    type = "one-to-many",
                                    entity = "Invoice",
                                    description = "Invoices generated from this estimate"
                                }
                            }
                        },
                        Invoice = new
                        {
                            type = "object",
                            description = "An invoice for construction work",
                            required = new[] { "clientId", "amount" },
                            properties = new
                            {
                                id = new
                                {
                                    type = "string",
                                    format = "uuid",
                                    description = "Unique identifier for the invoice",
                                    readOnly = true
                                },
                                estimateId = new
                                {
                                    type = "string",
                                    format = "uuid",
                                    description = "ID of the estimate this invoice is based on (optional)",
                                    nullable = true
                                },
                                clientId = new
                                {
                                    type = "string",
                                    format = "uuid",
                                    description = "ID of the client this invoice is for"
                                },
                                invoiceNumber = new
                                {
                                    type = "string",
                                    description = "Auto-generated invoice number (format: INV-YYYY-###)",
                                    example = "INV-2025-001",
                                    readOnly = true
                                },
                                description = new
                                {
                                    type = "string",
                                    description = "Description of the invoiced work",
                                    example = "First payment for kitchen remodel"
                                },
                                amount = new
                                {
                                    type = "number",
                                    format = "decimal",
                                    description = "Invoice amount",
                                    example = 12500.00,
                                    minimum = 0
                                },
                                status = new
                                {
                                    type = "string",
                                    description = "Current status of the invoice",
                                    @enum = new[] { "Draft", "Sent", "Paid", "Overdue" },
                                    example = "Sent"
                                },
                                dueDate = new
                                {
                                    type = "string",
                                    format = "date-time",
                                    description = "Date when payment is due",
                                    nullable = true
                                },
                                paidDate = new
                                {
                                    type = "string",
                                    format = "date-time",
                                    description = "Date when the invoice was paid",
                                    nullable = true
                                },
                                createdAt = new
                                {
                                    type = "string",
                                    format = "date-time",
                                    description = "When the invoice was created",
                                    readOnly = true
                                },
                                updatedAt = new
                                {
                                    type = "string",
                                    format = "date-time",
                                    description = "When the invoice was last updated",
                                    readOnly = true
                                }
                            },
                            relationships = new
                            {
                                client = new
                                {
                                    type = "many-to-one",
                                    entity = "Client",
                                    description = "The client this invoice belongs to"
                                },
                                estimate = new
                                {
                                    type = "many-to-one",
                                    entity = "Estimate",
                                    description = "The estimate this invoice is based on (if any)",
                                    nullable = true
                                }
                            }
                        },
                        EstimateStatus = new
                        {
                            type = "string",
                            description = "Status of an estimate",
                            @enum = new[] { "Draft", "Sent", "Approved", "Rejected" },
                            enumDescriptions = new
                            {
                                Draft = "Estimate is being prepared",
                                Sent = "Estimate has been sent to client",
                                Approved = "Client has approved the estimate",
                                Rejected = "Client has rejected the estimate"
                            }
                        },
                        InvoiceStatus = new
                        {
                            type = "string",
                            description = "Status of an invoice",
                            @enum = new[] { "Draft", "Sent", "Paid", "Overdue" },
                            enumDescriptions = new
                            {
                                Draft = "Invoice is being prepared",
                                Sent = "Invoice has been sent to client",
                                Paid = "Invoice has been paid",
                                Overdue = "Invoice payment is overdue"
                            }
                        }
                    }
                }
            };

            return Task.FromResult(JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetDatabaseSchema: {ex.Message}");
            return Task.FromResult(JsonSerializer.Serialize(new { error = "Failed to retrieve database schema", details = ex.Message }));
        }
    }
}

