using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class PostSearchViewModel : PagedViewModel
    {
        public string SearchQuery { get; set; }

        public IEnumerable<PostIndexViewModel> Records { get; set; }

        public PostSearchViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {
        }
    }
}
