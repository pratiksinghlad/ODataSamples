# 🚀 .NET 9 Minimal API Architecture Guide

This document explains the modern .NET 9 architecture patterns and best practices implemented in this OData sample application.

## 🏗️ Project Structure

```
ODataSamples/
├── Controllers/                 # OData controllers for advanced querying
├── Endpoints/                   # Modern Minimal API endpoints
│   ├── ProductEndpoints.cs     # Product CRUD operations
│   ├── CustomerEndpoints.cs    # Customer management
│   └── OrderEndpoints.cs       # Order operations
├── Extensions/                  # Service and middleware extensions
│   ├── ServiceCollectionExtensions.cs
│   └── WebApplicationExtensions.cs
├── Data/                        # Data access layer
├── Services/                    # Business logic services
└── Program.cs                   # Application entry point
```

## 🎯 Key .NET 9 Features Implemented

### 1. **User Secrets for Development**
```csharp
// Program.cs - Secure configuration management
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
```

**Benefits:**
- ✅ Secrets never committed to source control
- ✅ Automatic integration in development
- ✅ Follows Microsoft security best practices

### 2. **Extension Methods for Clean DI**
```csharp
// Extensions/ServiceCollectionExtensions.cs
builder.Services.AddDatabaseServices(builder.Configuration, builder.Environment);
builder.Services.AddRepositoryServices();
builder.Services.AddBusinessServices();
builder.Services.AddODataServices();
```

**Benefits:**
- ✅ Separation of concerns
- ✅ Testable configuration
- ✅ Follows Microsoft conventions

### 3. **Modern Minimal API with TypedResults**
```csharp
// Endpoints/ProductEndpoints.cs
private static async Task<Results<Ok<ProductModel>, NotFound, ProblemHttpResult>> GetProductByIdAsync(
    [Range(1, int.MaxValue)] int id,
    IUnitOfWork unitOfWork,
    ILogger<Program> logger)
{
    // Implementation with proper type safety
}
```

**Benefits:**
- ✅ Type-safe responses
- ✅ Automatic OpenAPI generation
- ✅ Built-in validation attributes
- ✅ Structured logging

### 4. **Global Exception Handling**
```csharp
// Extensions/WebApplicationExtensions.cs
public sealed class GlobalExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

**Benefits:**
- ✅ Consistent error responses
- ✅ Production-ready error handling
- ✅ Structured error logging

### 5. **Environment-Specific Configuration**
```csharp
// Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddCorsConfiguration(
    this IServiceCollection services,
    IWebHostEnvironment environment)
{
    if (environment.IsDevelopment())
    {
        options.AddPolicy("Development", policy => policy.AllowAnyOrigin());
    }
    else
    {
        options.AddPolicy("Production", policy => policy
            .WithOrigins("https://yourdomain.com")
            .AllowCredentials());
    }
}
```

## 🔧 Configuration Management

### **Development Environment**
```json
// User Secrets (right-click project → Manage User Secrets)
{
  "ConnectionStrings": {
    "DBConnection": "Server=localhost;Database=ODataDemo;Uid=root;Pwd=yourpassword;"
  }
}
```

### **Production Environment**
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DBConnection": "Server=prodserver;Database=ODataDemo;Uid=produser;Pwd=secure_password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

## 🚦 API Endpoints Overview

### **OData Endpoints (Advanced Querying)**
```http
# Complex queries with filtering, expansion, and selection
GET /odata/CustomersOData?$filter=City eq 'Seattle'&$expand=Orders&$select=Name,City
GET /odata/ProductsOData?$orderby=Price desc&$top=10
GET /odata/OrdersOData?$expand=Customer,OrderItems&$filter=OrderDate ge 2024-01-01
```

### **Minimal API Endpoints (Simple Operations)**
```http
# Products
GET    /api/products           # Get all products
GET    /api/products/1         # Get product by ID
POST   /api/products           # Create product
PUT    /api/products/1         # Update product
DELETE /api/products/1         # Delete product

# Customers
GET    /api/customers          # Get all customers
GET    /api/customers/1        # Get customer by ID
GET    /api/customers/1/orders # Get customer orders
POST   /api/customers          # Create customer
PUT    /api/customers/1        # Update customer
DELETE /api/customers/1        # Delete customer

# Orders
GET    /api/orders             # Get all orders
GET    /api/orders/1           # Get order by ID
GET    /api/orders/recent?days=30  # Get recent orders
POST   /api/orders             # Create order
DELETE /api/orders/1           # Delete order
```

## 📊 Request/Response Examples

### **Create Product (Minimal API)**
```http
POST /api/products
Content-Type: application/json

{
  "name": "Surface Pro 11",
  "price": 1299.99
}
```

**Response:**
```json
{
  "id": 123,
  "name": "Surface Pro 11",
  "price": 1299.99,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

### **Query Customers (OData)**
```http
GET /odata/CustomersOData?$filter=City eq 'Seattle'&$expand=Orders($select=Id,OrderDate)&$select=Name,City,Orders
```

**Response:**
```json
{
  "value": [
    {
      "name": "John Doe",
      "city": "Seattle",
      "orders": [
        {"id": 1, "orderDate": "2024-01-10T00:00:00Z"},
        {"id": 2, "orderDate": "2024-01-12T00:00:00Z"}
      ]
    }
  ]
}
```

## 🔍 Error Handling Examples

### **Validation Error (400 Bad Request)**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."],
    "Price": ["The field Price must be between 0.01 and 1.7976931348623157E+308."]
  }
}
```

### **Global Exception (500 Internal Server Error)**
```json
{
  "error": {
    "message": "An internal server error occurred",
    "statusCode": 500,
    "timestamp": "2024-01-15T10:30:00Z",
    "path": "/api/products/123"
  }
}
```

## 🎛️ Logging Examples

### **Structured Logging Output**
```
[2024-01-15 10:30:00] [Information] Retrieving product with ID 123
[2024-01-15 10:30:01] [Information] Retrieved product: Surface Pro 11
[2024-01-15 10:30:02] [Information] EF Core SQL for query:
SELECT p.Id, p.Name, p.Price, p.CreatedAt, p.UpdatedAt 
FROM Products p 
WHERE p.Id = 123
```

## 🚀 Running the Application

### **Development**
```bash
# Set up user secrets
dotnet user-secrets set "ConnectionStrings:DBConnection" "Server=localhost;Database=ODataDemo;Uid=root;Pwd=yourpassword;"

# Run the application
dotnet run
```

### **Production**
```bash
# Set environment variables
export ConnectionStrings__DBConnection="Server=prodserver;Database=ODataDemo;..."
export ASPNETCORE_ENVIRONMENT=Production

# Run the application
dotnet run
```

## 🔗 API Documentation

When running in development, visit:
- **Swagger UI**: `http://localhost:5000/swagger`
- **Scalar UI**: `http://localhost:5000/scalar/v1`
- **OData Metadata**: `http://localhost:5000/odata/$metadata`

## 💡 Best Practices Implemented

### ✅ **Configuration**
- User secrets for development
- Environment-specific settings
- Secure production configuration

### ✅ **Architecture**
- Extension methods for DI
- Middleware pipeline organization
- Separation of concerns

### ✅ **API Design**
- TypedResults for type safety
- Consistent error responses
- OpenAPI documentation

### ✅ **Logging**
- Structured logging with context
- Different log levels per environment
- EF Core SQL logging in development

### ✅ **Error Handling**
- Global exception middleware
- Validation responses
- Production-safe error messages

### ✅ **Performance**
- Async/await throughout
- Database connection resilience
- Optimized EF Core queries

This architecture provides a solid foundation for building production-ready .NET 9 applications with both OData and Minimal API capabilities! 🎉