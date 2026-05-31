using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRACIT.Data;

namespace MyRACIT.Tests.Integration;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real SQL Server DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<MyRacitDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add InMemory DbContext
                services.AddDbContext<MyRacitDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTestDb"));
            });
        });
    }

    [Fact]
    public async Task Get_LoginPage_Returns200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Auth/Login");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_StudentIndex_Unauthenticated_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Student/Index");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.Contains("Auth/Login", location ?? "");
    }

    [Fact]
    public async Task Get_AdminIndex_Unauthenticated_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Admin/Index");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.Contains("Auth/Login", location ?? "");
    }

    [Fact]
    public async Task Get_TeacherIndex_Unauthenticated_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Teacher/Index");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.Contains("Auth/Login", location ?? "");
    }

    [Fact]
    public async Task Get_HomePage_Returns200OrRedirect()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.OK ||
            response.StatusCode == System.Net.HttpStatusCode.Redirect ||
            response.StatusCode == System.Net.HttpStatusCode.Found);
    }
}
