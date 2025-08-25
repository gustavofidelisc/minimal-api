using minimal_api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using minimal_api.Infraestrutura.Interfaces;
using Test.Mocks;

namespace Test.Helpers
{
    public class Setup
    {
        public const string PORT = "5001";
        public static TestContext TestContext { get; set; } = default!;
        public static WebApplicationFactory<Startup> http { get; set; } = default!;
        public static HttpClient Client { get; set; } = default!;

        public static void ClassInit(TestContext testContext)
        {
            TestContext = testContext;
            http = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseSetting("https_port", PORT)
                        .UseEnvironment("Testing");

                    builder.ConfigureServices(services =>
                    {
                        services.AddScoped<IAdminstradorServico, AdministradorServicoMock>();
                    });
                }
            );
            Client = http.CreateClient();
        }
        
        public static void ClassCleanup()
        {
            Client.Dispose();
        }
    }

}
