using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stranitza.Models.Database
{
    public class StranitzaComment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1024)]
        public string Content { get; set; }

        [MaxLength(1024)]
        public string Note { get; set; }

        #region Navigation

        [Required]
        [MaxLength(127)]
        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public ApplicationUser Author { get; set; }

        [MaxLength(127)]
        [ForeignKey("Moderator")]
        public string ModeratorId { get; set; }

        public ApplicationUser Moderator { get; set; }

        [ForeignKey("Parent")]
        public int? ParentId { get; set; }

        public StranitzaComment Parent { get; set; }

        [ForeignKey("Issue")]
        public int? IssueId { get; set; }

        public StranitzaIssue Issue { get; set; }

        [ForeignKey("Post")]
        public int? PostId { get; set; }

        public StranitzaPost Post { get; set; }

        [ForeignKey("EPage")]
        public int? EPageId { get; set; }

        public StranitzaEPage EPage { get; set; }

        public ICollection<StranitzaComment> Comments { get; set; }

        #endregion

        #region Automatic

        public DateTime LastUpdated { get; set; }
                
        public DateTime DateCreated { get; set; }

        #endregion

    }
}
