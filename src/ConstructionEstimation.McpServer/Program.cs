using ConstructionEstimation.Data;
using ConstructionEstimation.McpServer.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;

// CRITICAL: All logging must go to stderr, not stdout
// MCP uses stdout for JSON-RPC communication
void Log(string message)
{
    Console.Error.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
}

try
{
    Log("Starting Construction Estimation MCP Server...");

    // Setup DI container
    var services = new ServiceCollection();
    
    // Configure database with absolute path
    var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var dbPath = Path.Combine(projectRoot, "construction_estimation.db");
    var connectionString = $"Data Source={dbPath}";
    
    Log($"Database path: {dbPath}");
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));

    // Register HttpClient for API calls
    services.AddHttpClient();
    
    // Register tool classes
    services.AddScoped<ClientTools>();
    services.AddScoped<EstimateTools>();
    services.AddScoped<InvoiceTools>();
    services.AddScoped<SchemaTools>();
    services.AddScoped<ApiTools>();

    var serviceProvider = services.BuildServiceProvider();

    // Ensure database is created and seed with sample data
    using (var scope = serviceProvider.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
        DbSeeder.SeedData(dbContext);
        Log("Database initialized and seeded");
    }

    Log("MCP Server ready, listening for requests on stdin...");

    // Main loop: read JSON-RPC messages from stdin, write responses to stdout
    string? line;
    while ((line = Console.In.ReadLine()) != null)
    {
        JsonNode? id = null;
        try
        {
            var request = JsonNode.Parse(line);
            if (request == null) continue;

            var method = request["method"]?.GetValue<string>();
            id = request["id"];
            var paramsNode = request["params"];

            Log($"Received request: {method}");

            JsonNode? response = null;

            switch (method)
            {
                case "initialize":
                    response = HandleInitialize(id);
                    break;

                case "tools/list":
                    response = HandleToolsList(id);
                    break;

                case "tools/call":
                    response = await HandleToolCall(id, paramsNode, serviceProvider);
                    break;

                case "prompts/list":
                    response = new JsonObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = id?.DeepClone(),
                        ["result"] = new JsonObject
                        {
                            ["prompts"] = new JsonArray()
                        }
                    };
                    break;

                case "resources/list":
                    response = new JsonObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = id?.DeepClone(),
                        ["result"] = new JsonObject
                        {
                            ["resources"] = new JsonArray()
                        }
                    };
                    break;

                case "ping":
                    response = new JsonObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = id?.DeepClone(),
                        ["result"] = new JsonObject()
                    };
                    break;

                case "notifications/initialized":
                case "notifications/cancelled":
                    // Notifications don't require a response
                    break;

                default:
                    // Only send error response if there's an id (i.e., it's a request, not a notification)
                    if (id != null)
                    {
                        response = new JsonObject
                        {
                            ["jsonrpc"] = "2.0",
                            ["id"] = id.DeepClone(),
                            ["error"] = new JsonObject
                            {
                                ["code"] = -32601,
                                ["message"] = $"Method not found: {method}"
                            }
                        };
                    }
                    break;
            }

            if (response != null)
            {
                // Write response to stdout (this is the only stdout output allowed)
                Console.WriteLine(response.ToJsonString());
            }
        }
        catch (Exception ex)
        {
            Log($"Error processing request: {ex.Message}");
            
            // Send error response - use the request id if we have it, otherwise null
            var errorResponse = new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = id?.DeepClone(),
                ["error"] = new JsonObject
                {
                    ["code"] = -32603,
                    ["message"] = "Internal error",
                    ["data"] = ex.Message
                }
            };
            Console.WriteLine(errorResponse.ToJsonString());
        }
    }

    Log("MCP Server shutting down");
}
catch (Exception ex)
{
    Log($"Fatal error: {ex.Message}");
    Log(ex.StackTrace ?? "");
    Environment.Exit(1);
}

JsonNode HandleInitialize(JsonNode? id)
{
    return new JsonObject
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id?.DeepClone(),
        ["result"] = new JsonObject
        {
            ["protocolVersion"] = "2024-11-05",
            ["capabilities"] = new JsonObject
            {
                ["tools"] = new JsonObject()
            },
            ["serverInfo"] = new JsonObject
            {
                ["name"] = "construction-estimator",
                ["version"] = "1.0.0"
            }
        }
    };
}

JsonNode HandleToolsList(JsonNode? id)
{
    var tools = new JsonArray
    {
        new JsonObject
        {
            ["name"] = "list_clients",
            ["description"] = "Get all clients with basic information",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject()
            }
        },
        new JsonObject
        {
            ["name"] = "get_client_details",
            ["description"] = "Get detailed client information including estimates and invoices",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["client_id"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "The GUID of the client"
                    }
                },
                ["required"] = new JsonArray { "client_id" }
            }
        },
        new JsonObject
        {
            ["name"] = "list_estimates",
            ["description"] = "Get all estimates with optional client filter",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["client_id"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Optional: Filter by client GUID"
                    }
                }
            }
        },
        new JsonObject
        {
            ["name"] = "get_estimate_details",
            ["description"] = "Get detailed information about a specific estimate",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["estimate_id"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "The GUID of the estimate"
                    }
                },
                ["required"] = new JsonArray { "estimate_id" }
            }
        },
        new JsonObject
        {
            ["name"] = "create_estimate",
            ["description"] = "Create a new estimate for a client",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["client_id"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "The GUID of the client"
                    },
                    ["title"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Title of the estimate"
                    },
                    ["description"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Detailed description of the work"
                    },
                    ["total_amount"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Total amount in decimal format"
                    },
                    ["status"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Status: Draft, Sent, Approved, or Rejected"
                    }
                },
                ["required"] = new JsonArray { "client_id", "title", "description", "total_amount" }
            }
        },
        new JsonObject
        {
            ["name"] = "get_estimate_statistics",
            ["description"] = "Get statistics about estimates (counts, averages, status breakdown)",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject()
            }
        },
        new JsonObject
        {
            ["name"] = "list_invoices",
            ["description"] = "Get all invoices with optional filters",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["client_id"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Optional: Filter by client GUID"
                    },
                    ["estimate_id"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Optional: Filter by estimate GUID"
                    }
                }
            }
        },
        new JsonObject
        {
            ["name"] = "get_client_financial_summary",
            ["description"] = "Get comprehensive financial summary for a client",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["client_id"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "The GUID of the client"
                    }
                },
                ["required"] = new JsonArray { "client_id" }
            }
        },
        new JsonObject
        {
            ["name"] = "update_estimate",
            ["description"] = "Update an existing estimate (title, description, totalAmount, status, validUntil)",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["estimate_id"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "The GUID of the estimate to update"
                    },
                    ["title"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Optional: New title for the estimate"
                    },
                    ["description"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Optional: New description"
                    },
                    ["total_amount"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Optional: New total amount in decimal format"
                    },
                    ["status"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Optional: New status (Draft, Sent, Approved, or Rejected)"
                    },
                    ["valid_until"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Optional: New valid until date (ISO 8601 format)"
                    }
                },
                ["required"] = new JsonArray { "estimate_id" }
            }
        },
        new JsonObject
        {
            ["name"] = "get_database_schema",
            ["description"] = "Get the complete database schema in OpenAPI/JSON Schema format for front-end development",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject()
            }
        },
        new JsonObject
        {
            ["name"] = "get_api_routes",
            ["description"] = "Get the complete REST API routes from the live Swagger endpoint with full request/response models",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject()
            }
        }
    };

    return new JsonObject
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id?.DeepClone(),
        ["result"] = new JsonObject
        {
            ["tools"] = tools
        }
    };
}

async Task<JsonNode> HandleToolCall(JsonNode? id, JsonNode? paramsNode, IServiceProvider serviceProvider)
{
    var toolName = paramsNode?["name"]?.GetValue<string>();
    var arguments = paramsNode?["arguments"] as JsonObject;

    Log($"Calling tool: {toolName}");

    string result;

    using (var scope = serviceProvider.CreateScope())
    {
        switch (toolName)
        {
            case "list_clients":
                var clientTools = scope.ServiceProvider.GetRequiredService<ClientTools>();
                result = await clientTools.ListClients();
                break;

            case "get_client_details":
                clientTools = scope.ServiceProvider.GetRequiredService<ClientTools>();
                var clientId = arguments?["client_id"]?.GetValue<string>() ?? "";
                result = await clientTools.GetClientDetails(clientId);
                break;

            case "list_estimates":
                var estimateTools = scope.ServiceProvider.GetRequiredService<EstimateTools>();
                var filterClientId = arguments?["client_id"]?.GetValue<string>();
                result = await estimateTools.ListEstimates(filterClientId);
                break;

            case "get_estimate_details":
                estimateTools = scope.ServiceProvider.GetRequiredService<EstimateTools>();
                var estimateId = arguments?["estimate_id"]?.GetValue<string>() ?? "";
                result = await estimateTools.GetEstimateDetails(estimateId);
                break;

            case "create_estimate":
                estimateTools = scope.ServiceProvider.GetRequiredService<EstimateTools>();
                var createClientId = arguments?["client_id"]?.GetValue<string>() ?? "";
                var title = arguments?["title"]?.GetValue<string>() ?? "";
                var description = arguments?["description"]?.GetValue<string>() ?? "";
                var totalAmount = arguments?["total_amount"]?.GetValue<string>() ?? "0";
                var status = arguments?["status"]?.GetValue<string>() ?? "Draft";
                result = await estimateTools.CreateEstimate(createClientId, title, description, totalAmount, status);
                break;

            case "get_estimate_statistics":
                estimateTools = scope.ServiceProvider.GetRequiredService<EstimateTools>();
                result = await estimateTools.GetEstimateStatistics();
                break;

            case "list_invoices":
                var invoiceTools = scope.ServiceProvider.GetRequiredService<InvoiceTools>();
                var invoiceClientId = arguments?["client_id"]?.GetValue<string>();
                var invoiceEstimateId = arguments?["estimate_id"]?.GetValue<string>();
                result = await invoiceTools.ListInvoices(invoiceClientId, invoiceEstimateId);
                break;

            case "get_client_financial_summary":
                invoiceTools = scope.ServiceProvider.GetRequiredService<InvoiceTools>();
                var summaryClientId = arguments?["client_id"]?.GetValue<string>() ?? "";
                result = await invoiceTools.GetClientFinancialSummary(summaryClientId);
                break;

            case "update_estimate":
                estimateTools = scope.ServiceProvider.GetRequiredService<EstimateTools>();
                var updateEstimateId = arguments?["estimate_id"]?.GetValue<string>() ?? "";
                var updateTitle = arguments?["title"]?.GetValue<string>();
                var updateDescription = arguments?["description"]?.GetValue<string>();
                var updateTotalAmount = arguments?["total_amount"]?.GetValue<string>();
                var updateStatus = arguments?["status"]?.GetValue<string>();
                var updateValidUntil = arguments?["valid_until"]?.GetValue<string>();
                result = await estimateTools.UpdateEstimate(updateEstimateId, updateTitle, updateDescription, updateTotalAmount, updateStatus, updateValidUntil);
                break;

            case "get_database_schema":
                var schemaTools = scope.ServiceProvider.GetRequiredService<SchemaTools>();
                result = await schemaTools.GetDatabaseSchema();
                break;

            case "get_api_routes":
                var apiTools = scope.ServiceProvider.GetRequiredService<ApiTools>();
                result = await apiTools.GetApiRoutes();
                break;

            default:
                return new JsonObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = id?.DeepClone(),
                    ["error"] = new JsonObject
                    {
                        ["code"] = -32602,
                        ["message"] = $"Unknown tool: {toolName}"
                    }
                };
        }
    }

    return new JsonObject
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id?.DeepClone(),
        ["result"] = new JsonObject
        {
            ["content"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = result
                }
            }
        }
    };
}
