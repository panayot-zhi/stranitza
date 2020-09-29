using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Utility;

namespace stranitza.Repositories
{
    public static class UserRepository
    {
        public static async Task<UserIndexViewModel> GetUsersPagedAsync(this UserManager<ApplicationUser> userManager, int? pageIndex, UserFilterViewModel filter, int pageSize = 10)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            IQueryable<ApplicationUser> query;

            switch (filter.Type)
            {
                case UserFilterType.Administrators:
                    query = (await userManager.GetUsersInRoleAsync(StranitzaRolesHelper.AdministratorRoleName)).AsQueryable();
                    break;
                case UserFilterType.Editors:
                    var editors = await userManager.GetUsersInRoleAsync(StranitzaRolesHelper.EditorRoleName);
                    var headEditors = await userManager.GetUsersInRoleAsync(StranitzaRolesHelper.HeadEditorRoleName);
                    var combination = new List<ApplicationUser>(headEditors.Count + editors.Count);
                    
                    combination.AddRange(editors);
                    combination.AddRange(headEditors);

                    query = combination.AsQueryable();
                    break;
                case UserFilterType.Authors:
                    query = userManager.Users.Where(x => x.IsAuthor == true).AsQueryable();
                    break;
                case UserFilterType.LockedOut:
                    query = userManager.Users.Where(x => x.LockoutEnd > DateTimeOffset.UtcNow).AsQueryable();
                    break;
                case UserFilterType.None:
                default:
                    query = userManager.Users.AsQueryable();
                    break;
            }

            if (!string.IsNullOrEmpty(filter.UserName))
            {
                query = query.Where(x => EF.Functions.Like(x.UserName, $"%{filter.UserName}%"));
            }

            if (!string.IsNullOrEmpty(filter.Name))
            {
                if (filter.Name.Contains(" "))
                {
                    var names = filter.Name.Split(" ");

                    if (names.Length == 2)
                    {
                        query = query.Where(x => 
                            (EF.Functions.Like(x.FirstName, $"%{names[0]}%") 
                             || EF.Functions.Like(x.LastName, $"%{names[1]}%")) &&
                            (EF.Functions.Like(x.FirstName, $"%{names[0]}%") 
                             || EF.Functions.Like(x.LastName, $"%{names[1]}%")));
                    }
                    else
                    {
                        query = query.Where(x => 
                            EF.Functions.Like(x.FirstName, $"%{filter.Name.Trim()}%") 
                            || EF.Functions.Like(x.LastName, $"%{filter.Name.Trim()}%"));
                    }
                }
                else
                {
                    query = query.Where(x =>
                        EF.Functions.Like(x.FirstName, $"%{filter.Name}%")
                        || EF.Functions.Like(x.LastName, $"%{filter.Name}%"));
                }
            }

            if (!string.IsNullOrEmpty(filter.Email))
            {
                query = query.Where(x => EF.Functions.Like(x.Email, $"%{filter.Email}%"));
            }

            if (!string.IsNullOrEmpty(filter.Description))
            {
                query = query.Where(x => EF.Functions.Like(x.Description, $"%{filter.Description}%"));
            }

            var count = query.Count();
            var users = query
                .Select(x => FromApplicationUser(x))
                .Skip((pageIndex.Value - 1) * pageSize).Take(pageSize);

            return new UserIndexViewModel(count, pageIndex.Value, pageSize)
            {
                Records = users.ToList()
            };
        }

        public static async Task<ApplicationUser> UpdateUserAsync(this DbSet<ApplicationUser> dbSet, UserDetailsViewModel vModel)
        {
            var entry = await dbSet.FindAsync(vModel.Id);

            dbSet.Attach(entry);

            // TODO

            return entry;
        }
        
        public static UserDetailsViewModel FromApplicationUser(ApplicationUser x)
        {
            return new UserDetailsViewModel()
            {
                Id = x.Id,
                UserName = x.UserName,
                FirstName = x.FirstName,
                LastName = x.LastName,
                IsAuthor = x.IsAuthor,

                Email = x.Email,
                EmailConfirmed = x.EmailConfirmed,

                Description = x.Description,
                PhoneNumber = x.PhoneNumber,
                PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                AccessFailedCount = x.AccessFailedCount,
                LockoutEnd = x.LockoutEnd,

                DisplayNameType = x.DisplayNameType,
                DisplayEmail = x.DisplayEmail,

                DisplayName = StranitzaExtensions.GetDisplayName(x.DisplayNameType, x.UserName, x.Names, x.Email, x.DisplayEmail),

                AvatarPath = StranitzaExtensions.GetAvatarPath(x.AvatarType, x.Email, 
                    x.FacebookAvatarPath, x.TwitterAvatarPath, x.GoogleAvatarPath, x.InternalAvatarPath),

                LastUpdated = x.LastUpdated,
                DateCreated = x.DateCreated,

            };
        }

        public static async Task<ApplicationUser> FindAuthorAsync(this DbSet<ApplicationUser> dbSet, string firstName, string lastName)
        {            
            var authorQuery = dbSet.Where(user =>
                user.IsAuthor && user.FirstName == firstName && user.LastName == lastName);

            var count = await authorQuery.CountAsync();

            if (count == 1)
            {
                var author = authorQuery.Single();
                Log.Logger.Information("Author {Names} found in the system. Assigning entry.",
                    author.FirstName + " " + author.LastName);

                return author;
            }

            if (count > 1)
            {
                Log.Logger.Warning("Could not assign author for this index entry automatically: Competing users for entry, please resolve manually.");

                foreach (var applicationUser in authorQuery)
                {
                    Log.Logger.Warning("Author {Names} ({UserName}) {Email} created {DateCreated}.",
                        applicationUser.FirstName + " " + applicationUser.LastName,
                        applicationUser.UserName,
                        applicationUser.Email,
                        applicationUser.DateCreated);
                }
            }

            // the check for an author 
            // yielded no results                
            return null;
        }

    }
}
