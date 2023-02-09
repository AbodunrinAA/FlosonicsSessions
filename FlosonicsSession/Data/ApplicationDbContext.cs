using FlosonicsSession.Models;
using Microsoft.EntityFrameworkCore;

namespace FlosonicsSession.Data;

public class ApplicationDbContext: DbContext
{
    public DbSet<Session> Sessions { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Session>()
            .HasIndex(s => s.ETag)
            .IsUnique();
        
        modelBuilder.Entity<Session>()
            .Property(s => s.ETag)
            .HasAnnotation("IsReadOnly", true);
        
        modelBuilder.Entity<Session>()
            .Property(s => s.DateAdded)
            .HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .HasAnnotation("IsReadOnly", true);

        modelBuilder.Entity<Session>()
            .HasIndex(s => s.Duration);
        
        modelBuilder.Entity<Session>()
            .Property(s => s.Duration)
            .HasMaxLength(1)
            .IsRequired();

        modelBuilder.Entity<Session>()
            .HasIndex(s => s.Name)
            .IsUnique();
        
        modelBuilder.Entity<Session>()
            .Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(50);

        /*
          modelBuilder.Entity<Session>()
            .Property(s => s.Tags)
            .IsRequired();.HasConversion(
            v => string.Join(",", v),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));*/


    }
}