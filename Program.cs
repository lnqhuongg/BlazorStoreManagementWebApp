using Blazored.SessionStorage;
using BlazorStoreManagementWebApp.Components;
using BlazorStoreManagementWebApp.Mappings;
using BlazorStoreManagementWebApp.Models;
using Microsoft.EntityFrameworkCore;
using StoreManagementBE.BackendServer.Infrastructure.DI;
using QuestPDF.Infrastructure;
using BlazorStoreManagementWebApp.Models.Momo;
using BlazorStoreManagementWebApp.Services.Momo;

var builder = WebApplication.CreateBuilder(args);

// Ket noi MOMO
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

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

builder.Services.AddControllers(); // QUAN TRỌNG
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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

app.UseRouting();

app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
    