using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using stranitza.Models.Database;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class UserFilterViewModel
    {
        public string Name { get; set; }

        public virtual string UserName { get; set; }

        public virtual string Email { get; set; }

        // public virtual bool EmailConfirmed { get; set; }

        // public virtual string PhoneNumber { get; set; }

        // public virtual bool PhoneNumberConfirmed { get; set; }

        // public virtual DateTimeOffset? LockoutEnd { get; set; }

        // public virtual int AccessFailedCount { get; set; }

        public string Description { get; set; }

        // public string LastName { get; set; }

        //public bool? IsAuthor { get; set; }

        // public DateTime LastUpdated { get; set; }

        // public DateTime DateCreated { get; set; }

    }
}
