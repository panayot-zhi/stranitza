﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Serilog;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Utility;

namespace stranitza.Services
{
    public class StatisticService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMemoryCache _cache;
        private static string _appVersion;

        public StatisticService(ApplicationDbContext dbContext, LinkGenerator linkGenerator,
            IHttpContextAccessor httpContextAccessor, IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;
            _dbContext = dbContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        public int GetPostsCount()
        {
            return _dbContext.StranitzaPosts.Count();
        }

        public int GetIssuesCount()
        {
            return _dbContext.StranitzaIssues.Count();
        }

        public int GetCommentsCount()
        {
            return _dbContext.StranitzaComments.Count();
        }

        public int GetEPagesCount()
        {
            return _dbContext.StranitzaEPages.Count();
        }

        public int GetUsersCount()
        {
            return _dbContext.Users.Count();
        }

        public IEnumerable<SuggestionsViewModel> GetIssuesSuggestions(int count = 5)
        {
            var parameters = new object[]
            {
                new MySqlParameter("count", count)
            };

            var issues = _dbContext.StranitzaIssues.FromSqlRaw(Sql.GetIssuesSuggestions, parameters).Select(x =>
                new StranitzaIssue()
                {
                    IssueNumber = x.IssueNumber,
                    ReleaseNumber = x.ReleaseNumber,
                    ReleaseYear = x.ReleaseYear

                }).ToList();
                
            return issues.Select(x => new SuggestionsViewModel()
            {
                // NOTE: Issue title should be resolved by a method!
                Content = $"<span class=\"issue-title\">Брой № {x.IssueNumber}, {x.ReleaseNumber} / {x.ReleaseYear}</span>",
                Href = _linkGenerator.GetPathByAction("Details", "Issues", new { id = x.Id },
                    PathString.Empty, FragmentString.Empty, new LinkOptions()
                        { LowercaseUrls = true })

            });
        }

        public IEnumerable<SuggestionsViewModel> GetSourcesSuggestions(int count = 5)
        {
            var parameters = new object[]
            {
                new MySqlParameter("count", count)
            };

            var sources = _dbContext.StranitzaSources.FromSqlRaw(Sql.GetSourcesSuggestions, parameters).Select(x =>
                new StranitzaSource()
                {
                    Title = x.Title,
                    Origin = x.Origin,
                    ReleaseYear = x.ReleaseYear,
                    EPageId = x.EPageId,
                    IssueId = x.IssueId,
                    Id = x.Id

                }).ToList();

            return sources.Select(x => new SuggestionsViewModel()
            {
                Content = GetSourceDisplayContent(x),
                Href = GetSourceHref(_linkGenerator, x)

            });
        }

        private static string GetSourceDisplayContent(StranitzaSource x)
        {
            return $"<span class=\"source-title\">{x.Title}</span><span class=\"source-origin\">{x.Origin}, {x.ReleaseYear}г.</span>";
        }

        private static string GetSourceHref(LinkGenerator linkGenerator, StranitzaSource x)
        {
            if (x.EPageId.HasValue)
            {
                return linkGenerator.GetPathByAction("Details", "EPages", new {id = x.EPageId},
                    PathString.Empty, FragmentString.Empty, new LinkOptions()
                        {LowercaseUrls = true});
            }

            if (x.IssueId.HasValue)
            {
                return linkGenerator.GetPathByAction("FindPage", "Sources", new {id = x.Id},
                    PathString.Empty, FragmentString.Empty, new LinkOptions()
                        {LowercaseUrls = true});
            }

            return linkGenerator.GetPathByAction("FindIssue", "Sources", new {id = x.Id},
                PathString.Empty, FragmentString.Empty, new LinkOptions()
                    {LowercaseUrls = true});
        }

        public IEnumerable<SuggestionsViewModel> GetOtherPostsSuggestions(int postId, int count = 5)
        {
            var parameters = new object[]
            {
                new MySqlParameter("count", count),
                new MySqlParameter("postId", postId),
            };

            var posts = _dbContext.StranitzaPosts.FromSqlRaw(Sql.GetPostsSuggestions, parameters).Select(x =>
                new StranitzaPost()
                {
                    Id = x.Id,
                    Title = x.Title,
                    DateCreated = x.DateCreated

                }).ToList();

            return posts.Select(x => new SuggestionsViewModel()
            {
                Content = $"<span class=\"post-title\">{x.Title}</span><span class=\"post-date\">{x.DateCreated:d MMMM yyyy}</span>",
                Href = _linkGenerator.GetPathByAction("Details", "Posts", new { id = x.Id },
                    PathString.Empty, FragmentString.Empty, new LinkOptions()
                        { LowercaseUrls = true })

            });
        }

        public IEnumerable<SuggestionsViewModel> GetPostsSuggestionsByOrigin(int postId, int count = 5)
        {
            var post = _dbContext.StranitzaPosts.Find(postId);
            if (post == null)
            {
                throw new StranitzaException($"Could not find a designated post with an id of '{postId}'.");
            }

            var parameters = new object[]
            {
                new MySqlParameter("count", count),
                new MySqlParameter("postId", post.Id),
                new MySqlParameter("origin", post.Origin)
            };

            var posts = _dbContext.StranitzaPosts.FromSqlRaw(Sql.PostsSuggestionsByOrigin, parameters).Select(x =>
                new StranitzaPost()
                {
                    Id = x.Id,
                    Title = x.Title,
                    DateCreated = x.DateCreated

                }).ToList();

            return posts.OrderByDescending(x => x.DateCreated).Select(x => new SuggestionsViewModel()
            {
                Content = $"<span class=\"post-title\">{x.Title}</span><span class=\"post-date\">{x.DateCreated:d MMMM yyyy}</span>",
                Href = _linkGenerator.GetPathByAction("Details", "Posts", new { id = x.Id },
                    PathString.Empty, FragmentString.Empty, new LinkOptions()
                        { LowercaseUrls = true })

            });
        }

        public IEnumerable<SuggestionsViewModel> GetRandomCommentsByAuthor(string authorId, int count = 5)
        {
            var parameters = new object[]
            {
                new MySqlParameter("count", count),
                new MySqlParameter("authorId", authorId),
            };

            var comments = _dbContext.StranitzaComments.FromSqlRaw(Sql.GetRandomCommentsByAuthor, parameters)
                .Include(x => x.EPage)
                .Include(x => x.Issue)
                .Include(x => x.Post)
                .Select(x => new StranitzaComment()
                {
                    Id = x.Id,
                    EPageId = x.EPageId,
                    IssueId = x.IssueId,
                    PostId = x.PostId,
                    AuthorId = x.AuthorId,
                    DateCreated = x.DateCreated,

                    EPage = !x.EPageId.HasValue ? null : new StranitzaEPage()
                    {
                        Title = x.EPage.Title
                    },

                    Issue = !x.IssueId.HasValue ? null : new StranitzaIssue()
                    {
                        IssueNumber = x.Issue.IssueNumber,
                        ReleaseNumber = x.Issue.ReleaseNumber,
                        ReleaseYear = x.Issue.ReleaseYear,
                    },

                    Post = !x.PostId.HasValue ? null : new StranitzaPost()
                    {
                        Title = x.Post.Title
                    },

                }).ToList();

            return comments.OrderByDescending(x => x.DateCreated).Select(x =>
                x.EPage != null
                    ? new SuggestionsViewModel() // epage comment
                    {
                        Content = $"<span class=\"post-title\">{x.EPage.Title}</span><span class=\"post-date\">{x.DateCreated:d MMMM yyyy}</span>",
                        Href = _linkGenerator.GetPathByAction("Details", "EPages", new { id = x.EPageId },
                            PathString.Empty, new FragmentString($"#comment-{x.Id}"), new LinkOptions()
                            { LowercaseUrls = true })

                    } : x.Post != null
                        ? new SuggestionsViewModel() // post comment
                        {
                            Content = $"<span class=\"post-title\">{x.Post.Title}</span><span class=\"post-date\">{x.DateCreated:d MMMM yyyy}</span>",
                            Href = _linkGenerator.GetPathByAction("Details", "Posts", new { id = x.PostId },
                                PathString.Empty, new FragmentString($"#comment-{x.Id}"), new LinkOptions()
                                { LowercaseUrls = true })

                        }
                        : new SuggestionsViewModel() // issue comment
                        {
                            Content = $"<span class=\"post-title\">{StranitzaExtensions.GetIssueTitle(x.Issue)}</span><span class=\"post-date\">{x.DateCreated:d MMMM yyyy}</span>",
                            Href = _linkGenerator.GetPathByAction("Details", "Issues", new { id = x.IssueId },
                                PathString.Empty, new FragmentString($"#comment-{x.Id}"), new LinkOptions()
                                { LowercaseUrls = true })

                        }
            ).ToList();
        }

        public IEnumerable<SuggestionsViewModel> GetEditorsPickSuggestions(int count = 5)
        {
            var parameters = new object[]
            {
                new MySqlParameter("count", count)
            };

            var posts = _dbContext.StranitzaPosts.FromSqlRaw(Sql.GetEditorsPickSuggestions, parameters).Select(x =>
                new StranitzaPost()
                {
                    Id = x.Id,
                    Title = x.Title,
                    DateCreated = x.DateCreated

                }).ToList();

            return posts.OrderByDescending(x => x.DateCreated).Select(x => new SuggestionsViewModel()
            {
                Content = $"<span class=\"post-title\">{x.Title}</span><span class=\"post-date\">{x.DateCreated:d MMMM yyyy}</span>",
                Href = _linkGenerator.GetPathByAction("Details", "Posts", new { id = x.Id },
                    PathString.Empty, FragmentString.Empty, new LinkOptions()
                        { LowercaseUrls = true })

            });
        }

        public IEnumerable<SuggestionsViewModel> GetEPagesSuggestionsByAuthor(int epageId, int count = 5)
        {
            var epage = _dbContext.StranitzaEPages.Find(epageId);
            if (epage == null)
            {
                throw new StranitzaException($"Could not find a designated e-page with an id of '{epageId}'.");
            }

            var authorId = epage.AuthorId;
            if (string.IsNullOrEmpty(authorId))
            {
                return Enumerable.Empty<SuggestionsViewModel>();
            }

            var parameters = new object[]
            {
                new MySqlParameter("count", count),
                new MySqlParameter("epageId", epage.Id),
                new MySqlParameter("authorId", epage.AuthorId)
            };

            var epages = _dbContext.StranitzaEPages.FromSqlRaw(Sql.GetEPagesSuggestionsByAuthor, parameters).Select(x =>
                new StranitzaEPage()
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    Title = x.Title,
                    AuthorId = x.AuthorId,
                    DateCreated = x.DateCreated

                }).ToList();

            return epages.OrderByDescending(x => x.DateCreated).Select(x => new SuggestionsViewModel()
            {
                Content = $"<span class=\"epage-title\">{x.Title}</span><span class=\"epage-category\">{x.Category.Name}</span><span class=\"epage-date\">{x.DateCreated:d MMMM yyyy}</span>",
                Href = _linkGenerator.GetPathByAction("Details", "EPages", new { id = x.Id }, 
                    PathString.Empty, FragmentString.Empty, new LinkOptions()
                    { LowercaseUrls = true })

            });
        }

        public string GetAppVersion()
        {
            if (_appVersion != null)
            {
                return _appVersion;
            }

            var assembly = GetType().Assembly;
            var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
            _appVersion = informationalVersion;
            return informationalVersion;
        }

        public async void UpdateIssueDownloadCountAsync(int issueId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userAgent = StranitzaExtensions.GetUserAgent(httpContext);
            var clientIp = StranitzaExtensions.GetIp(httpContext);
            var user = httpContext.User;

            IssueDownloadCountLogOrigin(issueId, user, clientIp, userAgent);

            await using (var dbContext = CreateDbContext())
            {
                var issue = await dbContext.StranitzaIssues.FindAsync(issueId);
                if (issue == null)
                {
                    Log.Logger.Error($"Update issue DownloadCount was called, but can't find issue by ID #{issueId}.");
                    return;
                }

                issue.DownloadCount ++;
                await dbContext.SaveChangesAsync();
            }

            Log.Logger.Debug($"Issue DownloadCount was updated for issue #{issueId}.");
        }

        private static void IssueDownloadCountLogOrigin(int issueId, ClaimsPrincipal user, string clientIp, string userAgent)
        {
            Log.Logger.Information($"Потребител ID={user.GetUserId()}, UserName='{user.GetUserName()}' " +
                                   $"с произход IP={clientIp}, UserAgent='{userAgent}' " +
                                   $"свали брой #{issueId}.");
        }

        /// <summary>
        /// Flush to DB every 5 minutes or if you hit 100 (individual) view hits.
        /// </summary>
        /// <param name="issueId">The id of the issue, cache key is view_count_issue_{issueId}</param>
        public void UpdateIssueViewCountAsync(int issueId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userAgent = StranitzaExtensions.GetUserAgent(httpContext);
            var clientIp = StranitzaExtensions.GetIp(httpContext);
            var user = httpContext.User;

            var key = $"view_count_issue_{issueId}";
            var origin = StranitzaExtensions.Md5Hash($"{clientIp} {userAgent}");

            // Look for cache key
            if (!_cache.TryGetValue(key, out string[] viewCountOrigins))
            {
                // Add new cache entry and be gone from here
                IssueViewCountLogOrigin(issueId, user, clientIp, userAgent);
                AddOrigin(key, origin, null);
                return;
            }

            // we have a cache entry...

            if (viewCountOrigins.Contains(origin))
            {
                // origin exists,
                // no double-counting
                return;
            }

            IssueViewCountLogOrigin(issueId, user, clientIp, userAgent);

            var viewCounts = viewCountOrigins.Length;
            if (viewCounts >= 99)
            {
                IssueViewCountFlush(issueId, ++viewCounts);
                _cache.Remove(key);
                return;
            }

            AddOrigin(key, origin, viewCountOrigins);
        }

        private static void IssueViewCountLogOrigin(int issueId, ClaimsPrincipal user, string clientIp, string userAgent)
        {
            Log.Logger.Information($"Потребител ID={user.GetUserId()}, UserName='{user.GetUserName()}' " +
                                   $"с произход IP={clientIp}, UserAgent='{userAgent}' " +
                                   $"прегледа брой #{issueId}.");
        }

        /// <summary>
        /// Flush to DB every 5 minutes or if you hit 100 (individual) view hits.
        /// </summary>
        /// <param name="postId">The id of the post, cache key is view_count_post_{postId}</param>
        public void UpdatePostViewCountAsync(int postId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userAgent = StranitzaExtensions.GetUserAgent(httpContext);
            var clientIp = StranitzaExtensions.GetIp(httpContext);

            var key = $"view_count_post_{postId}";
            var origin = StranitzaExtensions.Md5Hash($"{clientIp} {userAgent}");

            // Look for cache key
            if (!_cache.TryGetValue(key, out string[] viewCountOrigins))
            {
                // Add and be gone
                AddOrigin(key, origin, null);
                return;
            }

            // we have a cache entry...

            if (viewCountOrigins.Contains(origin))
            {
                // origin exists,
                // no double-counting
                return;
            }

            var viewCounts = viewCountOrigins.Length;
            if (viewCounts >= 99)
            {
                PostViewCountFlush(postId, ++viewCounts);
                _cache.Remove(key);
                return;
            }

            AddOrigin(key, origin, viewCountOrigins);
        }

        private void AddOrigin(string key, string origin, string[] viewCountOrigins)
        {
            // Key not in cache, create
            if (viewCountOrigins == null)
            {
                viewCountOrigins = new[] { origin };
            }
            else
            {
                var newSize = viewCountOrigins.Length + 1;
                Array.Resize(ref viewCountOrigins, newSize);
                viewCountOrigins[newSize - 1] = origin;
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .RegisterPostEvictionCallback(OnViewCountCacheItemExpired)
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); // NOTE: Ponder upon the value

            _cache.Set(key, viewCountOrigins, cacheEntryOptions);
        }

        private void OnViewCountCacheItemExpired(object key, object value, EvictionReason reason, object state)
        {
            if (reason != EvictionReason.Expired)
            {
                return;
            }

            var stringKey = (string) key;
            var valueAsStrings = (string[]) value;
            var viewCount = valueAsStrings.Length;
            if (stringKey.StartsWith("view_count_issue_"))
            {
                var issueIdString = stringKey.Replace("view_count_issue_", string.Empty);
                var issueId = int.Parse(issueIdString);

                IssueViewCountFlush(issueId, viewCount);
            } 
            else if (stringKey.StartsWith("view_count_post_"))
            {
                var postIdString = stringKey.Replace("view_count_post_", string.Empty);
                var postId = int.Parse(postIdString);

                PostViewCountFlush(postId, viewCount);
            }

            Log.Logger.Debug($"CacheItem with the key '{stringKey}' and values " +
                             $"[{string.Join(",", valueAsStrings)}] has expired and has been evicted from the cache registry.");
        }

        private async void PostViewCountFlush(int postId, int viewCount)
        {
            await using (var dbContext = CreateDbContext())
            {
                var post = await dbContext.StranitzaPosts.FindAsync(postId);
                if (post == null)
                {
                    Log.Logger.Error($"Update post ViewCount was called, but can't find post by ID #{postId}.");
                    return;
                }

                post.ViewCount += viewCount;
                await dbContext.SaveChangesAsync();
            }

            Log.Logger.Debug($"Post ViewCount was updated for post #{postId}.");
        }

        private async void IssueViewCountFlush(int issueId, int viewCount)
        {
            await using (var dbContext = CreateDbContext())
            {
                var issue = await dbContext.StranitzaIssues.FindAsync(issueId);
                if (issue == null)
                {
                    Log.Logger.Error($"Update issue ViewCount was called, but can't find issue by ID #{issueId}.");
                    return;
                }

                issue.ViewCount += viewCount;
                await dbContext.SaveChangesAsync();
            }

            Log.Logger.Debug($"Issue ViewCount was updated for issue #{issueId}.");
        }

        private ApplicationDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }

        // ReSharper disable ArrangeAccessorOwnerBody

        private static class Sql
        {
            public static string GetPostsSuggestions
            {
                get
                {
                    return
@"SELECT
    x.Id,
    x.Title,
    x.DateCreated
FROM
    StranitzaPosts x
WHERE
    x.Id <> @postId
ORDER BY
    rand()
LIMIT
    @count
";
                }
            }

            public static string GetRandomCommentsByAuthor
            {
                get
                {
                    return
@"SELECT 
    comment.Id,
       
    comment.EPageId,
    comment.IssueId,
    comment.PostId,
    comment.AuthorId,
    comment.DateCreated,

    issue.IssueNumber, 
    issue.ReleaseNumber, 
    issue.ReleaseYear,

    CASE
        WHEN comment.EPageId IS NOT NULL THEN epage.Title
        WHEN comment.PostId IS NOT NULL THEN post.Title        
    END

FROM StranitzaComments AS comment
    LEFT JOIN StranitzaEPages AS epage ON comment.EPageId = epage.Id
    LEFT JOIN StranitzaIssues AS issue ON comment.IssueId = issue.Id
    LEFT JOIN StranitzaPosts AS post ON comment.PostId = post.Id
WHERE
    comment.AuthorId =  @authorId
ORDER BY
    rand()
LIMIT
    @count
";
                }
            }

            public static string GetSourcesSuggestions
            {
                get
                {
                    return
@"SELECT
    x.Title,
    x.Origin,
    x.ReleaseYear,
    x.EpageId,
    x.IssueId,
    x.Id
FROM
    StranitzaSources x
ORDER BY
    rand()
LIMIT
    @count
";
                }
            }

            public static string PostsSuggestionsByOrigin
            {
                get
                {
                    return
@"SELECT
    x.Id,
    x.Title,
    x.DateCreated    
FROM
    StranitzaPosts x
WHERE
    x.Id <> @postId AND
    x.Origin LIKE '%@origin%'
ORDER BY
    rand()
LIMIT
    @count
";
                }
            }

            public static string GetEPagesSuggestionsByAuthor
            {
                get
                {
                    return
@"SELECT
    x.Id,
    x.CategoryId,
    x.Title,
    x.AuthorId,
    x.DateCreated
FROM
    StranitzaEPages x
WHERE
    x.Id <> @epageId AND
    x.AuthorId = @authorId
ORDER BY
    rand()
LIMIT
    @count
";
                }
            }

            public static string GetEditorsPickSuggestions
            {
                get
                {
                    return
@"SELECT
    x.Id,
    x.Title,
    x.DateCreated 
FROM
    StranitzaPosts x
WHERE    
    x.EditorsPick = 1
ORDER BY
    rand()
LIMIT
    @count
";
                }
            }

            public static string GetIssuesSuggestions
            {
                get
                {
                    return 
@"SELECT
    x.IssueNumber,
    x.ReleaseNumber,
    x.ReleaseYear
FROM
    StranitzaIssues x
WHERE
    x.IsAvailable = 1
ORDER BY
    rand()
LIMIT
    @count
";
                }
            }

        } // ReSharper restore ArrangeAccessorOwnerBody

    }
}
