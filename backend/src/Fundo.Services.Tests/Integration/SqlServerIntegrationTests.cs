using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Fundo.Applications.WebApi;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace Fundo.Services.Tests.Integration;

public class SqlServerIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string _token = string.Empty;

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("UseInMemoryDatabase", "false");
                builder.UseSetting("ConnectionStrings:LoanDb", _sqlContainer.GetConnectionString());
                builder.UseSetting("Jwt:Key", "TestcontainersKey-MustBeAtLeast32CharactersLong!");
                builder.UseSetting("Jwt:Issuer", "LoanApi");
                builder.UseSetting("Jwt:Audience", "LoanApp");
                builder.UseSetting("Jwt:ExpiresInHours", "1");
                builder.UseEnvironment("Production");
            });

        _client = _factory.CreateClient();

        // Register and authenticate
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", new RegisterRequest
        {
            Username = "sqltest-user",
            Password = "SqlTest123!"
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _token = auth!.Token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _sqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task CreateAndRetrieveLoan_WithRealSqlServer()
    {
        // Create a loan
        var createResponse = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
        {
            Amount = 5_000m,
            ApplicantName = "SQL Server Test",
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<LoanResponse>();
        created.Should().NotBeNull();
        created!.Amount.Should().Be(5_000m);
        created.CurrentBalance.Should().Be(5_000m);
        created.Status.Should().Be("active");

        // Retrieve by id
        var getResponse = await _client.GetAsync($"/loans/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<LoanResponse>();
        fetched!.Id.Should().Be(created.Id);
        fetched.ApplicantName.Should().Be("SQL Server Test");
    }

    [Fact]
    public async Task PaymentClosesLoan_WithRealSqlServer()
    {
        // Create a loan with small balance
        var createResponse = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
        {
            Amount = 200m,
            ApplicantName = "Close Test",
        });
        var created = await createResponse.Content.ReadFromJsonAsync<LoanResponse>();

        // Pay it all
        var payResponse = await _client.PostAsJsonAsync(
            $"/loans/{created!.Id}/payment",
            new PaymentRequest { Amount = 200m });

        payResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var paid = await payResponse.Content.ReadFromJsonAsync<LoanResponse>();
        paid!.CurrentBalance.Should().Be(0m);
        paid.Status.Should().Be("paid");
    }

    [Fact]
    public async Task MigrationsApplyCorrectly_TablesExist()
    {
        // The fact that we can create and query loans proves migrations ran
        var response = await _client.GetAsync("/loans");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
