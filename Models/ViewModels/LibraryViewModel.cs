using System.Collections.Generic;
using stranitza.Models.Database.Views;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class LibraryViewModel : PagedViewModel
    {
        public Dictionary<int, List<IssueIndexViewModel>> IssuesByYear { get; set; }        

        public IEnumerable<CountByYears> YearFilter { get; set; }

        public int? CurrentYear { get; set; }

        public LibraryViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {

        }
    }
}
