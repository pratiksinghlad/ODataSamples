# ODataSamples

**.NET 9 OData API with Minimal APIs and Central Package Management**

This project demonstrates a modern, enterprise-grade OData implementation using .NET 9, Entity Framework Core, and the Repository pattern with Unit of Work.

## 🚀 Features

- **🎯 .NET 9** with C# 13 language features
- **📊 EF Core 9.0.2** Code First approach with MySQL database
- **🏗️ Repository Pattern** with Unit of Work for clean architecture
- **🔍 Full OData Support** with all query capabilities ($filter, $expand, $orderby, $select, $top, $skip, $count)
- **📚 Minimal APIs** alongside OData controllers
- **🔒 Clean Architecture** with dependency injection and separation of concerns
- **📦 Central Package Management** with Directory.Packages.props
- **🏥 Health Checks** for monitoring application status
- **📖 API Documentation** with Scalar UI
- **🌱 Data Seeding** for development and testing

## 🏛️ Architecture

### Repository Pattern Structure
```
Data/
├── Context/
│   └── ApplicationDbContext.cs      # EF Core DbContext
├── Repositories/
│   ├── Interfaces/                  # Repository contracts
│   │   ├── IRepository<T>.cs        # Generic repository interface
│   │   ├── IProductRepository.cs    # Product-specific operations
│   │   ├── ICustomerRepository.cs   # Customer-specific operations
│   │   └── IOrderRepository.cs      # Order-specific operations
│   └── Implementations/             # Repository implementations
│       ├── Repository<T>.cs         # Generic repository base class
│       ├── ProductRepository.cs     # Product repository
│       ├── CustomerRepository.cs    # Customer repository
│       └── OrderRepository.cs       # Order repository
├── UnitOfWork/
│   ├── IUnitOfWork.cs              # Unit of Work interface
│   └── UnitOfWork.cs               # Unit of Work implementation
└── Seeding/
    └── DataSeeder.cs               # Database seeding service
```

### Models with EF Core Annotations
- **ProductModel**: Products with name, price, and audit fields
- **CustomerModel**: Customers with name, city, and audit fields  
- **OrderModel**: Orders with date, customer relationship, and calculated properties
- **OrderItemModel**: Order line items with product name and price

## 🚦 Getting Started

### Prerequisites
- **.NET 9 SDK** (or later)
- **MySQL Server** (for production) or **SQLite** (auto-configured for development)

### Running the Application

1. **Clone and Build**:
   ```bash
   git clone <repository-url>
   cd ODataSamples
   dotnet build
   ```

2. **Configure Database**:
   Update `appsettings.secret.json` with your MySQL connection string:
   ```json
   {
     "ConnectionStrings": {
       "DBConnection": "Server=localhost;Database=employee;User=root;Password=admin;"
     }
   }
   ```

3. **Run the Application**:
   ```bash
   dotnet run
   ```

4. **Access APIs**:
   - **API Documentation**: `http://localhost:5203/scalar/v1`
   - **Health Check**: `http://localhost:5203/health`

## 📡 API Endpoints

### OData Endpoints
Base URL: `http://localhost:5203/odata`

#### Products (`/odata/ProductsOData`)
```http
# Get all products
GET /odata/ProductsOData

# Filter products by price
GET /odata/ProductsOData?$filter=Price gt 300

# Order by price descending, select specific fields
GET /odata/ProductsOData?$orderby=Price desc&$select=Name,Price

# Top 5 products with count
GET /odata/ProductsOData?$top=5&$count=true
```

#### Customers (`/odata/CustomersOData`)
```http
# Get all customers
GET /odata/CustomersOData

# Filter customers by city
GET /odata/CustomersOData?$filter=City eq 'Seattle'

# Get customers with their orders
GET /odata/CustomersOData?$expand=Orders
```

#### Orders (`/odata/OrdersOData`)
```http
# Get all orders
GET /odata/OrdersOData

# Get orders with customer and order items
GET /odata/OrdersOData?$expand=Customer,OrderItems

# Filter orders by date range
GET /odata/OrdersOData?$filter=OrderDate ge 2024-01-01T00:00:00Z

# Get recent orders with full details
GET /odata/OrdersOData?$filter=OrderDate ge 2024-12-01T00:00:00Z&$expand=Customer,OrderItems&$orderby=OrderDate desc
```

### Minimal API Endpoints
Base URL: `http://localhost:5203/api`

#### Products (`/api/products`)
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID  
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update existing product
- `DELETE /api/products/{id}` - Delete product

#### Orders (`/api/orders`)
- `GET /api/orders` - Get all orders with full details
- `GET /api/orders/{id}` - Get order by ID with customer and items

#### Customers (`/api/customers`)
- `GET /api/customers` - Get all customers
- `GET /api/customers/{id}` - Get customer by ID

## 🎯 OData Query Examples

### Advanced Filtering
```http
# Products priced between $200 and $500
GET /odata/ProductsOData?$filter=Price ge 200 and Price le 500

# Customers from specific cities
GET /odata/CustomersOData?$filter=City in ('Seattle','Portland')

# Orders from the last 30 days
GET /odata/OrdersOData?$filter=OrderDate ge 2024-12-01T00:00:00Z
```

### Expansion and Selection
```http
# Orders with customer and item details, specific fields only
GET /odata/OrdersOData?$expand=Customer,OrderItems&$select=Id,OrderDate,Customer/Name,OrderItems/ProductName

# Customers with their order history
GET /odata/CustomersOData?$expand=Orders($expand=OrderItems)
```

### Sorting and Paging
```http
# Most expensive products first, top 10
GET /odata/ProductsOData?$orderby=Price desc&$top=10

# Customers by name, skip first 5, take next 10
GET /odata/CustomersOData?$orderby=Name&$skip=5&$top=10&$count=true
```

## 🔧 Repository Pattern Benefits

### Generic Repository (`IRepository<T>`)
- **CRUD Operations**: Add, Update, Delete, GetById
- **Querying**: GetAll, GetWhere, GetWithInclude
- **OData Support**: Full queryable interface support
- **Async/Await**: All operations support async patterns

### Specific Repositories
- **ProductRepository**: Basic CRUD with product-specific queries
- **CustomerRepository**: Customer queries with city-based filtering
- **OrderRepository**: Complex order queries with relationships, date ranges, and summaries

### Unit of Work Pattern
- **Transaction Management**: Coordinate multiple repository operations
- **Single Save**: SaveChangesAsync() for all repositories
- **Repository Access**: Centralized access to all repositories
- **Dependency Injection**: Clean IoC container integration

## 🏗️ Database Design

### Entity Relationships
```
Products (1:N) ← OrderItems (N:1) → Orders (N:1) → Customers
```

### Key Features
- **Audit Fields**: CreatedAt, UpdatedAt on all entities
- **Foreign Keys**: Proper relationships with cascade behaviors
- **Indexes**: Optimized queries for names, dates, and foreign keys
- **Constraints**: Data validation at the database level

### Development vs Production

- **All Environments**: MySQL with EF Core migrations
- **Database Creation**: Automatic migration on application startup
- **Connection Management**: Pooling, retry policies, and proper charset support

## 📊 Performance Features

### Entity Framework Optimizations
- **AsNoTracking()**: Read-only queries for better performance
- **Connection Pooling**: Reuse database connections
- **Retry Logic**: Automatic retry on transient failures
- **Query Optimization**: Proper use of Include() for eager loading

### Repository Pattern Benefits
- **Caching**: Easy to add caching layers
- **Unit Testing**: Mock repositories for testing
- **Business Logic**: Keep domain logic separate from data access
- **Maintainability**: Clear separation of concerns

## 🧪 Testing

The repository pattern makes unit testing straightforward:

```csharp
// Mock the repository
var mockRepository = new Mock<IProductRepository>();
var mockUnitOfWork = new Mock<IUnitOfWork>();

// Setup behavior
mockUnitOfWork.Setup(u => u.Products).Returns(mockRepository.Object);

// Test your controller/service
var controller = new ProductsController(mockUnitOfWork.Object);
```

## 🛠️ Development Tools

### Package Management
- **Central Package Management**: All versions managed in Directory.Packages.props
- **Version Consistency**: Ensures all projects use the same package versions

### Build Configuration
- **Directory.Build.props**: Shared build configuration
- **Target Framework**: .NET 9 with C# 13 features
- **Nullable Reference Types**: Enhanced null safety

### API Documentation
- **Scalar UI**: Modern, interactive API documentation
- **OpenAPI**: Complete API specification
- **OData Metadata**: Automatic metadata document generation

## 🔍 Monitoring and Health

### Health Checks
- **Application Health**: `/health` endpoint
- **Database Health**: EF Core context health checks
- **Custom Checks**: Easy to add service-specific health checks

### Logging
- **Structured Logging**: JSON-formatted logs
- **EF Core Logging**: SQL query logging in development
- **Request Logging**: HTTP request/response logging

## 🚀 Deployment

### Configuration
- **Environment-Specific**: appsettings per environment
- **Secrets Management**: appsettings.secret.json (not in source control)
- **Connection Strings**: Environment-specific database configuration

### Production Considerations
- **MySQL**: Production-ready database with proper connection management
- **Health Checks**: Monitoring endpoints for load balancers
- **Logging**: Structured logging for monitoring systems
- **CORS**: Configurable cross-origin resource sharing

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📞 Support

For questions or issues, please open an issue in the repository or contact the development team.

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