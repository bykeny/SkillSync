using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SkillSync.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use a dedicated environment so Program.cs can
        // switch to the in-memory database configuration.
        builder.UseEnvironment("Testing");
    }
}