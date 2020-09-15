using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using stranitza.Models.Database;
using stranitza.Models.Database.Views;
using stranitza.Models.ViewModels;
using stranitza.Utility;

namespace stranitza.Repositories
{
    public static class LibraryRepository
    {
        public static async Task<LibraryViewModel> GetIssuesPagedAsync(this DbSet<StranitzaIssue> dbSet,
            int? year, int? pageIndex, int pageSize = 10, bool shouldBeAvailable = false)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var query = dbSet.AsQueryable();

            if (year.HasValue)
            {
                query = query.Where(x => x.ReleaseYear == year.Value);
            }

            if (shouldBeAvailable)
            {
                query = query.Where(x => x.IsAvailable);
            }

            var count = await query.CountAsync();
            var issues = query
                .Include(x => x.Pages)
                .OrderByDescending(x => x.IssueNumber)
                .Select(x => new IssueIndexViewModel()
                {
                    Id = x.Id,
                    ReleaseYear = x.ReleaseYear,
                    IssueNumber = x.IssueNumber,
                    ReleaseNumber = x.ReleaseNumber,
                    Description = x.Description,
                    DownloadCount = x.DownloadCount,
                    IsAvailable = x.IsAvailable,
                    PagesCount = x.PagesCount,
                    ImagesCount = x.Pages.Count,
                    HasPdf = x.PdfFilePreview != null,
                    ViewCount = x.ViewCount,
                    AvailablePages = x.AvailablePages,
                    Tags = x.Tags,

                    CoverPage = x.Pages.Select(y => new PageViewModel()
                    {
                        Id = y.Id,
                        IssueId = y.IssueId,
                        Type = y.Type,
                        PageNumber = y.PageNumber,
                        SlideNumber = y.SlideNumber,
                        IsAvailable = y.IsAvailable,
                        PageFileId = y.PageFileId,
                        DateCreated = y.DateCreated
                    }).FirstOrDefault(y => y.Type == StranitzaPageType.Cover),

                    IndexPage = x.Pages.Select(y => new PageViewModel()
                    {
                        Id = y.Id,
                        IssueId = y.IssueId,
                        Type = y.Type,
                        PageNumber = y.PageNumber,
                        SlideNumber = y.SlideNumber,
                        IsAvailable = y.IsAvailable,
                        PageFileId = y.PageFileId,
                        DateCreated = y.DateCreated
                    }).FirstOrDefault(y => y.Type == StranitzaPageType.Index),

                    LastUpdated = x.LastUpdated,
                    DateCreated = x.DateCreated,
                })
                .Skip((pageIndex.Value - 1) * pageSize).Take(pageSize).ToList();
                //.GroupBy(x => x.ReleaseYear).ToDictionary(x => x.Key, group => group.ToList());
                
            var issuesDictionary = issues.GroupBy(x => x.ReleaseYear).ToDictionary(x => x.Key, group => group.ToList());
            return new LibraryViewModel(count, pageIndex.Value, pageSize)
            {
                IssuesByYear = issuesDictionary
            };
        }

        public static async Task<IssuePagesViewModel> GetIssuePagesPagedAsync(this DbSet<StranitzaIssue> dbSet,
           int issueId, int? pageIndex, int pageSize = 10)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var issue = await dbSet
                .Include(x => x.Pages)
                    .ThenInclude(y => y.PageFile)
                .SingleAsync(x => x.Id == issueId);

            var count = issue.Pages.Count;

            var records = issue.Pages
                .OrderBy(x => x.SlideNumber)    // display here as it would be displayed in gallery
                .Select(y => new PageViewModel()
                {
                    Id = y.Id,
                    Type = y.Type,
                    IssueId = y.IssueId,
                    PageFileId = y.PageFileId,
                    PageNumber = y.PageNumber,
                    SlideNumber = y.SlideNumber,
                    IsAvailable = y.IsAvailable,
                }).Skip((pageIndex.Value - 1) * pageSize).Take(pageSize);

            return new IssuePagesViewModel(count, pageIndex.Value, pageSize)
            {
                Id = issue.Id,
                ReleaseYear = issue.ReleaseYear,
                ReleaseNumber = issue.ReleaseNumber,
                IssueNumber = issue.IssueNumber,

                Records = records
            };
        }

        public static IEnumerable<CountByYears> GetIssuesCountByYears(this DbSet<CountByYears> dbSet, bool onlyAvailable = false)
        {
            var queryType = CountQueryType.Issues;
            if (onlyAvailable)
            {
                queryType = CountQueryType.AvailableIssues;
            }
            return dbSet.FromSqlRaw($"CALL CountByReleaseYear('{queryType}')").ToList();
        }

        public static async Task<IssueDetailsViewModel> GetIssueDetailsAsync(this DbSet<StranitzaIssue> dbSet, int id)
        {
            var issue = await dbSet
                .Include(s => s.Pages)
                .Include(s => s.Comments)
                .Include(s => s.Sources)
                    .ThenInclude(s => s.Category)
                .SingleOrDefaultAsync(m => m.Id == id);

            var viewModel = new IssueDetailsViewModel()
            {
                Id = issue.Id,
                ReleaseYear = issue.ReleaseYear,
                IssueNumber = issue.IssueNumber,
                ReleaseNumber = issue.ReleaseNumber,
                Description = issue.Description,
                IsAvailable = issue.IsAvailable,
                PagesCount = issue.PagesCount,
                AvailablePages = issue.AvailablePages,

                PdfFilePreviewId = issue.PdfFilePreviewId,
                PdfFileDownloadId = issue.PdfFileReducedId,

                ViewCount = issue.ViewCount,
                DownloadCount = issue.DownloadCount,

                LastUpdated = issue.LastUpdated,
                DateCreated = issue.DateCreated,

                Tags = issue.Tags,

                Pages = issue.Pages
                    .Where(y => y.IsAvailable)
                    .OrderBy(y => y.SlideNumber)
                    .Select(y => new PageViewModel()
                    {
                        Id = y.Id,
                        IssueId = y.IssueId,
                        PageNumber = y.PageNumber,
                        SlideNumber = y.SlideNumber,
                        DateCreated = y.DateCreated,
                        Type = y.Type,
                    }).ToList(),

                CommentsCount = issue.Comments.Count,
                Sources = issue.Sources.OrderBy(x => x.StartingPage)
                    .Select(x => new SourceDetailsViewModel()
                    {
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
                    }).ToList(),
            };

            return viewModel;
        }

        public static async Task<PageViewModel> GetPageDetailsAsync(this DbSet<StranitzaPage> dbSet, int id)
        {
            return await dbSet
                .Include(x => x.Issue)
                .Include(x => x.PageFile)
                .Select(y => new PageViewModel()
                {
                    Id = y.Id,
                    Type = y.Type,
                    IssueId = y.IssueId,
                    PageFileId = y.PageFileId,
                    PageNumber = y.PageNumber,
                    SlideNumber = y.SlideNumber,
                    IsAvailable = y.IsAvailable,

                    IssueNumber = y.Issue.IssueNumber,
                    ReleaseNumber = y.Issue.ReleaseNumber,
                    ReleaseYear = y.Issue.ReleaseYear,

                    FileName = y.PageFile.FileName,
                    MimeType = y.PageFile.MimeType,
                    FileTitle = y.PageFile.Title,
                    FileExtension = y.PageFile.Extension,
                    FilePath = y.PageFile.FilePath,
                    ThumbPath = y.PageFile.ThumbPath,
                    FileLastUpdated = y.PageFile.LastUpdated,
                    FileDateCreated = y.PageFile.DateCreated,
                    LastUpdated = y.LastUpdated,
                    DateCreated = y.DateCreated,
                }).SingleOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<IssueEditViewModel> GetIssueEditAsync(this DbSet<StranitzaIssue> dbSet, int id)
        {
            var model = await dbSet
                //.Include(x => x.Pages)
                //.Include(x => x.PdfFile)
                .Select(x => new IssueEditViewModel()
                {
                    Id = x.Id,
                    Description = x.Description,
                    AvailablePages = x.AvailablePages,
                    IsAvailable = x.IsAvailable,
                    IssueNumber = x.IssueNumber,
                    ReleaseNumber = x.ReleaseNumber,
                    ReleaseYear = x.ReleaseYear,

                    ImagePagesCount = x.Pages.Count,
                    PdfPagesCount = x.PdfFilePreviewId.HasValue ? 
                        x.PagesCount : 0,

                    Tags = x.Tags.Join(),

                    PdfFileDownloadId = x.PdfFileReducedId,
                    PdfFilePreviewId = x.PdfFilePreviewId,
                    PdfFileName = x.PdfFilePreviewId.HasValue ?
                        x.PdfFilePreview.FileName : null,

                    ZipFileId = x.ZipFileId,
                    ZipFileName = x.ZipFileId.HasValue ?
                        x.ZipFile.FileName : null,

                    CoverPage = x.Pages.Select(y => new PageViewModel()
                    {
                        Id = y.Id,
                        IssueId = y.IssueId,
                        Type = y.Type,
                        PageNumber = y.PageNumber,
                        SlideNumber = y.SlideNumber,
                        IsAvailable = y.IsAvailable,
                        PageFileId = y.PageFileId,
                        DateCreated = y.DateCreated
                    }).FirstOrDefault(y => y.Type == StranitzaPageType.Cover),

                    IndexPage = x.Pages.Select(y => new PageViewModel()
                    {
                        Id = y.Id,
                        IssueId = y.IssueId,
                        Type = y.Type,
                        PageNumber = y.PageNumber,
                        SlideNumber = y.SlideNumber,
                        IsAvailable = y.IsAvailable,
                        PageFileId = y.PageFileId,
                        DateCreated = y.DateCreated
                    }).FirstOrDefault(y => y.Type == StranitzaPageType.Index),

                    CommentsCount = x.Comments.Count,
                    SourcesCount = x.Sources.Count,

                    DateCreated = x.DateCreated,
                    LastUpdated = x.LastUpdated,

                }).SingleOrDefaultAsync(x => x.Id == id);

            //model.CoverPageId = model.CoverPage.Id;
            //model.CoverPageNumber = model.CoverPage.PageNumber;
            //model.IndexPageId = model.IndexPage.Id;
            //model.IndexPageNumber = model.IndexPage.PageNumber;

            return model;
        }

        public static async Task<StranitzaIssue> CreateIssueAsync(this DbSet<StranitzaIssue> dbSet, IssueCreateViewModel vModel)
        {
            var entry = new StranitzaIssue()
            {
                IssueNumber = vModel.IssueNumber.Value,
                ReleaseNumber = vModel.ReleaseNumber.Value,
                ReleaseYear = vModel.ReleaseYear.Value,
                Description = vModel.Description,
                IsAvailable = vModel.IsAvailable,
                PagesCount = vModel.PageFiles.Count
            };

            await dbSet.AddAsync(entry);

            return entry;
        }

        public static async Task<StranitzaIssue> UpdateIssueAsync(this DbSet<StranitzaIssue> dbSet, IssueEditViewModel vModel)
        {
            // retrieve the
            // whole model again
            var entry = await dbSet
                .Include(x => x.Pages)
                //.Include(x => x.PdfFilePreview)
                .SingleAsync(x => x.Id == vModel.Id);

            dbSet.Attach(entry);

            // TODO: Cover Pages?
            /*var coverPage = entry.CoverPage;
            if (coverPage.PageNumber != vModel.CoverPageNumber)
            {
                var newCoverPage = entry.Pages.SingleOrDefault(x => x.PageNumber == vModel.CoverPageNumber);
                if (newCoverPage == null)
                {
                    throw new StranitzaException($"Няма страница с указаният номер '{vModel.CoverPageNumber}' (CoverPageNumber).");
                }

                if (newCoverPage.Type != StranitzaPageType.Regular)
                {
                    throw new StranitzaException($"Указаната страница с номер '{vModel.CoverPageNumber}' не може да бъде сменена защото понастоящем е {newCoverPage.Type} (CoverPageNumber).");
                }
            }

            // TODO: Index Pages?
            var indexPage = entry.IndexPage;
            if (indexPage.PageNumber != vModel.IndexPageNumber)
            {
                var newIndexPage = entry.Pages.SingleOrDefault(x => x.PageNumber == vModel.IndexPageNumber);
                if (newIndexPage == null)
                {
                    throw new StranitzaException($"Няма страница с указаният номер '{vModel.IndexPageNumber}' (IndexPageNumber).");
                }

                if (newIndexPage.Type != StranitzaPageType.Regular)
                {
                    throw new StranitzaException($"Указаната страница с номер '{vModel.IndexPageNumber}' не може да бъде сменена защото понастоящем е {newIndexPage.Type} (CoverPageNumber).");
                }
            }*/

            entry.Description = vModel.Description;
            entry.IsAvailable = vModel.IsAvailable;

            if (vModel.AvailablePages?.SequenceEqual(entry.AvailablePages) == false)
            {
                vModel.AvailablePagesChanged = true;
            }

            entry.AvailablePages = vModel.AvailablePages;
            entry.Tags = vModel.Tags?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            return entry;
        }

        public static async Task<StranitzaPage> UpdatePageAsync(this DbSet<StranitzaPage> dbSet, PageViewModel vModel)
        {
            // retrieve the
            // whole model again
            var entry = await dbSet
                .Include(x => x.Issue)
                .Include(x => x.PageFile)
                .SingleAsync(x => x.Id == vModel.Id);

            dbSet.Attach(entry);

            // let it fail,
            // modelstate failed us
            // something is wrong with the world
            entry.Type = vModel.Type.Value;
            entry.PageNumber = vModel.PageNumber.Value;
            entry.IsAvailable = vModel.IsAvailable;
            entry.SlideNumber = vModel.SlideNumber;

            // fill props to update StranitzaFile
            vModel.PageFileId = entry.PageFileId;

            var file = entry.PageFile;

            vModel.FilePath = file.FilePath;
            vModel.ThumbPath = file.ThumbPath;
            vModel.FileName = file.FileName;
            vModel.FileTitle = file.Title;

            // useless
            return entry;
        }

        public static async Task<StranitzaFile> UpdateFileAsync(this DbSet<StranitzaFile> dbSet, PageViewModel vModel)
        {
            // retrieve the
            // whole model again
            /*var entry = await dbSet
                .SingleAsync(x => x.Id == vModel.PageFileId);*/
            var entry = await dbSet.FindAsync(vModel.PageFileId);

            entry.FilePath = vModel.FilePath;
            entry.ThumbPath = vModel.ThumbPath;
            entry.FileName = vModel.FileName;
            entry.Title = vModel.FileTitle;

            return entry;
        }
    }
}