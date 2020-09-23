using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Utility;

namespace stranitza.Repositories
{
    public static class UserRepository
    {
        public static async Task<AdminViewModel> GetUsersPagedAsync(this DbSet<ApplicationUser> dbSet,
            string email,
            string userName,
            string firstName,
            string lastName,
            string description,
            bool? isAuthor,
            int? pageIndex, int pageSize = 10)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var query = dbSet.AsQueryable();
            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(x => EF.Functions.Like(x.UserName, $"%{userName}%"));
            }

            if (!string.IsNullOrEmpty(firstName))
            {
                query = query.Where(x => EF.Functions.Like(x.FirstName, $"%{firstName}%"));
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                query = query.Where(x => EF.Functions.Like(x.LastName, $"%{lastName}%"));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(x => EF.Functions.Like(x.Email, $"%{email}%"));
            }

            if (!string.IsNullOrEmpty(description))
            {
                query = query.Where(x => EF.Functions.Like(x.Description, $"%{description}%"));
            }

            if (isAuthor.HasValue)
            {
                query = query.Where(x => x.IsAuthor == isAuthor);
            }

            var count = await query.CountAsync();
            var users = query
                .Include(x => x.Sources)
                .Include(x => x.Comments)
                .OrderByDescending(x => x.LastUpdated)
                .Select(x => FromApplicationUser(x)).Skip((pageIndex.Value - 1) * pageSize).Take(pageSize);

            return new AdminViewModel(count, pageIndex.Value, pageSize)
            {
                Records = await users.ToListAsync()
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

                // AvatarType = x.AvatarType,
                // FacebookAvatarPath = x.FacebookAvatarPath,
                // TwitterAvatarPath = x.TwitterAvatarPath,
                // GoogleAvatarPath = x.GoogleAvatarPath,
                // InternalAvatarPath = x.InternalAvatarPath,

                AvatarPath = StranitzaExtensions.GetAvatarPath(x.AvatarType, x.Email, 
                    x.FacebookAvatarPath, x.TwitterAvatarPath, x.GoogleAvatarPath, x.InternalAvatarPath),

                Sources = x.Sources,
                Comments = x.Comments,

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
