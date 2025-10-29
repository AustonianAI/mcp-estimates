# Construction Estimation API

A RESTful API backend for construction estimation management built with .NET Core 9.0 and Entity Framework Core. This demo application provides full CRUD operations for managing clients, estimates, and invoices.

## Features

- **Client Management** - Create and manage client information
- **Estimate Management** - Generate and track construction estimates with status tracking
- **Invoice Management** - Create invoices linked to estimates or standalone
- **MCP Server** - AI integration via Model Context Protocol for natural language interactions
- **SQLite Database** - Lightweight, file-based database for easy setup
- **Swagger UI** - Interactive API documentation and testing interface
- **RESTful Design** - Clean, intuitive API endpoints
- **Sample Data** - Automatically seeded demo data for testing

## Technology Stack

- **.NET Core 9.0** - Modern, cross-platform framework
- **Entity Framework Core 9.0** - ORM for database operations
- **SQLite** - Embedded database
- **Swagger/OpenAPI** - API documentation
- **Clean Architecture** - Separation of concerns with Core, Data, and API layers

## Project Structure

```
ConstructionEstimation/
├── src/
│   ├── ConstructionEstimation.Api/          # Web API project
│   │   ├── Controllers/                     # API controllers
│   │   ├── Program.cs                       # Application entry point
│   │   └── appsettings.json                 # Configuration
│   ├── ConstructionEstimation.Core/         # Domain models
│   │   └── Models/                          # Client, Estimate, Invoice
│   ├── ConstructionEstimation.Data/         # Data access layer
│   │   ├── AppDbContext.cs                  # EF Core context
│   │   └── DbSeeder.cs                      # Sample data seeder
│   └── ConstructionEstimation.McpServer/    # MCP server for AI integration
│       ├── Tools/                           # MCP tool implementations
│       └── Program.cs                       # MCP server entry point
└── ConstructionEstimation.sln               # Solution file
```

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later

### Installation

1. Clone the repository:

```bash
git clone <repository-url>
cd mcp-estimates
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Run the application:

**For HTTPS (recommended):**

```bash
dotnet run --project src/ConstructionEstimation.Api --launch-profile https
```

The API will start on `https://localhost:7052` (HTTPS) and `http://localhost:5125` (HTTP).

**For HTTP only:**

```bash
dotnet run --project src/ConstructionEstimation.Api --launch-profile http
```

The API will start on `http://localhost:5125` (HTTP only).

4. Open your browser and navigate to:

```
https://localhost:7052
```

You'll see the Swagger UI with interactive API documentation.

## API Endpoints

### Clients

| Method | Endpoint            | Description       |
| ------ | ------------------- | ----------------- |
| GET    | `/api/clients`      | Get all clients   |
| GET    | `/api/clients/{id}` | Get client by ID  |
| POST   | `/api/clients`      | Create new client |
| PUT    | `/api/clients/{id}` | Update client     |
| DELETE | `/api/clients/{id}` | Delete client     |

### Estimates

| Method | Endpoint                       | Description             |
| ------ | ------------------------------ | ----------------------- |
| GET    | `/api/estimates`               | Get all estimates       |
| GET    | `/api/estimates?clientId={id}` | Get estimates by client |
| GET    | `/api/estimates/{id}`          | Get estimate by ID      |
| POST   | `/api/estimates`               | Create new estimate     |
| PUT    | `/api/estimates/{id}`          | Update estimate         |
| DELETE | `/api/estimates/{id}`          | Delete estimate         |

### Invoices

| Method | Endpoint                        | Description              |
| ------ | ------------------------------- | ------------------------ |
| GET    | `/api/invoices`                 | Get all invoices         |
| GET    | `/api/invoices?clientId={id}`   | Get invoices by client   |
| GET    | `/api/invoices?estimateId={id}` | Get invoices by estimate |
| GET    | `/api/invoices/{id}`            | Get invoice by ID        |
| POST   | `/api/invoices`                 | Create new invoice       |
| PUT    | `/api/invoices/{id}`            | Update invoice           |
| DELETE | `/api/invoices/{id}`            | Delete invoice           |

## Data Models

### Client

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "ABC Construction",
  "email": "contact@abc.com",
  "phone": "555-1234",
  "address": "123 Main St",
  "city": "Springfield",
  "state": "IL",
  "zipCode": "62701",
  "createdAt": "2025-10-28T10:00:00Z",
  "updatedAt": "2025-10-28T10:00:00Z"
}
```

### Estimate

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "estimateNumber": "EST-2025-001",
  "title": "Kitchen Remodel",
  "description": "Complete kitchen renovation including cabinets, countertops, and appliances",
  "totalAmount": 25000.0,
  "status": "Draft",
  "validUntil": "2025-12-31T00:00:00Z",
  "createdAt": "2025-10-28T10:00:00Z",
  "updatedAt": "2025-10-28T10:00:00Z"
}
```

**Estimate Status Values:** `Draft`, `Sent`, `Approved`, `Rejected`

### Invoice

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "estimateId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "invoiceNumber": "INV-2025-001",
  "description": "First payment for kitchen remodel",
  "amount": 12500.0,
  "status": "Sent",
  "dueDate": "2025-11-15T00:00:00Z",
  "paidDate": null,
  "createdAt": "2025-10-28T10:00:00Z",
  "updatedAt": "2025-10-28T10:00:00Z"
}
```

**Invoice Status Values:** `Draft`, `Sent`, `Paid`, `Overdue`

## Usage Examples

### Create a Client

```bash
curl -X POST https://localhost:7052/api/clients \
  -H "Content-Type: application/json" \
  -d '{
    "name": "ABC Construction",
    "email": "contact@abc.com",
    "phone": "555-1234",
    "address": "123 Main St",
    "city": "Springfield",
    "state": "IL",
    "zipCode": "62701"
  }'
```

### Create an Estimate

```bash
curl -X POST https://localhost:7052/api/estimates \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "{client-id}",
    "estimateNumber": "EST-2025-001",
    "title": "Kitchen Remodel",
    "description": "Complete kitchen renovation",
    "totalAmount": 25000.00,
    "status": "Draft",
    "validUntil": "2025-12-31T00:00:00Z"
  }'
```

### Create an Invoice

```bash
curl -X POST https://localhost:7052/api/invoices \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "{client-id}",
    "estimateId": "{estimate-id}",
    "invoiceNumber": "INV-2025-001",
    "description": "First payment",
    "amount": 12500.00,
    "status": "Sent",
    "dueDate": "2025-11-15T00:00:00Z"
  }'
```

## Development

### Building the Solution

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Database

The application uses SQLite with Entity Framework Core. The database file (`construction_estimation.db`) is automatically created on first run and is stored in the API project directory.

To reset the database, simply delete the `.db` file and restart the application.

## Configuration

Configuration is managed through `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=construction_estimation.db"
  }
}
```

## License

This is a demo project created for demonstration purposes.

## MCP Server (AI Integration)

This project includes a **Model Context Protocol (MCP) server** that enables AI assistants like Claude to interact with your construction estimation data.

### What is MCP?

Model Context Protocol (MCP) is a standard protocol that allows AI assistants to securely connect to your data and tools. With the MCP server, you can use natural language to:

- Query client information
- Create and manage estimates
- View financial summaries
- Get statistics about your projects

### MCP Server Setup

#### Prerequisites

- [Claude Desktop](https://claude.ai/download) installed
- .NET 9.0 SDK

#### Configure Claude Desktop

1. Open your Claude Desktop configuration file:

**macOS:**

```bash
code ~/Library/Application\ Support/Claude/claude_desktop_config.json
```

**Windows:**

```bash
code %APPDATA%\Claude\claude_desktop_config.json
```

2. Add the construction estimator MCP server:

```json
{
  "mcpServers": {
    "construction-estimator": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/ABSOLUTE/PATH/TO/mcp-estimates/src/ConstructionEstimation.McpServer"
      ]
    }
  }
}
```

**Important:** Replace `/ABSOLUTE/PATH/TO/mcp-estimates` with the actual absolute path to your project directory.

3. Restart Claude Desktop completely (Quit and reopen, don't just close the window)

4. Look for the 🔨 (hammer) icon in Claude Desktop to see available tools

### Available MCP Tools

The MCP server exposes 10 tools for AI interaction:

#### Client Management

- **list_clients** - Get all clients with basic information
- **get_client_details** - Get detailed client info including estimates and invoices

#### Estimate Management

- **list_estimates** - Get all estimates with optional client filter
- **get_estimate_details** - Get detailed estimate information
- **create_estimate** - Create a new estimate for a client
- **update_estimate** - Update an existing estimate (title, description, amount, status, validUntil)
- **get_estimate_statistics** - Get statistics about estimates

#### Invoice & Financial

- **list_invoices** - Get all invoices with optional filters
- **get_client_financial_summary** - Get comprehensive financial summary

#### Schema & Development

- **get_database_schema** - Get complete database schema in OpenAPI/JSON Schema format for front-end development

### Example AI Queries

Once configured, you can ask Claude questions like:

- "Show me all clients in the system"
- "What's the financial summary for Smith Residence?"
- "Create an estimate for Martinez Family Home for a deck renovation worth $15,000"
- "Update estimate EST-2025-001 to change the status to Approved and increase the amount to $28,000"
- "What are the statistics on all my estimates?"
- "Show me all unpaid invoices"
- "Get the database schema so I can build a front-end UI"

### Running the MCP Server Standalone

You can also test the MCP server directly:

```bash
cd src/ConstructionEstimation.McpServer
dotnet run
```

The server listens on stdin for JSON-RPC messages and responds on stdout.

### Troubleshooting MCP Server

**Server not appearing in Claude Desktop:**

1. Verify the path in `claude_desktop_config.json` is absolute and correct
2. Check Claude Desktop logs:
   ```bash
   tail -f ~/Library/Logs/Claude/mcp*.log
   ```
3. Make sure you fully quit and restart Claude Desktop

**Tool calls failing:**

- Check the MCP server logs in Claude's log directory
- Verify the database file exists and is accessible
- Ensure .NET 9.0 is installed: `dotnet --version`

## Contributing

This is a demo project. Feel free to fork and modify for your own use.
