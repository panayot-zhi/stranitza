using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stranitza.Models.Database
{
    public class StranitzaPost
    {        
        public int Id { get; set; }

        [Required]
        [MaxLength(512)]
        public string Title { get; set; }
        
        [Required]
        [Column(TypeName = "TEXT")]
        public string Content { get; set; }

        [MaxLength(1024)]
        public string Description { get; set; }

        public bool EditorsPick { get; set; }

        public int ViewCount { get; set; }

        [Required]
        [MaxLength(1024)]
        public string Origin { get; set; }

        #region Navigation

        [Required]
        [MaxLength(127)]
        [ForeignKey("Uploader")]
        public string UploaderId { get; set; }
        
        public ApplicationUser Uploader { get; set; }        

        [ForeignKey("ImageFile")]
        public int? ImageFileId { get; set; }

        public StranitzaFile ImageFile { get; set; }

        public ICollection<StranitzaComment> Comments { get; set; }

        #endregion

        #region Automatic

        public DateTime LastUpdated { get; set; }
        
        public DateTime DateCreated { get; set; }

        #endregion
    }
}
