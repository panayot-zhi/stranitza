using System.Collections.Generic;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class NewsViewModel : PagedViewModel
    {
        public IEnumerable<PostIndexViewModel> Records { get; set; }

        public NewsViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {
        }
    }
}
