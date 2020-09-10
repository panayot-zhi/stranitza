using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using stranitza.Utility;

namespace stranitza.Models.Database
{
    public class StranitzaPage
    {
        public int Id { get; set; }

        [Required]
        public int PageNumber { get; set; }

        [Required]
        public int SlideNumber { get; set; }

        public bool IsAvailable { get; set; }

        public StranitzaPageType Type { get; set; }

        #region Navigation

        [ForeignKey("PageFile")]
        public int PageFileId { get; set; }

        public StranitzaFile PageFile { get; set; }

        [ForeignKey("Issue")]
        public int IssueId { get; set; }

        public StranitzaIssue Issue { get; set; }

        #endregion

        #region Automatic
        
        public DateTime LastUpdated { get; set; }

        public DateTime DateCreated { get; set; }

        #endregion
    }
}
