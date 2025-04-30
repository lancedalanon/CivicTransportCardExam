using Microsoft.EntityFrameworkCore;
using CivicTransportCard.Data;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider
        .SetAttemptedValueIsInvalidAccessor((value, fieldName) =>
            $"'{fieldName}' is not found.");
    options.ModelBindingMessageProvider
        .SetUnknownValueIsInvalidAccessor(fieldName =>
            $"'{fieldName}' is not found.");
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<TransportCardService>();
builder.Services.AddScoped<DiscountedTransportCardService>();

// Register CardContext with Pomelo MySQL provider
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CardContext>(options =>
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

// Swagger UI
builder.Services
    .AddControllers()
    .AddJsonOptions(opts => {
        opts.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() {
        Title = "CivicTransportCard API",
        Version = "v1"
    });

    c.EnableAnnotations();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CivicTransportCard API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
