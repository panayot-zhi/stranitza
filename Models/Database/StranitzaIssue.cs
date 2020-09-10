using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using stranitza.Utility;

namespace stranitza.Models.Database
{
    public class StranitzaIssue
    {
        public int Id { get; set; }

        [Required]        
        public int IssueNumber { get; set; }

        [Required]
        public int ReleaseNumber { get; set; }

        [Required]
        public int ReleaseYear { get; set; }

        [MaxLength(1024)]
        public string Description { get; set; }

        public bool IsAvailable { get; set; }

        public int PagesCount { get; set; }

        public int ViewCount { get; set; }

        public int DownloadCount { get; set; }

        [Column(TypeName = "VARCHAR(512)")]
        public int[] AvailablePages { get; set; }

        [Column(TypeName = "VARCHAR(256)")]
        public string[] Tags { get; set; }

        #region Navigation

        [ForeignKey("PdfFilePreview")]
        public int? PdfFilePreviewId { get; set; }

        public StranitzaFile PdfFilePreview { get; set; }

        [ForeignKey("PdfFileReduced")]
        public int? PdfFileReducedId { get; set; }

        public StranitzaFile PdfFileReduced { get; set; }

        [ForeignKey("ZipFile")]
        public int? ZipFileId { get; set; }

        public StranitzaFile ZipFile { get; set; }

        public ICollection<StranitzaPage> Pages { get; set; }

        public ICollection<StranitzaComment> Comments { get; set; }

        public ICollection<StranitzaSource> Sources { get; set; }

        #endregion

        #region NotMapped

        [NotMapped]
        public bool HasPdf => PdfFilePreviewId.HasValue;

        [NotMapped]
        public StranitzaPage CoverPage => Pages.SingleOrDefault(x => x.Type == StranitzaPageType.Cover);

        [NotMapped]
        public StranitzaPage IndexPage => Pages.SingleOrDefault(x => x.Type == StranitzaPageType.Index);

        #endregion

        #region Automatic

        public DateTime LastUpdated { get; set; }
        
        public DateTime DateCreated { get; set; }

        #endregion


    }
}
