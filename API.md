# API Endpoints

## OData Endpoints

The application exposes the following OData-enabled endpoints:

### Customers

- `GET /odata/Customers` - Get all customers with OData query support
  - Supports: `$filter`, `$expand`, `$select`, `$orderby`, `$top`, `$skip`, `$count`
  - Example: `GET /odata/Customers?$filter=City eq 'London'&$orderby=Name`

- `GET /odata/Customers({id})` - Get customer by ID with OData query support
  - Supports: `$expand`, `$select`
  - Example: `GET /odata/Customers(1)?$expand=Orders`

- `POST /odata/Customers` - Create a new customer
- `PUT /odata/Customers({id})` - Update an existing customer
- `DELETE /odata/Customers({id})` - Delete a customer

### Products

- `GET /odata/Products` - Get all products with OData query support
  - Supports: `$filter`, `$expand`, `$select`, `$orderby`, `$top`, `$skip`, `$count`
  - Example: `GET /odata/Products?$filter=Price gt 100&$orderby=Name`

- `GET /odata/Products({id})` - Get product by ID with OData query support
  - Supports: `$expand`, `$select`
  - Example: `GET /odata/Products(1)?$select=Name,Price`

- `POST /odata/Products` - Create a new product
- `PUT /odata/Products({id})` - Update an existing product
- `DELETE /odata/Products({id})` - Delete a product

### Orders

- `GET /odata/Orders` - Get all orders with OData query support
  - Supports: `$filter`, `$expand`, `$select`, `$orderby`, `$top`, `$skip`, `$count`
  - Example: `GET /odata/Orders?$filter=CustomerId eq 1&$expand=OrderItems`

- `GET /odata/Orders({id})` - Get order by ID with OData query support
  - Supports: `$expand`, `$select`
  - Example: `GET /odata/Orders(1)?$expand=OrderItems($expand=Product)`

- `POST /odata/Orders` - Create a new order
- `DELETE /odata/Orders({id})` - Delete an order

## Example Queries

### Complex Filtering

```http
GET /odata/Customers?$filter=City eq 'London' and Orders/any(o: o.Amount gt 1000)
```

### Expanding Related Entities

```http
GET /odata/Orders?$expand=Customer,OrderItems($expand=Product)&$filter=Amount gt 500
```

### Select Specific Fields

```http
GET /odata/Products?$select=Name,Price,Description&$orderby=Price desc&$top=10
```

### Aggregate Functions

```http
GET /odata/Orders?$apply=aggregate(Amount with sum as TotalAmount)
```

All endpoints are documented with OpenAPI/Swagger, making them easily testable through the built-in Swagger UI.
