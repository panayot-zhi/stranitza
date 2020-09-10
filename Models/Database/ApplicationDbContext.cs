using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using stranitza.Utility;

namespace stranitza.Models.Database
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
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
                    dbString => dbString.Separate<int>());

            builder.Entity<StranitzaIssue>()
                .Property(p => p.Tags)
                .HasConversion<string>(array => array.Join(),
                    dbString => dbString.Separate<string>());

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
        }

    }
}
