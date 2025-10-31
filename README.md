# OData .NET 9 Sample Application

A comprehensive ASP.NET Core 9 application demonstrating OData integration with Entity Framework Core, MySQL, and the Repository pattern using C# 13 features.

## 📋 Table of Contents

- [Architecture Overview](#architecture-overview)
- [How OData Filtering Works](#how-odata-filtering-works)
- [Customer CRUD Example - Complete Flow](#customer-crud-example---complete-flow)
- [Repository Pattern Implementation](#repository-pattern-implementation)
- [Project Structure](#project-structure)
- [Setup and Configuration](#setup-and-configuration)
- [OData Query Examples](#odata-query-examples)
- [API Endpoints](#api-endpoints)

---

## 🏗️ Architecture Overview

This application follows a clean architecture pattern with the following layers:

```
┌─────────────────────────────────────────────────────────────┐
│                  OData Minimal API                          │
│   (CustomerEndpoints, ProductEndpoints, OrderEndpoints)     │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                  Unit of Work                               │
│              (IUnitOfWork, UnitOfWork)                      │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                 Repository Layer                            │
│    (IRepository<T>, ICustomerRepository, etc.)             │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│              Entity Framework Core                          │
│              (ApplicationDbContext)                         │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                  MySQL Database                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔍 How OData Filtering Works

### Modern .NET 9 OData with Minimal API

OData functionality is implemented using .NET 9's modern Minimal API approach. Here's how it works:

#### 1. **OData Configuration** (Program.cs)

```csharp
builder.Services.AddMvc()
    .AddOData(options => options
        .Select()     // Enables $select queries
        .Filter()     // Enables $filter queries  ⭐ KEY FOR FILTERING
        .OrderBy()    // Enables $orderby queries
        .Count()      // Enables $count queries
        .Expand()     // Enables $expand queries
        .SetMaxTop(1000)
        .AddRouteComponents("odata", GetEdmModel()));
```

#### 2. **EDM Model Configuration**

```csharp
static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    
    // Register entity sets - creates OData endpoints
    builder.EntitySet<CustomerModel>("Customers");
    builder.EntitySet<ProductModel>("Products");
    builder.EntitySet<OrderModel>("Orders");
    
    // Configure entity properties
    builder.EntityType<CustomerModel>()
        .HasKey(c => c.Id);
    
    return builder.GetEdmModel();
}
```

#### 3. **Minimal API OData Endpoints**

```csharp
// Endpoints/ODataEndpoints.cs
public static class ODataEndpoints
{
    public static WebApplication MapODataEndpoints(this WebApplication app)
    {
        app.MapGroup("odata/Customers")
            .MapGet("/", ([FromServices] IUnitOfWork unitOfWork) =>
            {
                // ⭐ CRITICAL: Returns IQueryable for OData processing
                var customers = unitOfWork.Customers.GetAll();
                return Results.Ok(customers);
            })
            .WithOpenApi();
    }
    catch (Exception ex)
    {
        // Error handling
    }
}
```

### 🎯 **The Filter Flow Process**

When a client makes an OData request like:
```
GET /odata/CustomersOData?$filter=City eq 'Seattle'&$orderby=Name
```

Here's what happens step by step:

1. **Request Parsing**: OData middleware parses the query string
2. **EnableQuery Processing**: The `[EnableQuery]` attribute intercepts the `IQueryable<CustomerModel>`
3. **Query Translation**: OData translates `$filter=City eq 'Seattle'` into LINQ: `.Where(c => c.City == "Seattle")`
4. **Query Composition**: OData builds the complete LINQ query
5. **EF Core Translation**: Entity Framework converts LINQ to SQL
6. **Database Execution**: MySQL executes the optimized SQL query
7. **Result Materialization**: Only filtered results are returned

### 🔑 **Why Repository Returns IQueryable**

```csharp
// In Repository<TEntity>
public virtual IQueryable<TEntity> GetAll()
{
    return _dbSet.AsNoTracking();  // ⭐ IQueryable - NOT executed yet!
}
```

**This is crucial because:**
- `IQueryable` represents a query that hasn't been executed
- OData can modify this query by adding WHERE, ORDER BY, SELECT clauses
- Only the final composed query hits the database
- This enables server-side filtering and optimal performance

---

## 👥 Customer CRUD Example - Complete Flow

Let's trace a complete customer operation from HTTP request to database:

### � **GET All Customers with Filter**

**HTTP Request:**
```http
GET /odata/CustomersOData?$filter=City eq 'Seattle'&$expand=Orders&$select=Name,City
```

**Flow Trace:**

1. **� HTTP Request** arrives at ASP.NET Core
2. **�🎯 Route Matching** → `CustomersODataController.Get()`
3. **🏗️ Controller Action:**
   ```csharp
   [EnableQuery(MaxExpansionDepth = 3, MaxTop = 1000)]
   public IActionResult Get()
   {
       var customers = _unitOfWork.Customers.GetAll(); // Step 4
       return Ok(customers); // Step 8
   }
   ```
4. **🏢 Unit of Work:**
   ```csharp
   public ICustomerRepository Customers =>
       _customerRepository ??= new CustomerRepository(_context);
   ```
5. **📚 Customer Repository:**
   ```csharp
   // CustomerRepository inherits from Repository<CustomerModel>
   public class CustomerRepository : Repository<CustomerModel>, ICustomerRepository
   ```
6. **🗃️ Base Repository:**
   ```csharp
   public virtual IQueryable<TEntity> GetAll()
   {
       return _dbSet.AsNoTracking(); // Returns IQueryable<CustomerModel>
   }
   ```
7. **⚡ OData Magic** (`[EnableQuery]` processes the IQueryable):
   - Parses `$filter=City eq 'Seattle'` → `.Where(c => c.City == "Seattle")`
   - Parses `$expand=Orders` → `.Include(c => c.Orders)`
   - Parses `$select=Name,City` → `.Select(c => new { c.Name, c.City })`
8. **�️ EF Core** converts to SQL:
   ```sql
   SELECT c.Name, c.City 
   FROM Customers c 
   LEFT JOIN Orders o ON c.Id = o.CustomerId 
   WHERE c.City = 'Seattle'
   ```

> **🔍 Want to understand the complete automatic conversion process?**  
> See our detailed guide: [How OData Automatically Converts IQueryable to SQL](./ODATA_AUTOMATIC_CONVERSION.md)
>
> This document explains exactly how your simple controller code:
> ```csharp
> var customers = _unitOfWork.Customers.GetAll();
> return Ok(customers);
> ```
> Automatically becomes optimized SQL queries with zero additional code!

9. **🏗️ MySQL** executes query and returns results
10. **📤 Response** sent back as JSON

### 📝 **POST Create Customer**

**HTTP Request:**
```http
POST /odata/CustomersOData
Content-Type: application/json

{
    "Name": "John Doe",
    "City": "Portland"
}
```

**Flow Trace:**

1. **🌐 HTTP POST** → `CustomersODataController.Post()`
2. **🎯 Controller Action:**
   ```csharp
   public async Task<IActionResult> Post([FromBody] CustomerModel customer)
   {
       // Validation
       if (!ModelState.IsValid)
           return BadRequest(ModelState);
           
       // Reset auto-generated fields
       customer.Id = 0;
       customer.CreatedAt = DateTime.UtcNow;
       customer.UpdatedAt = DateTime.UtcNow;
       
       var createdCustomer = await _unitOfWork.Customers.AddAsync(customer); // Step 3
       await _unitOfWork.SaveChangesAsync(); // Step 6
       
       return Created($"/odata/CustomersOData({createdCustomer.Id})", createdCustomer);
   }
   ```
3. **📚 Customer Repository AddAsync:**
   ```csharp
   // Inherited from Repository<CustomerModel>
   public virtual async Task<TEntity> AddAsync(TEntity entity)
   {
       ArgumentNullException.ThrowIfNull(entity);
       var entityEntry = await _dbSet.AddAsync(entity);
       return entityEntry.Entity;
   }
   ```
4. **🗃️ EF Core** tracks the entity in `Added` state
5. **🏢 Unit of Work SaveChanges:**
   ```csharp
   public async Task<int> SaveChangesAsync()
   {
       return await _context.SaveChangesAsync(); // Step 6
   }
   ```
6. **🗄️ EF Core** generates and executes:
   ```sql
   INSERT INTO Customers (Name, City, CreatedAt, UpdatedAt) 
   VALUES ('John Doe', 'Portland', '2025-09-20 10:30:00', '2025-09-20 10:30:00');
   SELECT LAST_INSERT_ID();
   ```
7. **📤 Response:** `201 Created` with location header and customer data

### ✏️ **PUT Update Customer**

**HTTP Request:**
```http
PUT /odata/CustomersOData(5)
Content-Type: application/json

{
    "Name": "John Smith",
    "City": "Seattle"
}
```

**Flow Trace:**

1. **🌐 HTTP PUT** → `CustomersODataController.Put(int key, CustomerModel updatedCustomer)`
2. **🔍 Get Existing Customer:**
   ```csharp
   var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(key);
   ```
3. **📚 Repository GetByIdAsync:**
   ```csharp
   public virtual async Task<TEntity?> GetByIdAsync(int id)
   {
       return await _dbSet.FindAsync(id); // EF Core Find by primary key
   }
   ```
4. **✏️ Update Properties:**
   ```csharp
   existingCustomer.Name = updatedCustomer.Name;
   existingCustomer.City = updatedCustomer.City;
   existingCustomer.UpdatedAt = DateTime.UtcNow;
   
   _unitOfWork.Customers.Update(existingCustomer);
   ```
5. **�️ Repository Update:**
   ```csharp
   public virtual void Update(TEntity entity)
   {
       _dbSet.Update(entity); // Marks entity as Modified
   }
   ```
6. **💾 Save Changes** → SQL:
   ```sql
   UPDATE Customers 
   SET Name = 'John Smith', City = 'Seattle', UpdatedAt = '2025-09-20 10:35:00'
   WHERE Id = 5;
   ```

### 🗑️ **DELETE Customer**

**HTTP Request:**
```http
DELETE /odata/CustomersOData(5)
```

**Flow Trace:**

1. **🌐 HTTP DELETE** → `CustomersODataController.Delete(int key)`
2. **🗑️ Delete Operation:**
   ```csharp
   var deleted = await _unitOfWork.Customers.DeleteByIdAsync(key);
   ```
3. **📚 Repository DeleteByIdAsync:**
   ```csharp
   public virtual async Task<bool> DeleteByIdAsync(int id)
   {
       var entity = await GetByIdAsync(id);
       if (entity is null)
           return false;
           
       Delete(entity);
       return true;
   }
   ```
4. **🗄️ EF Core** executes:
   ```sql
   DELETE FROM Customers WHERE Id = 5;
   ```

---

## 🗃️ Repository Pattern Implementation

### Interface Hierarchy

```csharp
IRepository<TEntity>              // Generic CRUD operations
    ↑
ICustomerRepository              // Customer-specific operations
    ↑
CustomerRepository               // Implementation
```

### Generic Repository Interface

```csharp
public interface IRepository<TEntity> where TEntity : class
{
    // Query operations (return IQueryable for OData)
    IQueryable<TEntity> GetAll();
    IQueryable<TEntity> GetWhere(Expression<Func<TEntity, bool>> filter);
    
    // Individual operations
    Task<TEntity?> GetByIdAsync(int id);
    
    // CRUD operations
    Task<TEntity> AddAsync(TEntity entity);
    void Update(TEntity entity);
    Task<bool> DeleteByIdAsync(int id);
    
    // Advanced queries
    IQueryable<TEntity> GetWithInclude(params Expression<Func<TEntity, object>>[] includeProperties);
}
```

### Customer-Specific Repository

```csharp
public interface ICustomerRepository : IRepository<CustomerModel>
{
    IQueryable<CustomerModel> GetByCity(string city);
    IQueryable<CustomerModel> GetWithOrders();
    IQueryable<CustomerModel> GetByNamePattern(string namePattern);
}

public class CustomerRepository : Repository<CustomerModel>, ICustomerRepository
{
    public IQueryable<CustomerModel> GetByCity(string city)
    {
        return GetWhere(c => c.City.Equals(city, StringComparison.OrdinalIgnoreCase));
    }
    
    public IQueryable<CustomerModel> GetWithOrders()
    {
        return GetWithInclude(c => c.Orders);
    }
}
```

---

## � Project Structure

```
ODataSamples/
├── Controllers/                    # OData and REST API controllers
│   ├── CustomersODataController.cs # Customer OData endpoint
│   ├── ProductsODataController.cs  # Product OData endpoint
│   └── OrdersODataController.cs    # Order OData endpoint
├── Data/
│   ├── Context/
│   │   └── ApplicationDbContext.cs # EF Core DbContext
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs      # Generic repository interface
│   │   │   ├── ICustomerRepository.cs
│   │   │   ├── IProductRepository.cs
│   │   │   └── IOrderRepository.cs
│   │   └── Implementations/
│   │       ├── Repository.cs       # Generic repository implementation
│   │       ├── CustomerRepository.cs
│   │       ├── ProductRepository.cs
│   │       └── OrderRepository.cs
│   ├── UnitOfWork/
│   │   ├── IUnitOfWork.cs          # Unit of Work interface
│   │   └── UnitOfWork.cs           # Unit of Work implementation
│   ├── Seeding/
│   │   └── DataSeeder.cs           # Database seeding
│   └── InMemoryData.cs             # Sample data
├── Model/                          # Entity models
│   ├── CustomerModel.cs
│   ├── ProductModel.cs
│   ├── OrderModel.cs
│   └── OrderItemModel.cs
├── Program.cs                      # Application startup and configuration
├── appsettings.json               # Application settings
└── appsettings.secret.json        # Connection strings (git-ignored)
```

---

## ⚙️ Setup and Configuration

### Prerequisites

- .NET 9 SDK
- MySQL Server 8.0+
- Visual Studio 2022 or VS Code

### Database Configuration

1. **Create MySQL Database:**
   ```sql
   CREATE DATABASE employee;
   ```

2. **Update Connection String** in `appsettings.secret.json`:
   ```json
   {
     "ConnectionStrings": {
       "DBConnection": "server=localhost;uid=root;pwd=admin;database=employee;"
     }
   }
   ```

3. **Run the Application:**
   ```bash
   dotnet run
   ```

The application automatically:
- Applies EF Core migrations
- Seeds sample data
- Starts on `http://localhost:5203`

### Technology Stack

- **Framework:** ASP.NET Core 9 with C# 13
- **ORM:** Entity Framework Core 9.0.2
- **Database:** MySQL with Pomelo.EntityFrameworkCore.MySql
- **OData:** Microsoft.AspNetCore.OData 9.2.0
- **Architecture:** Repository Pattern + Unit of Work

---

## 🔍 OData Query Examples

### Basic Queries

```http
# Get all customers
GET /odata/CustomersOData

# Get customer by ID
GET /odata/CustomersOData(1)

# Filter customers by city
GET /odata/CustomersOData?$filter=City eq 'Seattle'

# Order customers by name
GET /odata/CustomersOData?$orderby=Name

# Select specific fields
GET /odata/CustomersOData?$select=Name,City

# Get top 5 customers
GET /odata/CustomersOData?$top=5

# Skip first 10 customers
GET /odata/CustomersOData?$skip=10

# Count customers
GET /odata/CustomersOData?$count=true
```

### Advanced Queries

```http
# Expand related data
GET /odata/CustomersOData?$expand=Orders

# Complex filtering
GET /odata/CustomersOData?$filter=City eq 'Seattle' and contains(Name,'John')

# Multiple expansions with select
GET /odata/CustomersOData?$expand=Orders($select=Id,OrderDate)&$select=Name,City

# Nested filtering
GET /odata/CustomersOData?$expand=Orders($filter=OrderDate gt 2024-01-01)

# Ordering with expansion
GET /odata/CustomersOData?$expand=Orders&$orderby=Name&$filter=City eq 'Portland'
```

### Filter Operators

```http
# Equality
$filter=City eq 'Seattle'

# Inequality  
$filter=City ne 'Seattle'

# Greater than
$filter=Id gt 10

# Contains
$filter=contains(Name,'John')

# Starts with
$filter=startswith(Name,'J')

# Logical operators
$filter=City eq 'Seattle' and Name ne 'John'
$filter=City eq 'Seattle' or City eq 'Portland'

# Date filtering
$filter=CreatedAt gt 2024-01-01T00:00:00Z
```

---

## 🔗 API Endpoints

### OData Endpoints (Full Query Support)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/odata/CustomersOData` | Get all customers with OData queries |
| GET | `/odata/CustomersOData(id)` | Get customer by ID |
| POST | `/odata/CustomersOData` | Create new customer |
| PUT | `/odata/CustomersOData(id)` | Update customer |
| DELETE | `/odata/CustomersOData(id)` | Delete customer |
| GET | `/odata/ProductsOData` | Get all products with OData queries |
| GET | `/odata/OrdersOData` | Get all orders with OData queries |

### Custom OData Functions

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/odata/CustomersOData/GetByCity?city=Seattle` | Get customers by city |

### REST API Endpoints (Traditional)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products (traditional REST) |
| GET | `/api/products/{id}` | Get product by ID |

### Metadata and Service Document

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/odata/$metadata` | OData metadata document |
| GET | `/odata/` | OData service document |

---

## 🎯 Key Features

- **� Full OData Query Support:** $filter, $orderby, $select, $expand, $top, $skip, $count
- **🗄️ MySQL Integration:** Complete migration from SQLite to MySQL
- **🏗️ Repository Pattern:** Clean separation of concerns with Unit of Work
- **⚡ Performance Optimized:** AsNoTracking for read operations
- **🔒 Validation:** Model validation with detailed error handling
- **📊 Entity Relationships:** Proper foreign key relationships and navigation properties
- **🔄 Auto-Migration:** Database schema updates on application start
- **🌱 Data Seeding:** Automatic sample data population

---

## 🤝 Contributing

This is a sample project demonstrating OData integration with .NET 9. Feel free to explore, learn, and adapt the patterns shown here for your own projects.

---

## 🎯 When to Use OData vs Custom Repository Methods

This section explains exactly when to use each approach based on the implementation above.

### ✅ **USE OData Filtering** (Recommended for most cases)

**When to use:**
- Simple filtering, sorting, and pagination
- Flexible queries that clients might want to customize
- Standard CRUD operations
- When you want clients to compose their own queries

**Examples:**
```http
# Let OData handle filtering - much more flexible
GET /odata/CustomersOData?$filter=City eq 'Seattle'
GET /odata/CustomersOData?$filter=City eq 'Seattle' and contains(Name,'John')
GET /odata/CustomersOData?$orderby=Name&$top=10&$skip=20
GET /odata/CustomersOData?$select=Name,City&$expand=Orders
```

**Controller Implementation:**
```csharp
[EnableQuery] // ⭐ This handles all filtering automatically
public IActionResult Get()
{
    // Only calls GetAll() - OData adds filtering
    var customers = _unitOfWork.Customers.GetAll();
    return Ok(customers);
}
```

**Why this is better:**
- More flexible - clients can filter by any field
- Less code to maintain
- Standard OData conventions
- Automatic query optimization

### ✅ **USE Custom Repository Methods** (For business logic)

**When to use:**
- Complex business rules that can't be expressed in OData
- Pre-built queries for specific use cases
- Complex joins with calculated fields
- Performance-critical queries that need optimization
- Business operations that combine multiple data sources

**Examples from the Service Layer:**

#### **1. Business Statistics:**
```csharp
// Service method that uses GetByCity()
public async Task<CustomerCityStatisticsDto> GetCityStatisticsAsync(string city)
{
    // This actually calls GetByCity() repository method
    var customers = await _unitOfWork.Customers
        .GetByCity(city) // 🎯 Custom repository method!
        .Include(c => c.Orders)
        .ToListAsync();

    return new CustomerCityStatisticsDto
    {
        City = city,
        TotalCustomers = customers.Count,
        TotalOrders = customers.SelectMany(c => c.Orders).Count(),
        // Complex business calculations...
    };
}
```

#### **2. Complex Date-based Analysis:**
```csharp
// Service method that uses GetByOrderDateRange()
public async Task<List<CustomerSummaryDto>> GetActiveCustomersAsync()
{
    var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
    
    // This calls GetByOrderDateRange() custom repository method
    var customers = await _unitOfWork.Customers
        .GetByOrderDateRange(thirtyDaysAgo, DateTime.UtcNow) // 🎯 Custom method!
        .Select(c => new CustomerSummaryDto { /* ... */ })
        .ToListAsync();
    
    return customers;
}
```

#### **3. Performance-Optimized Queries:**
```csharp
// Service method that uses GetWithOrderStatistics()
public async Task<CustomerDashboardDto> GetDashboardAsync(int customerId)
{
    // This calls GetWithOrderStatistics() for optimized includes
    var customer = await _unitOfWork.Customers
        .GetWithOrderStatistics() // 🎯 Pre-optimized includes!
        .FirstOrDefaultAsync(c => c.Id == customerId);
    
    // Complex business logic calculations...
}
```

### ❌ **DON'T Use Custom Repository Methods for Simple Filtering**

**Wrong approach:**
```csharp
// ❌ Unnecessary custom method
[HttpGet("wrong/by-city")]
public IActionResult GetByCity(string city)
{
    // This unnecessarily calls GetByCity() when OData would work better
    var customers = _unitOfWork.Customers.GetByCity(city);
    return Ok(customers);
}
```

**Right approach:**
```csharp
// ✅ Let OData handle it
[EnableQuery]
public IActionResult Get()
{
    // Client can add ?$filter=City eq 'Seattle' and it works automatically
    var customers = _unitOfWork.Customers.GetAll();
    return Ok(customers);
}
```

### 🔄 **The Complete Flow Comparison**

#### **OData Flow (Recommended for queries):**
```
1. Client: GET /odata/CustomersOData?$filter=City eq 'Seattle'
2. Controller: GetAll() called
3. Repository: Returns IQueryable<Customer>
4. OData: Adds WHERE clause automatically
5. EF Core: Generates optimized SQL
6. Database: Executes efficient query
```

#### **Custom Repository Flow (For business logic):**
```
1. Client: GET /api/CustomerDemo/business-logic/city-statistics?city=Seattle
2. Controller: Calls CustomerService
3. Service: Calls GetByCity() repository method
4. Repository: Returns IQueryable with predefined filter
5. Service: Adds business logic and calculations
6. Database: Executes with custom business rules
```

### 📊 **Performance Comparison**

| Approach | Flexibility | Performance | Maintenance | Use Case |
|----------|-------------|-------------|-------------|----------|
| **OData Filtering** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | General queries |
| **Custom Repository** | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | Business logic |

### 🎯 **Practical Guidelines**

1. **Start with OData** - Use `[EnableQuery]` for 80% of your read operations
2. **Add Custom Methods** only when you need:
   - Complex business calculations
   - Performance-critical queries
   - Multi-step business operations
   - Queries that can't be expressed in OData

3. **Service Layer Pattern** - Put business logic in services that use custom repository methods
4. **Controller Simplicity** - Keep controllers thin, delegate to services for complex operations

### 🔍 **Debug Output Example**

When you call different endpoints, you'll see different repository methods being called:

**OData Endpoint:**
```
GET /odata/CustomersOData?$filter=City eq 'Seattle'
Console Output:
🔍 Repository<CustomerModel>.GetAll() called
   ↳ This is used by OData - filtering will be added by [EnableQuery] attribute
```

**Business Logic Endpoint:**
```
GET /api/CustomerDemo/business-logic/city-statistics?city=Seattle
Console Output:
🏢 CustomerService.GetCityStatisticsAsync() called for city: Seattle
🏢 CustomerRepository.GetByCity() called with city: Seattle
   ↳ This is a CUSTOM repository method - used by business logic, not OData
🔍 Repository<CustomerModel>.GetWhere() called with filter: c => c.City.Equals(city, ...)
   ↳ This indicates CUSTOM repository method usage, NOT OData filtering
```

This clearly shows **when** each approach is used and **why** you need both patterns.

---

## 📚 Additional Resources

- [OData Documentation](https://docs.microsoft.com/en-us/odata/)
- [ASP.NET Core OData](https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Repository Pattern](https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application)

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