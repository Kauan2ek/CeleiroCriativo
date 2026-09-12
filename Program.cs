using System.Text.Json.Serialization;
using APICeleiroCriativo.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<CeleiroCriativoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

const string PoliticaFrontEnd = "FrontEnd";
builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaFrontEnd, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CeleiroCriativoContext>();
    context.Database.EnsureCreated();
    await DbInitializer.SeedAsync(context);
}

app.UseStaticFiles(); // serve System/APICeleiroCriativo/wwwroot/uploads (documentos)
app.UseCors(PoliticaFrontEnd);
app.MapControllers();

app.Run();
