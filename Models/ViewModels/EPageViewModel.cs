﻿using System.Collections.Generic;
using System.Linq;
using stranitza.Models.Generic;

namespace stranitza.Models.ViewModels
{
    public class EPageViewModel
    {
        public Dictionary<string, List<EPageIndexViewModel>> EPagesByCategory { get; set; }

        public IEnumerable<CountByYears> YearFilter { get; set; }

        public int CurrentYear { get; set; }

        public bool HasArchive => YearFilter.Any();
    }
}
