using Blazored.SessionStorage;
using BlazorStoreManagementWebApp.Components;
using BlazorStoreManagementWebApp.Mappings;
using BlazorStoreManagementWebApp.Models;
using Microsoft.EntityFrameworkCore;
using StoreManagementBE.BackendServer.Infrastructure.DI;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 29))
    )
);

builder.Services.AddAutoMapper(typeof(MappingProfile));

// Dang ky cac service o ben DI (Dependency Injection)
builder.Services.AddApplicationServices();

builder.Services.AddHttpContextAccessor();




// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

QuestPDF.Settings.License = LicenseType.Community;

// them service session storage
builder.Services.AddBlazoredSessionStorage();
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
