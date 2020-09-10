using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using stranitza.Utility;

namespace stranitza.Models.Database
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [MaxLength(1024)]
        public string Description { get; set; }

        [MaxLength(255)]
        [ProtectedPersonalData]
        public string FirstName { get; set; }

        [MaxLength(255)]
        [ProtectedPersonalData]
        public string LastName { get; set; }

        [PersonalData]
        public bool IsAuthor { get; set; }

        [PersonalData]
        public StranitzaDisplayNameType DisplayNameType { get; set; }

        [PersonalData]
        public bool DisplayEmail { get; set; }

        #region Avatar

        [PersonalData]
        public StranitzaAvatarType AvatarType { get; set; }

        [PersonalData]
        public string FacebookAvatarPath { get; set; }

        [PersonalData]
        public string TwitterAvatarPath { get; set; }

        [PersonalData]
        public string GoogleAvatarPath { get; set; }

        [PersonalData]
        public string InternalAvatarPath { get; set; }

        #endregion

        #region Navigation

        public ICollection<StranitzaEPage> AuthoredEPages { get; set; }

        public ICollection<StranitzaEPage> UploadedEPages { get; set; }

        public ICollection<StranitzaSource> Sources { get; set; }

        public ICollection<StranitzaComment> Comments { get; set; }

        public ICollection<StranitzaComment> ModeratedComments { get; set; }
        
        public ICollection<StranitzaPost> Posts { get; set; }        

        #endregion

        #region NotMapped

        [NotMapped]
        [PersonalData]
        public string Names => $"{FirstName} {LastName}";

        #endregion

        #region Automatic

        [PersonalData]
        public DateTime LastUpdated { get; set; }

        [PersonalData]
        public DateTime DateCreated { get; set; }

        #endregion
    }
}
