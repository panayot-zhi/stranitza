using System;
using System.Collections.Generic;

namespace stranitza.Models.ViewModels
{
    public class PostDetailsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Origin { get; set; }

        public string ImageFileName { get; set; }                        

        public string Description { get; set; }

        public string Content { get; set; }

        public bool EditorsPick { get; set; }

        public int ViewCount { get; set; }

        public string Uploader { get; set; }

        public string UploaderId { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime DateCreated { get; set; }

        public int CommentsCount { get; set; }

        //public ICollection<CommentViewModel> Comments { get; set; }

        public ICollection<SuggestionsViewModel> MoreFromAuthor { get; set; }

        public ICollection<SuggestionsViewModel> EditorPicks { get; set; }

        public ICollection<SuggestionsViewModel> RecentPosts { get; set; }
    }
}