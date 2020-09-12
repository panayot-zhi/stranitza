using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace stranitza.Models.Database.Views
{
    public class CountByYears
    {
        public int Year { get; set; }

        public int Count { get; set; }
    }

}
