using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace stranitza.Models.Database
{
    public class StranitzaEPage
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(255)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(512)]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "TEXT")]
        [JsonIgnore]
        public string Content { get; set; }        

        [Required]
        public int ReleaseNumber { get; set; }

        [Required]
        public int ReleaseYear { get; set; }

        [MaxLength(1024)]
        public string Description { get; set; }

        [MaxLength(1024)]
        public string Notes { get; set; }

        public bool IsTranslation { get; set; }

        #region Navigation

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public StranitzaCategory Category { get; set; }

        [MaxLength(127)]
        [ForeignKey("Author")]
        public string AuthorId { get; set; }
        
        public ApplicationUser Author { get; set; }

        [Required]
        [MaxLength(127)]
        [ForeignKey("Uploader")]
        public string UploaderId { get; set; }
        
        public ApplicationUser Uploader { get; set; }

        // NOTE: Creates two separate relations with no principal
        //[ForeignKey("Source")]
        public int? SourceId { get; set; }

        public StranitzaSource Source { get; set; }

        public ICollection<StranitzaComment> Comments { get; set; }

        #endregion

        #region Automatic

        public DateTime DateCreated { get; set; }

        #endregion

    }
}
