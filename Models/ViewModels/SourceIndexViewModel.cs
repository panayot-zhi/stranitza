using System;

namespace stranitza.Models.ViewModels
{
    public class SourceIndexViewModel
    {
        public int Id { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string Origin { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Notes { get; set; }

        public int ReleaseNumber { get; set; }

        public int ReleaseYear { get; set; }

        public string Pages { get; set; }

        public int StartingPage { get; set; }

        public bool IsTranslation { get; set; }

        public string Uploader { get; set; }        

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }        

//        public StranitzaCategory Category { get; set; }

        public int? IssueId { get; set; }

//        public StranitzaIssue Issue { get; set; }
        
        public int? EPageId { get; set; }

//        public StranitzaEPage EPage { get; set; }

        public string AuthorId { get; set; }

//        public ApplicationUser Author { get; set; }

        public string AuthorAvatarPath { get; set; }

        public string AuthorDisplayName { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime DateCreated { get; set; }        
    }
}
