using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stranitza.Models.Database
{
    public class StranitzaSource
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string FirstName { get; set; }

        [MaxLength(255)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(512)]
        public string Origin { get; set; }

        [MaxLength(512)]
        public string Title { get; set; }

        [MaxLength(1024)]
        public string Description { get; set; }

        [MaxLength(1024)]
        public string Notes { get; set; }

        [Required]
        public int ReleaseNumber { get; set; }

        [Required]
        public int ReleaseYear { get; set; }

        [MaxLength(64)]
        public string Pages { get; set; }

        public int StartingPage { get; set; }        

        public bool IsTranslation { get; set; }

        [Required]
        [MaxLength(127)]
        public string Uploader { get; set; }

        #region Navigation

        public int CategoryId { get; set; }

        public StranitzaCategory Category { get; set; }

        public int? IssueId { get; set; }

        public StranitzaIssue Issue { get; set; }

        [ForeignKey("EPage")]
        public int? EPageId { get; set; }
        
        public StranitzaEPage EPage { get; set; }

        [MaxLength(127)]
        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public ApplicationUser Author { get; set; }

        #endregion

        #region Automatic

        public DateTime LastUpdated { get; set; }
        
        public DateTime DateCreated { get; set; }

        #endregion
    }
}
