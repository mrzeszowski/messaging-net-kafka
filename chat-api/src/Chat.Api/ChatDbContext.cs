using Microsoft.EntityFrameworkCore;

namespace Chat.Api;

internal class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("chat");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatDbContext).Assembly);
    }
}