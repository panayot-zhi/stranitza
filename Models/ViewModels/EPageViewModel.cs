using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;

namespace stranitza.Models.ViewModels
{
    public class EPageViewModel
    {
        public Dictionary<string, List<EPageIndexViewModel>> EPagesByCategory { get; set; }

        public IEnumerable<FilterYearViewModel> YearFilter { get; set; }

        public int CurrentYear { get; set; }

        public bool HasArchive => YearFilter.Any();
    }
}
