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
    public class UserDetailsViewModel
    {
        public virtual string Id { get; set; }

        public virtual string UserName { get; set; }

        //public virtual string NormalizedUserName { get; set; }

        public virtual string Email { get; set; }

        //public virtual string NormalizedEmail { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        //public virtual string PasswordHash { get; set; }

        //public virtual string SecurityStamp { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual bool PhoneNumberConfirmed { get; set; }

        //public virtual bool TwoFactorEnabled { get; set; }

        public virtual DateTimeOffset? LockoutEnd { get; set; }

        //public virtual bool LockoutEnabled { get; set; }

        public virtual int AccessFailedCount { get; set; }

        public string Description { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsAuthor { get; set; }

        public string DisplayName { get; set; }

        public StranitzaDisplayNameType DisplayNameType { get; set; }

        public bool DisplayEmail { get; set; }

        public string AvatarPath { get; set; }

        public StranitzaAvatarType AvatarType { get; set; }

        public string FacebookAvatarPath { get; set; }

        public string TwitterAvatarPath { get; set; }

        public string GoogleAvatarPath { get; set; }

        public string InternalAvatarPath { get; set; }

        public string Names => $"{FirstName} {LastName}";

        public string DisplayNameOnly => this.DisplayName.Replace($" <{this.Email}>", string.Empty);

        public string DisplayNameConditional => this.DisplayEmail ? this.DisplayNameOnly : this.DisplayName;


        // public ICollection<StranitzaEPage> AuthoredEPages { get; set; }

        // public ICollection<StranitzaEPage> UploadedEPages { get; set; }

        public ICollection<StranitzaSource> Sources { get; set; }

        public ICollection<StranitzaComment> Comments { get; set; }

        // public ICollection<StranitzaComment> ModeratedComments { get; set; }

        // public ICollection<StranitzaPost> Posts { get; set; }


        public DateTime LastUpdated { get; set; }

        public DateTime DateCreated { get; set; }

    }
}
