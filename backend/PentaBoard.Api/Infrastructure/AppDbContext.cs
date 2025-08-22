using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Domain;
using PentaBoard.Api.Domain.Entities;

namespace PentaBoard.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserInvite> UserInvites => Set<UserInvite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- User ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.FirstName)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(u => u.LastName)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(u => u.Email)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(u => u.PasswordHash)
                  .IsRequired();

            entity.Property(u => u.Role)
                  .HasMaxLength(50)
                  .IsRequired();
        });

        // --- UserInvite ---
        modelBuilder.Entity<UserInvite>(e =>
        {
            e.ToTable("UserInvites");

            e.HasKey(x => x.Id);

            e.HasIndex(x => x.Email);
            e.HasIndex(x => x.TokenHash).IsUnique();

            e.Property(x => x.Email)
             .HasMaxLength(200)
             .IsRequired();

            e.Property(x => x.Role)
             .HasMaxLength(100)
             .IsRequired();

            // SHA-256 base64 = 44 karakter; 64 güvenli alan bırakıyoruz
            e.Property(x => x.TokenHash)
             .HasMaxLength(64)
             .IsRequired();

            e.Property(x => x.Note)
             .HasMaxLength(500);

            e.Property(x => x.CreatedAt)
             .HasColumnType("datetime2");

            e.Property(x => x.ExpiresAt)
             .HasColumnType("datetime2");

            e.Property(x => x.AcceptedAt)
             .HasColumnType("datetime2");
        });
    }
}
