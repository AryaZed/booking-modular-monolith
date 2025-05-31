using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Data;

// Custom implementation of IHttpContextAccessor for design-time
internal class DesignTimeHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext HttpContext { get; set; } = null;
}

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
    public IdentityContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<IdentityContext>();

        builder.UseNpgsql("Server=localhost;Port=5432;Database=identity_db;User Id=postgres;Password=postgres;Include Error Detail=true");
        
        // Create a simple HttpContextAccessor for design-time
        var httpContextAccessor = new DesignTimeHttpContextAccessor();
        
        return new IdentityContext(builder.Options, httpContextAccessor);
    }
}
