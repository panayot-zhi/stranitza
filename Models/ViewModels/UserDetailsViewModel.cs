using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using stranitza.Models.Database;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class UserDetailsViewModel
    {
        [Display(Name = "Идентификационен номер")]
        public virtual string Id { get; set; }

        [Display(Name = "Псевдоним")]
        public virtual string UserName { get; set; }

        [Display(Name = "Email")]
        public virtual string Email { get; set; }

        [Display(Name = "Потвърден")]
        public virtual bool EmailConfirmed { get; set; }

        [Display(Name = "Телефонен номер")]
        public virtual string PhoneNumber { get; set; }

        [Display(Name = "Потвърден")]
        public virtual bool PhoneNumberConfirmed { get; set; }

        [Display(Name = "Бан до (дата)")]
        public virtual DateTimeOffset? LockoutEnd { get; set; }

        [Display(Name = "Брой невалидни опити за вход")]
        public virtual int AccessFailedCount { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Име")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "Автор")]
        public bool IsAuthor { get; set; }

        public string DisplayName { get; set; }

        [Display(Name = "Да се показва")]
        public StranitzaDisplayNameType DisplayNameType { get; set; }

        [Display(Name = "Email видим")]
        public bool DisplayEmail { get; set; }

        public string AvatarPath { get; set; }

        // public StranitzaAvatarType AvatarType { get; set; }
        //
        // public string FacebookAvatarPath { get; set; }
        //
        // public string TwitterAvatarPath { get; set; }
        //
        // public string GoogleAvatarPath { get; set; }
        //
        // public string InternalAvatarPath { get; set; }

        public string Names => $"{FirstName} {LastName}";

        public string DisplayNameOnly => this.DisplayName?.Replace($" <{this.Email}>", string.Empty);

        public string DisplayNameConditional => this.DisplayEmail ? this.DisplayNameOnly : this.DisplayName;


        [Display(Name = "Роли")]
        public IList<string> Roles { get; set; }

        [Display(Name = "Роля")]
        public StranitzaRoles Role { get; set; }

        [Display(Name = "Източници")]
        public IndexViewModel Sources { get; set; }

        [Display(Name = "Има източници")]
        public bool HasAnySources => Sources?.TotalRecords > 0;

        [Display(Name = "Коментари")]
        public ICollection<StranitzaComment> Comments { get; set; }

        [Display(Name = "Има източници")]
        public bool HasAnyComments => Comments?.Count > 0;

        [Display(Name = "Последна промяна")]
        public DateTime LastUpdated { get; set; }

        [Display(Name = "Дата на създаване")]
        public DateTime DateCreated { get; set; }

    }
}
