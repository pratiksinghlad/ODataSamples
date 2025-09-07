# ODataSamples

**.NET 9 OData API with Minimal APIs and Central Package Management**

This repository demonstrates how to build modern OData APIs using **.NET 9** with both traditional controllers and **Minimal APIs**. The project showcases OData (Open Data Protocol) implementation alongside standard REST APIs, featuring central build configuration and package management.

## 🚀 Features

### Modern .NET 9 Implementation
- **C# 13** language features and syntax
- **Minimal APIs** for lightweight endpoint definitions
- **Central Package Management** via `Directory.Packages.props`
- **Central Build Configuration** via `Directory.Build.props`
- **Global Using Statements** for cleaner code

### API Approaches
- **OData Controllers**: Full OData query capabilities with `$filter`, `$expand`, `$orderby`, etc.
- **Traditional REST Controllers**: Standard CRUD operations
- **Minimal APIs**: Lightweight endpoint definitions with modern C# patterns

### Libraries & Tools
- **Microsoft.AspNetCore.OData**: OData v4 implementation for .NET
- **Microsoft.AspNetCore.OpenApi**: OpenAPI 3.0 support
- **Scalar.AspNetCore**: Modern API documentation and testing interface
- **Microsoft.EntityFrameworkCore**: Data access abstractions

## 📁 Project Structure

```
├── Directory.Build.props          # Central build configuration
├── Directory.Packages.props       # Central package management
├── GlobalUsings.cs                # Global using declarations
├── Program.cs                     # Application entry point with Minimal APIs
├── Controllers/                   # Traditional and OData controllers
│   ├── ProductsController.cs      # REST API controller
│   ├── ProductsODataController.cs # OData controller
│   └── OrdersODataController.cs   # OData controller with expansions
├── Model/                         # Data models with C# 13 features
│   ├── ProductModel.cs
│   ├── CustomerModel.cs
│   ├── OrderModel.cs
│   └── OrderItemModel.cs
└── Data/                          # In-memory data provider
    └── InMemoryData.cs
```

## 🔧 Key Modernizations

### Central Package Management
All package versions are managed centrally in `Directory.Packages.props`, making it easier to maintain consistent versions across projects.

### C# 13 Features Used
- **Collection expressions**: `[]` syntax for collections
- **Required properties**: `required` keyword for mandatory properties
- **Target-typed new expressions**: `new()` constructor calls
- **Pattern matching**: Enhanced null checks and type patterns
- **XML documentation**: Comprehensive API documentation

### Minimal APIs Integration
The project combines traditional controllers with Minimal APIs, showing both approaches:
- OData functionality via controllers for complex querying
- Simple CRUD operations via Minimal APIs for performance

## 🌐 API Endpoints

### OData Endpoints (Full Query Support)
- `GET /odata/ProductsOData` - Products with OData queries
- `GET /odata/OrdersOData` - Orders with OData queries and expansions
- `POST /odata/ProductsOData` - Create products
- `PUT /odata/ProductsOData({id})` - Update products

### Minimal API Endpoints
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Traditional REST Endpoints
- `GET /api/Products` - Traditional REST controller

## 📖 OData Query Examples

```http
# Filter products by price range and sort by name
GET /odata/ProductsOData?$filter=Price ge 300 and Price le 1000&$orderby=Name

# Get orders with customer and order items expanded
GET /odata/OrdersOData?$expand=Customer,OrderItems

# Select specific properties with pagination
GET /odata/ProductsOData?$select=Name,Price&$top=10&$skip=5

# Get specific order with expansions
GET /odata/OrdersOData(101)?$expand=Customer,OrderItems
```