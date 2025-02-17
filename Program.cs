using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataDemo.Model;
using Scalar.AspNetCore;

namespace ODataDemo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers()
        .AddOData(options =>
            options.Select()
            .Filter()
            .OrderBy()
            .Count()
            .SetMaxTop(100)
            .AddRouteComponents("odata", GetEdmModel())
        );

        builder.Services.AddOpenApi();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                // Allow any origin, any method, any header
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapScalarApiReference();

        app.UseAuthorization();

        app.MapControllers();

        app.UseCors("AllowAll");

        app.Run();
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();

        builder.EntitySet<ProductModel>("ProductsOData");
        builder.EntitySet<OrderModel>("OrdersOData");
        builder.EntitySet<CustomerModel>("CustomersOData");
        builder.EntitySet<OrderItem>("OrderItemsOData");

        return builder.GetEdmModel();
    }
}