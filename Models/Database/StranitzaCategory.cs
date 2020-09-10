using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace stranitza.Models.Database
{
    public class StranitzaCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(127)]        
        public string Name { get; set; }

//        [Required]
//        [MaxLength(255)]
//        public string DisplayName { get; set; }

        [MaxLength(1024)]
        public string Description { get; set; }

        #region Navigation

        public ICollection<StranitzaEPage> EPages { get; set; }

        public ICollection<StranitzaSource> Sources { get; set; }
        
        #endregion

        #region Automatic

        public DateTime LastUpdated { get; set; }
        
        public DateTime DateCreated { get; set; }

        #endregion

    }
}
