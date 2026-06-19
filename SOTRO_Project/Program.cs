using SOTRO_Project.Components;
using SOTRO_Project.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5254/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IBuildingApiService, BuildingApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5254/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IRoomApiService, RoomApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5254/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<ITenantApiService, TenantApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5254/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IDashboardApiService, DashboardApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5254/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<IInvoiceApiService, InvoiceApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5254/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpClient<ISubscriptionApiService, SubscriptionApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5254/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpClient<IAdminAccountApiService, AdminAccountApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5254/";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
