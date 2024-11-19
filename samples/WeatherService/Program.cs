using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddXmlSerializerFormatters(); // Add XML formatter support

// Add API Gateway services
builder.Services.AddApiGatewayServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Use API Gateway middleware
app.UseApiGateway();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
