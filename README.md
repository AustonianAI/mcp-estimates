# Construction Estimation API

A RESTful API backend for construction estimation management built with .NET Core 9.0 and Entity Framework Core. This demo application provides full CRUD operations for managing clients, estimates, and invoices.

## Features

- **Client Management** - Create and manage client information
- **Estimate Management** - Generate and track construction estimates with status tracking
- **Invoice Management** - Create invoices linked to estimates or standalone
- **SQLite Database** - Lightweight, file-based database for easy setup
- **Swagger UI** - Interactive API documentation and testing interface
- **RESTful Design** - Clean, intuitive API endpoints

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
│   └── ConstructionEstimation.Data/         # Data access layer
│       └── AppDbContext.cs                  # EF Core context
└── ConstructionEstimation.sln               # Solution file
```

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later

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

```bash
dotnet run --project src/ConstructionEstimation.Api
```

The API will start on `https://localhost:7052` (HTTPS) and `http://localhost:5125` (HTTP).

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

## Contributing

This is a demo project. Feel free to fork and modify for your own use.
