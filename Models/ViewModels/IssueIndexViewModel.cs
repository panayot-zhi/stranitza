using System;

namespace stranitza.Models.ViewModels
{
    public class IssueIndexViewModel
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

        public bool HasPdf { get; set; }

        public PageViewModel CoverPage { get; set; }

        public PageViewModel IndexPage { get; set; }


        public DateTime LastUpdated { get; set; }

        public DateTime DateCreated { get; set; }
    }
}

