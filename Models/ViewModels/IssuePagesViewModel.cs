using System.Collections.Generic;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class IssuePagesViewModel : PagedViewModel
    {
        public int Id { get; set; }

        public int IssueNumber { get; set; }

        public int ReleaseYear { get; set; }

        public int ReleaseNumber { get; set; }

        public IEnumerable<PageViewModel> Records { get; set; }

        public IssuePagesViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {
        }
    }
}
