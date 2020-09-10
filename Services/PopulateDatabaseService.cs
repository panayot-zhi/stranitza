using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using stranitza.Models.Database;
using stranitza.Utility;
using Serilog;

namespace stranitza.Services
{
    public class PopulateDatabaseService
    {
        public static void EnsureCriticalFilesAndFolders(IConfiguration configuration)
        {
            var rootFolderPath = configuration["RepositoryPath"];

            if (!Directory.Exists(rootFolderPath))
            {
                throw new StranitzaException($"No directory found at {rootFolderPath}.");
            }

            var jsonFilePath = Path.Combine(configuration["RepositoryPath"], StranitzaConstants.IndexJsonFileName);
            if (!File.Exists(jsonFilePath))
            {
                Log.Logger.Warning("No json index file found at {JsonFilePath}.", jsonFilePath);
            }

            var forbiddenPagePdfFilePath = Path.Combine(rootFolderPath, StranitzaConstants.ForbiddenPagePdfFileName);
            if (!File.Exists(forbiddenPagePdfFilePath))
            {
                Log.Logger.Warning("No forbidden page pdf file found at {ForbiddenPagePdfFilePath}.", forbiddenPagePdfFilePath);
            }

            var issuesFolderPath = Path.Combine(rootFolderPath, StranitzaConstants.IssuesFolderName);
            if (!Directory.Exists(issuesFolderPath))
            {
                Directory.CreateDirectory(issuesFolderPath);
            }

            var uploadsFolderPath = Path.Combine(rootFolderPath, StranitzaConstants.UploadsFolderName);
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }
        }

        public static async Task EnsureCriticalRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var rolePair in StranitzaRolesHelper.KnownRoles)
            {
                var role = rolePair.Value;
                var roleCheck = await roleManager.RoleExistsAsync(role);
                if (!roleCheck)
                {
                    //create the roles and seed them to the database
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        Log.Logger.Error("{Role} creation failed: {@RoleResult}", role, roleResult.Errors);
                    }
                }
            }
        }

        /// <summary>
        /// Create or wire default users to the Administrator and HeadEditor role.
        /// Don't call this method before you've ensured that critical roles fro the application exist.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static async Task EnsureCriticalUsers(IServiceProvider serviceProvider, IConfiguration configuration)
        {            
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            IdentityResult userResult;
            
            var administrator = await userManager.FindByEmailAsync(StranitzaConstants.AdministratorEmail);
            if (administrator == null)
            {
                administrator = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = StranitzaConstants.AdministratorUsername,
                    Email = StranitzaConstants.AdministratorEmail,
                    EmailConfirmed = true,
                    FirstName = StranitzaConstants.AdministratorFirstName,
                    LastName = StranitzaConstants.AdministratorLastName,

                    AvatarType = StranitzaAvatarType.Gravatar,                    
                    //AvatarPath = StranitzaExtensions.GetGravatarUrl(StranitzaConstants.AdministratorEmail)
                };

                userResult = await userManager.CreateAsync(administrator, configuration["DefaultPassword"]);
                if (!userResult.Succeeded)
                {
                    Log.Logger.Error("Administrator creation failed: {@UserResult}", userResult.Errors);
                    throw new StranitzaException("Administrator creation failed!");
                }

                await userManager.AddToRoleAsync(administrator, StranitzaRolesHelper.AdministratorRoleName);
            }
            
            var headEditor = await userManager.FindByEmailAsync(StranitzaConstants.HeadEditorEmail);
            if (headEditor == null)
            {
                headEditor = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = StranitzaConstants.HeadEditorUsername,
                    Email = StranitzaConstants.HeadEditorEmail,
                    EmailConfirmed = true,
                    FirstName = StranitzaConstants.HeadEditorFirstName,
                    LastName = StranitzaConstants.HeadEditorLastName,
                    IsAuthor = true,

                    AvatarType = StranitzaAvatarType.Gravatar,
                    //AvatarPath = StranitzaExtensions.GetGravatarUrl(StranitzaConstants.HeadEditorEmail)
                };

                userResult = await userManager.CreateAsync(headEditor, configuration["DefaultPassword"]);
                if (!userResult.Succeeded)
                {
                    Log.Logger.Error("Head editor creation failed: {@UserResult}", userResult.Errors);
                    throw new StranitzaException("Head editor creation failed!");
                }

                await userManager.AddToRoleAsync(headEditor, StranitzaRolesHelper.HeadEditorRoleName);
            }                            
        }

        public static async Task LoadIssuesFromRootFolder(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var issueService = serviceProvider.GetRequiredService<LibraryService>();

            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var stranitzaIssuesCount = dbContext.StranitzaIssues.Count();
            if (stranitzaIssuesCount > 0)
            {
                Log.Logger.Information("Database contains {IssuesCount} number of issue records. Skip loading from root...", stranitzaIssuesCount);
                return;
            }

            var issuesFolderPath = Path.Combine(configuration["RepositoryPath"], StranitzaConstants.IssuesFolderName);

            Log.Logger.Information("Recreating database issues from root directory folder structure...");

            int issueDirectoryCount = 0;
            foreach (var releaseYear in Directory.GetDirectories(issuesFolderPath, "*", SearchOption.TopDirectoryOnly))
            {
                Log.Logger.Information("Processing directory {ReleaseYear} in root.", releaseYear);

                foreach (var releaseNumber in Directory.GetDirectories(releaseYear, "*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        await issueService.CreateIssueRecord(new DirectoryInfo(releaseNumber));
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Failed creating issue from directory information {ReleaseYear} in root: {Message}", releaseNumber, ex.Message);
                    }
                }

                issueDirectoryCount++;
            }

            Log.Logger.Information("Processed {issueDirectoryCount} directories in root.", issueDirectoryCount);
        }

        public static async Task LoadIndexFromRootFolder(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var jsonFilePath = Path.Combine(configuration["RepositoryPath"], StranitzaConstants.IndexJsonFileName);

            if (!File.Exists(jsonFilePath))
            {
                return;
            }

            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var stranitzaSourcesCount = dbContext.StranitzaSources.Count();
            if (stranitzaSourcesCount > 0)
            {
                Log.Logger.Information("Database contains {SourcesCount} number of index records. Skip loading from json...", stranitzaSourcesCount);
                return;
            }

            var indexService = serviceProvider.GetRequiredService<IndexService>();            
            //var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            //var administrator = await userManager.FindByEmailAsync(StranitzaConstants.AdministratorEmail);

            /*if (administrator == null)
            {
                throw new StranitzaException("Administrator user cannot be found!");
            }*/ 

            var indexJson = await File.ReadAllTextAsync(jsonFilePath);
            var indexEntries = JArray.Parse(indexJson);

            foreach (var ie in indexEntries)
            {
                var indexEntry = ie.Value<JObject>();

                // NOTE: Checking for existing sources won't create new records
                // on sources that match FirstName and LastName and Title
                // or Origin and Title; omit this check only if the database
                // sources table is empty or don't run at all, as this should be run per request
                await indexService.CreateIndexRecord(indexEntry, uploader: "SYSTEM", executeExistingSourceCheck: false);
            }
        }
    }
}
