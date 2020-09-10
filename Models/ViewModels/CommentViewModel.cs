using System;
using System.ComponentModel.DataAnnotations;

namespace stranitza.Models.ViewModels
{
    public class CommentViewModel
    {
        public int? Id { get; set; }

        public int? PostId { get; set; }

        public int? IssueId { get; set; }

        public int? EPageId { get; set; }

        public int? ParentId { get; set; }

        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Required(ErrorMessage = "Моля, попълнете полето.")]
        public string Content { get; set; }         
        
        public string Note { get; set; }          

        public string UploaderId { get; set; }

        public string UploaderDisplayName { get; set; }

        public string UploaderAvatarPath { get; set; }

        public string ModeratorId { get; set; }

        public string ModeratorDisplayName { get; set; }

        public CommentsWrapViewModel Children { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime DateCreated { get; set; }

    }
}
