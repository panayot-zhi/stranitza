using System.Collections.Generic;

namespace stranitza.Models.ViewModels
{
    public class CommentsWrapViewModel
    {
        public IList<CommentViewModel> Comments { get; set; }

        public string CurrentUserDisplayName { get; set; }

        public int? ParentId { get; set; }

        public int TotalCount { get; set; }

        public int CurrentOffset { get; set; }

        public int Limit { get; set; }

        public CommentsWrapViewModel(IList<CommentViewModel> comments, int totalCount, int limit, int currentOffset)
        {
            Comments = comments;
            TotalCount = totalCount;
            CurrentOffset = currentOffset;
            Limit = limit;
        }
    }
}
