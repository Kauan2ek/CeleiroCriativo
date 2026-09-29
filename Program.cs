var builder = WebApplication.CreateBuilder(args);

// middlewares -> tratamento de request/response
builder.Services.AddControllersWithViews(); // Retorna page web

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}");

app.Run();
