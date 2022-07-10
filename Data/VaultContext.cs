using Microsoft.EntityFrameworkCore;
using VaultChanger.Models;

namespace VaultChanger.Data
{
    public class VaultContext : DbContext
    {
        public VaultContext(DbContextOptions<VaultContext> options)
            : base(options)
        {
        }
        
        public DbSet<Request> Requests {get; set;}
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Request>()
                .HasKey(request => new { request.VaultNamespace, request.MountPoint, request.Path, request.Key });
            
            modelBuilder
                .Entity<Request>()
                .Property(request => request.Type)
                .HasConversion<string>();
        }
    }
}