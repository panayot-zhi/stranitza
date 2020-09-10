using Microsoft.EntityFrameworkCore;
using stranitza.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace stranitza.Models.Database.Repositories
{
    public static class NewsRepository
    {
        public static async Task<NewsViewModel> GetPostsPagedAsync(this DbSet<StranitzaPost> postsDbSet,
            int? pageIndex, int pageSize = 4)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var query = postsDbSet.AsQueryable();

            var count = await query.CountAsync();
            var posts = query
                .Include(x => x.Uploader)
                .Include(x => x.ImageFile)
                .Select(x => new PostIndexViewModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    Origin = x.Origin,
                    DateCreated = x.DateCreated,
                    Description = x.Description,
                    CommentsCount = x.Comments.Count,
                    ViewCount = x.ViewCount,
                    EditorsPick = x.EditorsPick,
                    LastUpdated = x.LastUpdated,
                    /*UploaderNames = x.Uploader.Names,*/

                    ImageFileName = x.ImageFileId.HasValue ?
                        $"{x.ImageFile.FileName}.{x.ImageFile.Extension}" : null,
                    ImageTitle = x.ImageFileId.HasValue ?
                        $"{x.ImageFile.Title}" : null,
                })
                .OrderByDescending(x => x.DateCreated)
                .Skip((pageIndex.Value - 1) * pageSize).Take(pageSize);

            return new NewsViewModel(count, pageIndex.Value, pageSize)
            {
                Records = await posts.ToListAsync()
            };
        }

        public static async Task<PostDetailsViewModel> GetPostDetailsAsync(this DbSet<StranitzaPost> dbSet, int id)
        {
            return await dbSet.Select(x => new PostDetailsViewModel()
            {
                Id = x.Id,
                Title = x.Title,
                Origin = x.Origin,
                Uploader = x.Uploader.UserName,
                Content = x.Content,
                EditorsPick = x.EditorsPick,
                ViewCount = x.ViewCount,
                Description = x.Description,
                ImageFileName = x.ImageFileId.HasValue ?
                    $"{x.ImageFile.FileName}.{x.ImageFile.Extension}" : null,

                CommentsCount = x.Comments.Count,

                DateCreated = x.DateCreated,
                LastUpdated = x.LastUpdated,

            }).SingleOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<PostEditViewModel> GetPostEditAsync(this DbSet<StranitzaPost> dbSet, int id)
        {
            return await dbSet.Include(x => x.ImageFile).Select(x => new PostEditViewModel()
            {
                Id = x.Id,
                Title = x.Title,
                Origin = x.Origin,
                Uploader = x.Uploader.UserName,
                Content = x.Content,
                EditorsPick = x.EditorsPick,

                ImageFileId = x.ImageFileId,

                ImageFileName = x.ImageFileId.HasValue ?
                    $"{x.ImageFile.FileName}.{x.ImageFile.Extension}" : null,
                ImageTitle = x.ImageFileId.HasValue ?
                    $"{x.ImageFile.Title}" : null,

                CommentsCount = x.Comments.Count,

                DateCreated = x.DateCreated,
                LastUpdated = x.LastUpdated,

                Description = x.Description
            }).SingleOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<StranitzaPost> CreatePostAsync(this DbSet<StranitzaPost> dbSet, PostCreateViewModel vModel, string uploaderId)
        {
            var entry = new StranitzaPost()
            {
                UploaderId = uploaderId,
                Content = vModel.Content,
                Origin = vModel.Origin,
                Description = vModel.Description,
                Title = vModel.Title,
                ImageFileId = vModel.ImageFileId
            };

            await dbSet.AddAsync(entry);

            return entry;
        }

        public static async Task<StranitzaPost> UpdatePostAsync(this DbSet<StranitzaPost> dbSet, PostEditViewModel vModel)
        {
            var entry = await dbSet.FindAsync(vModel.Id);

            dbSet.Attach(entry);

            entry.Title = vModel.Title;
            entry.Origin = vModel.Origin;
            entry.EditorsPick = vModel.EditorsPick;
            entry.ImageFileId = vModel.ImageFileId;
            entry.Description = vModel.Description;
            entry.Content = vModel.Content;

            return entry;
        }
    }
}