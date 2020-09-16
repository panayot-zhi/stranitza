using System;
using System.Collections.Generic;

namespace stranitza.Models.ViewModels
{
    public class IssueDetailsViewModel
    {
        public int Id { get; set; }

        public int IssueNumber { get; set; }

        public int ReleaseNumber { get; set; }

        public int ReleaseYear { get; set; }

        public string Description { get; set; }

        public bool IsAvailable { get; set; }

        public int PagesCount { get; set; }

        public int ViewCount { get; set; }

        public int DownloadCount { get; set; }

        public int[] AvailablePages { get; set; }

        public string[] Tags { get; set; }

        public int CommentsCount { get; set; }

        public int? PdfFilePreviewId { get; set; }

        public int? PdfFileDownloadId { get; set; }

        public bool HasPdf => PdfFilePreviewId.HasValue;


        public DateTime LastUpdated { get; set; }

        public DateTime DateCreated { get; set; }


        public IEnumerable<PageViewModel> Pages { get; set; }

        public ICollection<SourceDetailsViewModel> Sources { get; set; }

    }
}