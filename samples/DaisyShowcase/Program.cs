var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.WebHost.ConfigureKestrel(o => o.AllowSynchronousIO = true);

var app = builder.Build();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Showcase}/{action=Index}/{id?}");

app.Run();
