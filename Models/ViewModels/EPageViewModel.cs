using stranitza.Models.Database.Views;
using System.Collections.Generic;
using System.Linq;

namespace stranitza.Models.ViewModels
{
    public class EPageViewModel
    {
        public Dictionary<string, List<EPageIndexViewModel>> EPagesByCategory { get; set; }

        public IEnumerable<EPagesCountByYear> YearFilter { get; set; }

        public int CurrentYear { get; set; }

        public bool HasArchive => YearFilter.Any();
    }
}
