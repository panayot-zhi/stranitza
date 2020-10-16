using System.Collections.Generic;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class IssueSearchViewModel : PagedViewModel
    {
        public string SearchQuery { get; set; }

        public IEnumerable<IssueIndexViewModel> Records { get; set; }

        public IssueSearchViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {
        }
    }
}
