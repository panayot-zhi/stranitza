using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace stranitza.Models.Database.Repositories
{
    public static class UserRepository
    {
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
