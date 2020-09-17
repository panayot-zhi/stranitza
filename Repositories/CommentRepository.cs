using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;

namespace stranitza.Repositories
{
    public static class CommentRepository
    {
        public static async Task<CommentsWrapViewModel> GetCommentsWrappedAsync(this DbSet<StranitzaComment> dbSet,
            int? postId, int? issueId, int? epageId, int? parentId, int limit, int offset)
        {
            var query = dbSet.Where(x => x.PostId == postId && x.ParentId == parentId && x.IssueId == issueId && x.EPageId == epageId).AsQueryable();

            var count = await query.CountAsync();
            var records = await query
                .OrderByDescending(x => x.DateCreated)
                .Select(x => new CommentViewModel()
                {
                    Id = x.Id,
                    IssueId = x.IssueId,
                    PostId = x.PostId,
                    EPageId = x.EPageId,
                    ParentId = x.ParentId,
                    UploaderId = x.AuthorId,
                    ModeratorId = x.ModeratorId,

                    Content = x.Content,
                    Note = x.Note,

                    LastUpdated = x.LastUpdated,
                    DateCreated = x.DateCreated,

                }).Skip(offset).Take(limit).ToListAsync();

            return new CommentsWrapViewModel(records, count, limit, offset)
            {
                ParentId = parentId
            };

        }

        public static async Task<StranitzaComment> CreateCommentAsync(this DbSet<StranitzaComment> dbSet, CommentViewModel vModel, string uploader)
        {
            var entry = new StranitzaComment()
            {
                PostId = vModel.PostId,
                IssueId = vModel.IssueId,
                EPageId = vModel.EPageId,
                ParentId = vModel.ParentId,
                AuthorId = uploader,

                Content = vModel.Content
            };

            await dbSet.AddAsync(entry);

            return entry;
        }

        public static async Task<StranitzaComment> UpdateCommentAsync(this DbSet<StranitzaComment> dbSet, CommentViewModel vModel, string moderatorId)
        {
            var entry = await dbSet.FindAsync(vModel.Id);
            if (entry == null)
            {
                return null;
            }

            dbSet.Attach(entry);

            entry.ModeratorId = moderatorId;
            entry.Note = vModel.Note;

            return entry;
        }
    }
}
