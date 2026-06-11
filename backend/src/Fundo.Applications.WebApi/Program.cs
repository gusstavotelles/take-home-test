using FluentValidation;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Middleware;
using Fundo.Applications.WebApi.Services;
using Fundo.Applications.WebApi.Validators;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Fundo.Applications.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        ConfigureServices(builder);

        var app = builder.Build();
        Configure(app);

        InitializeDatabase(app);

        app.Run();
    }

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");
        var connectionString = configuration.GetConnectionString("LoanDb");

        if (useInMemory || string.IsNullOrWhiteSpace(connectionString))
        {
            builder.Services.AddDbContext<LoanDbContext>(options =>
                options.UseInMemoryDatabase("LoanDb"));
        }
        else
        {
            builder.Services.AddDbContext<LoanDbContext>(options =>
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        }

        builder.Services.AddScoped<ILoanService, LoanService>();
        builder.Services.AddScoped<IValidator<CreateLoanRequest>, CreateLoanRequestValidator>();
        builder.Services.AddScoped<IValidator<PaymentRequest>, PaymentRequestValidator>();

        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:4200" };

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(corsOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });
    }

    public static void Configure(WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors();
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllers();
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
    }

    private static void InitializeDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LoanDbContext>();

        if (context.Database.IsRelational())
        {
            context.Database.EnsureCreated();
        }
        else
        {
            context.Database.EnsureCreated();
        }

        LoanDbSeeder.SeedAsync(context).GetAwaiter().GetResult();
    }
}
