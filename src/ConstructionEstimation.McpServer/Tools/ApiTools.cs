using System.Text.Json;
using System.Text.Json.Nodes;

namespace ConstructionEstimation.McpServer.Tools;

public class ApiTools
{
    private readonly HttpClient _httpClient;

    public ApiTools(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetApiRoutes()
    {
        try
        {
            // Fetch the OpenAPI spec from the running API
            var swaggerUrl = "http://localhost:5125/swagger/v1/swagger.json";
            
            Console.Error.WriteLine($"Fetching API routes from {swaggerUrl}...");
            
            var response = await _httpClient.GetAsync(swaggerUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = "Failed to fetch API routes",
                    details = $"API returned status code {response.StatusCode}. Make sure the API is running at http://localhost:5125",
                    hint = "Start the API with: dotnet run --project src/ConstructionEstimation.Api --launch-profile http"
                });
            }

            var swaggerJson = await response.Content.ReadAsStringAsync();
            var swaggerDoc = JsonNode.Parse(swaggerJson);

            if (swaggerDoc == null)
            {
                return JsonSerializer.Serialize(new { error = "Failed to parse Swagger JSON" });
            }

            // Add or replace servers field with {baseUrl} placeholder
            var serversArray = new JsonArray
            {
                new JsonObject
                {
                    ["url"] = "{baseUrl}",
                    ["description"] = "API base URL (e.g., http://localhost:5125 for development)"
                }
            };
            swaggerDoc["servers"] = serversArray;

            // Add helpful metadata
            var enhancedDoc = new JsonObject
            {
                ["swagger_spec"] = swaggerDoc,
                ["metadata"] = new JsonObject
                {
                    ["development_urls"] = new JsonObject
                    {
                        ["http"] = "http://localhost:5125",
                        ["https"] = "https://localhost:7052"
                    },
                    ["note"] = "Replace {baseUrl} in the swagger_spec with your actual API base URL",
                    ["fetched_at"] = DateTime.UtcNow.ToString("o")
                }
            };

            return JsonSerializer.Serialize(enhancedDoc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error fetching API routes: {ex.Message}");
            return JsonSerializer.Serialize(new 
            { 
                error = "Could not connect to API",
                details = ex.Message,
                hint = "Make sure the Construction Estimation API is running. Start it with: dotnet run --project src/ConstructionEstimation.Api --launch-profile http"
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetApiRoutes: {ex.Message}");
            return JsonSerializer.Serialize(new 
            { 
                error = "Failed to retrieve API routes", 
                details = ex.Message 
            });
        }
    }
}

