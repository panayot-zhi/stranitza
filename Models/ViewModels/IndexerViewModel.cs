using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class IndexerViewModel
    {
        public int IssueId { get; set; }
        
        public int ReleaseNumber { get; set; }
        
        public int ReleaseYear { get; set; }
        
        public int IssueNumber { get; set; }

        public StranitzaIndexer.IndexingResult Result { get; set; }

        public ICollection<SourceDetailsViewModel> ExistingSources { get; set; }

        public SelectList Categories { get; set; }
    }
}
