using System;
using System.ComponentModel.DataAnnotations;

namespace stranitza.Models.Database
{
    public class StranitzaFile
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(127)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(128)]
        public string MimeType { get; set; }

        [Required]
        [MaxLength(256)]
        public string Title { get; set; }

        [Required]
        [MaxLength(32)]
        public string Extension { get; set; }

        [Required]
        [MaxLength(512)]
        public string FilePath { get; set; }

        [MaxLength(512)]
        public string ThumbPath { get; set; }

        #region Automatic

        public DateTime LastUpdated { get; set; }

        public DateTime DateCreated { get; set; }

        #endregion Automatic
    }
}