using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Fundo.Applications.WebApi;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Fundo.Services.Tests.Integration;

public class LoansApiTests : IClassFixture<LoanApiFactory>, IAsyncLifetime
{
    private readonly LoanApiFactory _factory;
    private HttpClient _client = null!;
    private string _token = string.Empty;

    public LoansApiTests(LoanApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        // Register a test user and get token
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", new RegisterRequest
        {
            Username = $"testuser-{Guid.NewGuid():N}",
            Password = "TestPass123!"
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _token = auth!.Token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ReturnsEmptyForNewUser()
    {
        var response = await _client.GetAsync("/loans");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var loans = await response.Content.ReadFromJsonAsync<List<LoanResponse>>();
        loans.Should().NotBeNull();
        loans!.Should().BeEmpty();
    }

    [Fact]
    public async Task Post_CreateLoan_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
        {
            Amount = 2_500m,
            ApplicantName = "Integration Test",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<LoanResponse>();
        created.Should().NotBeNull();
        created!.Amount.Should().Be(2_500m);
        created.CurrentBalance.Should().Be(2_500m);
        created.Status.Should().Be("active");
    }

    [Fact]
    public async Task Post_CreateLoan_WithInvalidPayload_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
        {
            Amount = 0m,
            ApplicantName = "",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_NonExistentLoan_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/loans/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_Payment_DeductsBalanceAndReturnsLoan()
    {
        var create = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
        {
            Amount = 1_000m,
            ApplicantName = "Payer",
        });
        var created = await create.Content.ReadFromJsonAsync<LoanResponse>();
        created.Should().NotBeNull();

        var paymentResponse = await _client.PostAsJsonAsync(
            $"/loans/{created!.Id}/payment",
            new PaymentRequest { Amount = 400m });

        paymentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await paymentResponse.Content.ReadFromJsonAsync<LoanResponse>();
        updated!.CurrentBalance.Should().Be(600m);
        updated.Status.Should().Be("active");
    }

    [Fact]
    public async Task Post_Payment_WhenAmountExceedsBalance_ReturnsConflict()
    {
        var create = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
        {
            Amount = 100m,
            ApplicantName = "Conflict",
        });
        var created = await create.Content.ReadFromJsonAsync<LoanResponse>();

        var response = await _client.PostAsJsonAsync(
            $"/loans/{created!.Id}/payment",
            new PaymentRequest { Amount = 500m });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_Payment_WhenLoanMissing_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync(
            $"/loans/{Guid.NewGuid()}/payment",
            new PaymentRequest { Amount = 50m });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_Unauthorized_ReturnsUnauthorized()
    {
        var unauthClient = _factory.CreateClient();

        var response = await unauthClient.GetAsync("/loans");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class LoanApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"LoanDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("UseInMemoryDatabase", "true");
        builder.UseSetting("ConnectionStrings:LoanDb", string.Empty);
        builder.UseSetting("Jwt:Key", "IntegrationTestKey-MustBeAtLeast32CharactersLong!");
        builder.UseSetting("Jwt:Issuer", "LoanApi");
        builder.UseSetting("Jwt:Audience", "LoanApp");
        builder.UseSetting("Jwt:ExpiresInHours", "1");
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<LoanDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<LoanDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
