var builder = WebApplication.CreateBuilder(args);

// middlewares -> tratamento de request/response
builder.Services.AddControllersWithViews(); // Retorna page web

var app = builder.Build();

app.MapControllerRoute("default", "{controller}/{action}");

app.Run();
