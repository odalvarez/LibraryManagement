using Asp.Versioning;
using FluentValidation;
using LibraryManagement.Api.Middleware;
using LibraryManagement.Api.Settings;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

// CONFIGURE SERILOG FROM APPSETTINGS
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// BOOK SETTINGS
var bookSettings = builder.Configuration.GetSection(BookSettings.SectionName).Get<BookSettings>() ?? new BookSettings();
builder.Services.Configure<BookServiceOptions>(options => options.MaxAllowed = bookSettings.MaxAllowed);

// DATABASE
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ILibraryDbContext>(sp => sp.GetRequiredService<LibraryDbContext>());

// SERVICES
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();

// FLUENT VALIDATION
builder.Services.AddValidatorsFromAssemblyContaining<AuthorService>();

// CONTROLLERS
builder.Services.AddControllers();

// API VERSIONING
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Library Management API", Version = "v1" });
    options.EnableAnnotations();
});


var app = builder.Build();

// GLOBAL EXCEPTION HANDLER
app.UseMiddleware<GlobalExceptionMiddleware>();

// SWAGGER UI
app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Management API v1"));

// STATIC FILES
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();

// AUTO-APPLY MIGRATIONS ON STARTUP
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.Migrate();
}

app.Run();
