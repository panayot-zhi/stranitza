using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using stranitza.Models.Database;
using stranitza.Models.Generic;
using stranitza.Models.ViewModels;
using stranitza.Utility;

namespace stranitza.Repositories
{
    public static class IndexRepository
    {
        public static async Task<IndexViewModel> GetSourcesPagedAsync(this DbSet<StranitzaSource> sourcesDbSet,
            int? year, int? categoryId, string origin, int? issueId,
            int? pageIndex, int pageSize = 10)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var query = sourcesDbSet.AsQueryable();

            if (issueId.HasValue)
            {
                query = query.Where(x => x.IssueId == issueId.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(x => x.ReleaseYear == year.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(origin))
            {
                if ("#".Equals(origin))
                {
                    // skip
                }
                else if ("„“".Equals(origin))   // special case
                {
                    query = query.Where(x => x.Origin.StartsWith("„"));
                    query = query.OrderBy(x => x.Origin);
                }
                else
                {
                    query = query.Where(x => x.Origin.StartsWith(origin));
                    query = query.OrderBy(x => x.Origin);
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.ReleaseYear).
                    ThenByDescending(x => x.ReleaseNumber)
                    .ThenBy(x => x.StartingPage);
            }

            var count = await query.CountAsync();
            var sources = query
                .Select(x => new SourceIndexViewModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    StartingPage = x.StartingPage,
                    ReleaseYear = x.ReleaseYear,
                    ReleaseNumber = x.ReleaseNumber,
                    Pages = x.Pages,
                    FirstName = x.FirstName,
                    LastName = x.LastName,

                    Origin = x.Origin,
                    Description = x.Description,
                    Notes = x.Notes,

                    IssueId = x.IssueId,
                    EPageId = x.EPageId,
                    AuthorId = x.AuthorId,

                    Uploader = x.Uploader,

                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,

                    DateCreated = x.DateCreated,
                    LastUpdated = x.LastUpdated

                }).Skip((pageIndex.Value - 1) * pageSize).Take(pageSize);

            return new IndexViewModel(count, pageIndex.Value, pageSize)
            {
                Records = await sources.ToListAsync()
            };
        }

        public static async Task<IndexViewModel> GetSourcesByAuthorPagedAsync(this DbSet<StranitzaSource> sourcesDbSet,
            string authorId, int? year, int? categoryId, int? pageIndex, int pageSize = 10)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var query = sourcesDbSet.Where(x => x.AuthorId == authorId).AsQueryable();

            if (year.HasValue)
            {
                query = query.Where(x => x.ReleaseYear == year.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId);
            }

            var count = await query.CountAsync();
            var sources = query
                .OrderByDescending(x => x.ReleaseYear)
                    .ThenByDescending(x => x.ReleaseNumber)
                .Select(x => new SourceIndexViewModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    StartingPage = x.StartingPage,
                    ReleaseYear = x.ReleaseYear,
                    ReleaseNumber = x.ReleaseNumber,
                    Pages = x.Pages,
                    FirstName = x.FirstName,
                    LastName = x.LastName,

                    Origin = x.Origin,
                    Description = x.Description,
                    Notes = x.Notes,

                    IssueId = x.IssueId,
                    EPageId = x.EPageId,
                    AuthorId = x.AuthorId,

                    Uploader = x.Uploader,

                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,

                    DateCreated = x.DateCreated,
                    LastUpdated = x.LastUpdated

                }).Skip((pageIndex.Value - 1) * pageSize).Take(pageSize);

            return new IndexViewModel(count, pageIndex.Value, pageSize)
            {
                Records = await sources.ToListAsync()
            };
        }

        public static async Task<CategorySourcesViewModel> GetCategorySourcesPagedAsync(this DbSet<StranitzaSource> dbSet,
            int categoryId, int? pageIndex, string sortPropertyName, SortOrder sortOrder, int pageSize = 10)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var query = dbSet                    
                .Where(x => x.CategoryId == categoryId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sortPropertyName))
            {
                query = query.OrderBy(sortPropertyName, sortOrder);
            }

            var count = await query.CountAsync();
            var sources = await query
                .Select(x => new SourceIndexViewModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    StartingPage = x.StartingPage,
                    ReleaseYear = x.ReleaseYear,
                    ReleaseNumber = x.ReleaseNumber,
                    Pages = x.Pages,
                    FirstName = x.FirstName,
                    LastName = x.LastName,

                    Origin = x.Origin,
                    Description = x.Description,
                    Notes = x.Notes,

                    IssueId = x.IssueId,
                    EPageId = x.EPageId,
                    AuthorId = x.AuthorId,

                    Uploader = x.Uploader,

                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,

                    DateCreated = x.DateCreated,
                    LastUpdated = x.LastUpdated

                }).Skip((pageIndex.Value - 1) * pageSize).Take(pageSize).ToListAsync();

            return new CategorySourcesViewModel(count, pageIndex.Value, pageSize)
            {
                Records = sources
            };
        }

        public static IEnumerable<CountByYears> GetSourcesCountByYears(this DbSet<CountByYears> dbSet)
        {
            return dbSet.FromSqlRaw($"CALL CountByReleaseYear('{CountQueryType.Sources}')").ToList();
        }

        public static IEnumerable<CountByYears> GetSourcesCountByYearsAndAuthor(this DbSet<CountByYears> dbSet, string authorId)
        {
            var parameters = new object[]
            {
                new MySqlParameter("authorId", authorId)
            };

            return dbSet.FromSqlRaw(Sql.GetCountByReleaseYearAndAuthor, parameters).ToList();
        }

        public static async Task<List<SourceDetailsViewModel>> GetSourcesByIssueAsync(this DbSet<StranitzaSource> sourcesDbSet, int issueId)
        {
            return await sourcesDbSet
                .Where(x => x.IssueId == issueId)
                .OrderBy(x => x.StartingPage)
                .Select(x => new SourceDetailsViewModel()
                {
                    Id = x.Id,
                    //ReleaseNumber = x.ReleaseNumber,
                    //ReleaseYear = x.ReleaseYear,
                    AuthorId = x.AuthorId,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    //Description = x.Description,
                    Title = x.Title,
                    Origin = x.Origin,
                    Pages = x.Pages,
                    StartingPage = x.StartingPage,

                    CategoryName = x.Category.Name,
                    IsTranslation = x.IsTranslation,

                    DateCreated = x.DateCreated,
                    LastUpdated = x.LastUpdated,

                }).ToListAsync();
        }


        public static async Task<SourceDetailsViewModel> GetSourceDetailsViewModelAsync(this DbSet<StranitzaSource> dbSet, int id)
        {
            return await dbSet.Select(x => new SourceDetailsViewModel()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,                
                Origin = x.Origin,
                Title = x.Title,
                Description = x.Description,
                Notes = x.Notes,
                ReleaseNumber = x.ReleaseNumber,
                ReleaseYear = x.ReleaseYear,
                Pages = x.Pages,

                StartingPage = x.StartingPage,
                IsTranslation = x.IsTranslation,
                Uploader = x.Uploader,
                CategoryName = x.Category.Name,

                CategoryId = x.CategoryId,
                IssueId = x.IssueId,
                EPageId = x.EPageId,
                AuthorId = x.AuthorId,
                AuthorUserName = x.AuthorId != null ? 
                    x.Author.UserName : null,

                LastUpdated = x.LastUpdated,
                DateCreated = x.DateCreated,

            }).SingleOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<SourceEditViewModel> GetSourceEditViewModelAsync(this DbSet<StranitzaSource> dbSet, int id)
        {
            return await dbSet.Select(x => new SourceEditViewModel()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Origin = x.Origin,
                Title = x.Title,
                Description = x.Description,
                Notes = x.Notes,
                ReleaseNumber = x.ReleaseNumber,
                ReleaseYear = x.ReleaseYear,
                Pages = x.Pages,

                StartingPage = x.StartingPage,
                IsTranslation = x.IsTranslation,
                Uploader = x.Uploader,

                CategoryId = x.CategoryId,
                IssueId = x.IssueId,
                AuthorId = x.AuthorId,

                LastUpdated = x.LastUpdated,
                DateCreated = x.DateCreated,

            }).SingleOrDefaultAsync(x => x.Id == id);
        }


        public static async Task<StranitzaSource> CreateSourceAsync(this DbSet<StranitzaSource> dbSet,
            SourceCreateViewModel vModel, string uploader)
        {
            var entry = new StranitzaSource()
            {
                FirstName = vModel.FirstName,
                LastName = vModel.LastName,
                Origin = vModel.Origin,
                Title = vModel.Title,
                Description = vModel.Description,
                Notes = vModel.Notes,
                ReleaseNumber = vModel.ReleaseNumber.Value,
                ReleaseYear = vModel.ReleaseYear.Value,
                Pages = vModel.Pages,
                StartingPage = vModel.StartingPage.Value,
                IsTranslation = vModel.IsTranslation,
                
                IssueId = vModel.IssueId,
                CategoryId = vModel.CategoryId.Value,

                Uploader = uploader
            };

            await dbSet.AddAsync(entry);

            return entry;
        }

        public static async Task<StranitzaSource> CreateSourceAsync(this DbSet<StranitzaSource> dbSet,
            StranitzaEPage epage, string uploader)
        {
            var entry = new StranitzaSource()
            {
                FirstName = epage.FirstName,
                LastName = epage.LastName,
                Origin = $"{epage.FirstName} {epage.LastName}",
                Title = epage.Title,
                Description = epage.Description,
                Notes = epage.Notes,
                ReleaseNumber = epage.ReleaseNumber,
                ReleaseYear = epage.ReleaseYear,
                IsTranslation = epage.IsTranslation,
                CategoryId = epage.CategoryId,

                //Pages = 
                StartingPage = 0,

                EPageId = epage.Id,
                AuthorId = epage.AuthorId,
                Uploader = uploader,
            };

            await dbSet.AddAsync(entry);

            return entry;
        }

        public static async Task<SourceSearchViewModel> SearchSourcesPagedAsync(this DbSet<StranitzaSource> dbSet,
            string searchQuery, int? pageIndex, int pageSize = 10)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var query = dbSet.Where(x =>
                    EF.Functions.Like(x.Title, $"%{searchQuery}%") ||
                    EF.Functions.Like(x.Origin, $"%{searchQuery}%") ||
                    EF.Functions.Like(x.Description, $"%{searchQuery}%") ||
                    EF.Functions.Like(x.Notes, $"%{searchQuery}%")
                    /*EF.Functions.Like(x.FirstName, $"%{searchQuery}%") || */
                    /*EF.Functions.Like(x.LastName, $"%{searchQuery}%")*/
            );

            var count = await query.CountAsync();
            var sources = query
                .OrderByDescending(x => x.ReleaseYear)
                    .ThenByDescending(x => x.ReleaseNumber)
                .Select(x => new SourceIndexViewModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    StartingPage = x.StartingPage,
                    ReleaseYear = x.ReleaseYear,
                    ReleaseNumber = x.ReleaseNumber,
                    Pages = x.Pages,
                    FirstName = x.FirstName,
                    LastName = x.LastName,

                    Origin = x.Origin,
                    Description = x.Description,
                    Notes = x.Notes,

                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    IssueId = x.IssueId,
                    EPageId = x.EPageId,

                }).Skip((pageIndex.Value - 1) * pageSize).Take(pageSize);

            return new SourceSearchViewModel(count, pageIndex.Value, pageSize)
            {
                Records = await sources.ToListAsync()
            };
        }

        // ReSharper disable ArrangeAccessorOwnerBody
        private static class Sql
        {
            public static string GetCountByReleaseYearAndAuthor
            {
                get
                {
                    return
@"SELECT 
    x.ReleaseYear as Year, 
    COUNT(*) as Count 
FROM 
    StranitzaSources x
WHERE 
    x.AuthorId = @authorId
GROUP BY 
    x.ReleaseYear
ORDER BY 
    x.ReleaseYear DESC;
";
                }
            }

        } // ReSharper restore ArrangeAccessorOwnerBody
    }
}