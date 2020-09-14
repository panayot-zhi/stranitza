using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using stranitza.Models.Database.Views;
using stranitza.Utility;

namespace stranitza.Models.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public const string AutoUpdateProperty = "LastUpdated";
        public const string AutoCreateProperty = "DateCreated";

        public DbSet<StranitzaCategory> StranitzaCategories { get; set; }

        public DbSet<StranitzaComment> StranitzaComments { get; set; }

        public DbSet<StranitzaEPage> StranitzaEPages { get; set; }

        public DbSet<StranitzaFile> StranitzaFiles { get; set; }

        public DbSet<StranitzaIssue> StranitzaIssues { get; set; }

        public DbSet<StranitzaPage> StranitzaPages { get; set; }

        public DbSet<StranitzaPost> StranitzaPosts { get; set; }

        public DbSet<StranitzaSource> StranitzaSources { get; set; }

        // notMapped, views & generic

        public DbSet<CountByYears> CountByYears { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            TrackCreatedEntities();
            TrackUpdatedEntities();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected void TrackUpdatedEntities()
        {
            var editedEntities = ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified).ToList();

            editedEntities.ForEach(entry =>
            {
                if (entry.HasProperty(AutoUpdateProperty))
                {
                    entry.Property(AutoUpdateProperty).CurrentValue = DateTime.Now;
                }

                if (entry.HasProperty(AutoCreateProperty))
                {
                    entry.Property(AutoCreateProperty).IsModified = false;
                }
            });
        }

        protected void TrackCreatedEntities()
        {
            var addedEntities = ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added).ToList();

            var now = DateTime.Now;
            addedEntities.ForEach(entry =>
            {
                if (entry.HasProperty(AutoCreateProperty))
                {
                    entry.Property(AutoCreateProperty).CurrentValue = now;
                }

                if (entry.HasProperty(AutoUpdateProperty))
                {
                    entry.Property(AutoUpdateProperty).CurrentValue = now;
                }
            });
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // comments
            builder.Entity<StranitzaComment>()
                .HasOne(a => a.Parent)
                .WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StranitzaComment>()
                .HasOne(a => a.Author)
                .WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StranitzaComment>()
                .HasOne(a => a.Issue)
                .WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StranitzaComment>()
                .HasOne(a => a.Post)
                .WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StranitzaComment>()
                .HasOne(a => a.EPage)
                .WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade);

            // epages
            builder.Entity<StranitzaEPage>()
                .HasOne(a => a.Author)
                .WithMany(p => p.AuthoredEPages)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<StranitzaEPage>()
                .HasOne(a => a.Uploader)
                .WithMany(p => p.UploadedEPages)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StranitzaEPage>()
                .HasOne(a => a.Source)
                .WithOne(p => p.EPage)
                .OnDelete(DeleteBehavior.Cascade);

            // issues
            builder.Entity<StranitzaIssue>()
                .HasIndex(e => e.IssueNumber).IsUnique();

            builder.Entity<StranitzaIssue>()
                .Property(p => p.AvailablePages)
                .HasConversion<string>(array => array.Join(),
                    dbString => dbString.Separate<int>())
                .Metadata.SetValueComparer(IntegerArrayValueComparer);

            builder.Entity<StranitzaIssue>()
                .Property(p => p.Tags)
                .HasConversion<string>(array => array.Join(),
                    dbString => dbString.Separate<string>())
                .Metadata.SetValueComparer(StringArrayValueComparer);

            builder.Entity<StranitzaIssue>()
                .HasIndex(index => new { index.ReleaseYear, index.ReleaseNumber }).IsUnique();

            builder.Entity<StranitzaIssue>()
                .HasOne(p => p.ZipFile)
                .WithOne().OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StranitzaIssue>()
                .HasOne(p => p.PdfFileReduced)
                .WithOne().OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StranitzaIssue>()
                .HasOne(p => p.PdfFilePreview)
                .WithOne().OnDelete(DeleteBehavior.Cascade);

            // page
            builder.Entity<StranitzaPage>()
                .HasIndex(x => x.IssueId);

            builder.Entity<StranitzaPage>()
                .HasIndex(index => new { index.IssueId, index.PageNumber }).IsUnique();

            // posts
            builder.Entity<StranitzaPost>()
                .HasOne(a => a.Uploader)
                .WithMany(p => p.Posts)
                .OnDelete(DeleteBehavior.Restrict);

            // sources
            builder.Entity<StranitzaSource>()
                .HasOne(a => a.Author)
                .WithMany(p => p.Sources)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<StranitzaSource>()
                .HasOne(a => a.Issue)
                .WithMany(p => p.Sources)
                .OnDelete(DeleteBehavior.SetNull);

            // generic & views

            builder.Entity<CountByYears>(x =>
            {
                x.HasNoKey();
                x.ToView("Stupid .NET EF Core");
            });
        }

        private static readonly ValueComparer IntegerArrayValueComparer = new ValueComparer<int[]>(
            (i1, i2) => i1.SequenceEqual(i2),
            ints => ints.Aggregate(0, (accumulator, value) => HashCode.Combine(accumulator, value.GetHashCode())),
            ints => ints.ToArray());

        private static readonly ValueComparer StringArrayValueComparer = new ValueComparer<string[]>(
            (s1, s2) => s1.SequenceEqual(s2),
            strings => strings.Aggregate(0, (accumulator, value) => HashCode.Combine(accumulator, value.GetHashCode())),
            strings => strings.ToArray());

    }
}
