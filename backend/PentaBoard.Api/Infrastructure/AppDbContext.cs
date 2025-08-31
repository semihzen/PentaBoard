using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Domain;                 // User, UserInvite, Project, ProjectMember
using PentaBoard.Api.Domain.Entities;        // Board, BoardColumn, WorkItem

// İsim çakışmalarına karşı alias
using BoardEntity        = PentaBoard.Api.Domain.Entities.Board;
using BoardColumnEntity  = PentaBoard.Api.Domain.Entities.BoardColumn;
using WorkItemEntity     = PentaBoard.Api.Domain.Entities.WorkItem;
// ✅ Yeni: ProjectFile
using ProjectFileEntity  = PentaBoard.Api.Domain.Entities.ProjectFile;

namespace PentaBoard.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<UserInvite> UserInvites => Set<UserInvite>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    public DbSet<BoardEntity> Boards => Set<BoardEntity>();
    public DbSet<BoardColumnEntity> BoardColumns => Set<BoardColumnEntity>();

    // WorkItems
    public DbSet<WorkItemEntity> WorkItems => Set<WorkItemEntity>();

    // ✅ Yeni: ProjectFiles
    public DbSet<ProjectFileEntity> ProjectFiles => Set<ProjectFileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- User ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(50).IsRequired();
        });

        // --- UserInvite ---
        modelBuilder.Entity<UserInvite>(e =>
        {
            e.ToTable("UserInvites");

            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email);
            e.HasIndex(x => x.TokenHash).IsUnique();

            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasMaxLength(100).IsRequired();
            e.Property(x => x.TokenHash).HasMaxLength(64).IsRequired(); // sha256 b64 ~44
            e.Property(x => x.Note).HasMaxLength(500);

            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.Property(x => x.ExpiresAt).HasColumnType("datetime2");
            e.Property(x => x.AcceptedAt).HasColumnType("datetime2");
        });

        // --- Project ---
        modelBuilder.Entity<Project>(e =>
        {
            e.ToTable("Projects");

            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Key).IsUnique();

            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Key).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.Color).HasMaxLength(50).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");

            e.HasOne(x => x.ProjectAdmin)
                .WithMany()
                .HasForeignKey(x => x.ProjectAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- ProjectMember ---
        modelBuilder.Entity<ProjectMember>(e =>
        {
            e.ToTable("ProjectMembers");
            e.HasKey(x => x.Id);

            e.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();

            e.Property(x => x.SubRole)
                .HasMaxLength(100)
                .IsRequired()
                .HasDefaultValue("member");

            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            e.HasOne<Project>()
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Boards ---
        modelBuilder.Entity<BoardEntity>(e =>
        {
            e.ToTable("Boards");
            e.HasKey(x => x.Id);

            // Her projeye tek board (istersen kaldır)
            e.HasIndex(x => x.ProjectId).IsUnique();

            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            e.Property(x => x.SettingsJson).HasColumnType("nvarchar(max)");

            e.HasOne<Project>()
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<User>()
                .WithMany()
                .HasForeignKey("CreatedById")
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- BoardColumns (states) ---
        modelBuilder.Entity<BoardColumnEntity>(e =>
        {
            e.ToTable("BoardColumns");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Color).HasMaxLength(50);
            e.Property(x => x.IsDefault).HasDefaultValue(false);
            e.Property(x => x.IsDoneLike).HasDefaultValue(false);

            e.HasIndex(x => new { x.BoardId, x.OrderKey }).IsUnique();
            e.HasIndex(x => new { x.BoardId, x.Name }).IsUnique();

            e.HasOne<BoardEntity>()
                .WithMany(b => b.Columns)
                .HasForeignKey(x => x.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- WorkItems ---
        modelBuilder.Entity<WorkItemEntity>(e =>
        {
            e.ToTable("WorkItems");
            e.HasKey(x => x.Id);

            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.Property(x => x.Type).HasMaxLength(30).IsRequired().HasDefaultValue("Task");
            e.Property(x => x.Description).HasColumnType("nvarchar(max)");
            e.Property(x => x.OriginalEstimateHours).HasColumnType("decimal(6,2)");
            e.Property(x => x.RemainingHours).HasColumnType("decimal(6,2)");
            e.Property(x => x.CompletedHours).HasColumnType("decimal(6,2)");
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2");
            e.Property(x => x.TagsJson).HasColumnType("nvarchar(max)");

            e.HasIndex(x => x.ProjectId);
            e.HasIndex(x => x.BoardId);
            e.HasIndex(x => new { x.BoardColumnId, x.OrderKey });
            e.HasIndex(x => x.AssigneeId);

            e.HasOne<Project>()
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<BoardEntity>()
                .WithMany()
                .HasForeignKey(x => x.BoardId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<BoardColumnEntity>()
                .WithMany()
                .HasForeignKey(x => x.BoardColumnId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // --- ✅ ProjectFiles ---
        modelBuilder.Entity<ProjectFileEntity>(e =>
        {
            e.ToTable("ProjectFiles");
            e.HasKey(x => x.Id);

            e.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            e.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");

            e.HasIndex(x => new { x.ProjectId, x.CreatedAt });

            e.HasOne<Project>()         // -> Projects.Id (Guid)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<User>()            // -> Users.Id (Guid)
                .WithMany()
                .HasForeignKey(x => x.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
