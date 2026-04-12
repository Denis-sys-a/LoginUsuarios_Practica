using MySql.Data.MySqlClient;
using LoginUsuarios.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<MySqlConnection>(_ =>
    new MySqlConnection(builder.Configuration.GetConnectionString("MySqlConnection")));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Account/Login");
    return Task.CompletedTask;
});

app.Run();