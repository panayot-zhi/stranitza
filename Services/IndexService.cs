using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using stranitza.Models.Database;
using Serilog;
using stranitza.Repositories;

namespace stranitza.Services
{
    public class IndexService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private static readonly char[] QuotesCharacters = { '"', '„', '“', '\'' };

        public IndexService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;            
        }

        public async Task CreateIndexRecord(JObject jsonRecord, string uploader, bool executeExistingSourceCheck = true)
        {
            var entryCategory = jsonRecord.SelectToken("category").Value<string>();
            var entryOriginalCategory = jsonRecord.SelectToken("originalCategory").Value<string>();

            var entry = new StranitzaSource()
            {
                FirstName = jsonRecord.SelectToken("firstName").Value<string>(),
                LastName = jsonRecord.SelectToken("lastName").Value<string>(),
                Origin = jsonRecord.SelectToken("origin").Value<string>() ?? string.Empty,
                Title = jsonRecord.SelectToken("title").Value<string>() ?? string.Empty,
                Description = jsonRecord.SelectToken("description").Value<string>(),
                ReleaseNumber = int.Parse(jsonRecord.SelectToken("releaseNumber").Value<string>()),
                ReleaseYear = int.Parse(jsonRecord.SelectToken("releaseYear").Value<string>()),
                StartingPage = jsonRecord.SelectToken("startingPage").Value<int>(),
                Pages = jsonRecord.SelectToken("pages").Value<string>(),
                Notes = jsonRecord.SelectToken("notes").Value<string>(),
                IsTranslation = jsonRecord.SelectToken("isTranslation").Value<bool>(),

                Uploader = uploader
            };

            // NO: Trim all leading and trailing quotes symbols from title and origin            
            //entry.Title = entry.Title.TrimStart(QuotesCharacters).TrimEnd(QuotesCharacters);
            //entry.Origin = entry.Origin.TrimStart(QuotesCharacters).TrimEnd(QuotesCharacters);

            if (executeExistingSourceCheck && CheckForExistingSource(entry))
            {
                return;
            }

            // NOTE: Resolve author
            entry.Author = await _applicationDbContext.Users.FindAuthorAsync(entry.FirstName, entry.LastName);

            // NOTE: resolve issue by ReleaseNumber and ReleaseYear, which MUST form a unique combination
            var issueByReleaseYearAndNumber = _applicationDbContext.StranitzaIssues.SingleOrDefault(issue =>
                issue.ReleaseNumber == entry.ReleaseNumber && issue.ReleaseYear == entry.ReleaseYear);
            if (issueByReleaseYearAndNumber != null)
            {
                entry.Issue = issueByReleaseYearAndNumber;
            }

            /*  NOTE: EPage creates it's own index entry and manages it. This is not a concern for this method.

            // NOTE: resolve epage by ReleaseNumber and ReleaseYear, which MUST form a unique combination
            var existingEpageEntry = _applicationDbContext.StranitzaEPages.SingleOrDefault(epage =>
                epage.ReleaseYear == entry.ReleaseYear && epage.Title == entry.Title &&
                epage.FirstName == entry.FirstName && epage.LastName == entry.LastName);

            if (existingEpageEntry != null)
            {
                entry.EPage = existingEpageEntry;
            }*/

            if (entryCategory == null)
            {
                entryCategory = "NONE";
                entryOriginalCategory = "Без категория";
            }

            var categoryByName = _applicationDbContext.StranitzaCategories.SingleOrDefault(cat =>
                cat.Name == entryCategory || cat.Name == "IMPORTED_" + entryCategory);

            if (categoryByName == null)
            {
                entry.Category = await CreateCategoryRecord(entryCategory, entryOriginalCategory);
            }
            else
            {
                await UpdateCategoryRecord(categoryByName, entryOriginalCategory);
                entry.Category = categoryByName;
            }

            await _applicationDbContext.AddAsync(entry);
            await _applicationDbContext.SaveChangesAsync();

            Log.Logger.Information("Generated index record {IndexId}.", entry.Id);
        }
        public bool CheckForExistingSource(StranitzaSource entry)
        {
            var existingSourceEntries = _applicationDbContext.StranitzaSources.Where(
                x => (x.FirstName == entry.FirstName && x.LastName == entry.FirstName && x.Title == entry.Title) ||
                     (x.Title == entry.Title && x.Origin == entry.Origin));

            var existingSourcesCount = existingSourceEntries.Count();
            if (existingSourcesCount == 1)
            {                
                Log.Logger.Warning("A record found matching the source FirstName, LastName or Origin and Title! Please check entry: {ExistingEntry}",
                    existingSourceEntries.Single().Id);
                Log.Logger.Information("FirstName: {FirstName}, LastName: {LastName}, Origin: {Origin}, Title: {Title}",
                    entry.FirstName, entry.LastName, entry.Origin, entry.Title);

                return true;
            }

            if (existingSourcesCount > 1)
            {
                Log.Logger.Warning("Multiple records found matching the source FirstName, LastName or Origin and Title! Please check entries: {ExistingEntries}",
                    string.Join(",", existingSourceEntries.Select(x => x.Id.ToString())));
                Log.Logger.Information("FirstName: {FirstName}, LastName: {LastName}, Origin: {Origin}, Title: {Title}",
                    entry.FirstName, entry.LastName, entry.Origin, entry.Title);

                return true;
            }

            // No exisitng sources found in the database...
            return false;
        }

        public async Task<StranitzaCategory> CreateCategoryRecord(string name, string originalName)
        {
            var category = new StranitzaCategory()
            {
                Name = name,
                //DisplayName = name,
                Description = originalName + Environment.NewLine + "(импортирана)"
            };

            await _applicationDbContext.AddAsync(category);
            await _applicationDbContext.SaveChangesAsync();

            Log.Logger.Information("Generated category entry {CategoryId}: {CategoryName}.", category.Id, category.Name);

            return category;
        }

        public async Task UpdateCategoryRecord(StranitzaCategory category, string categoryName)
        {
            if (category.Description == null) return;
            if (category.Description.Contains(categoryName)) return;

            category.Description += Environment.NewLine;
            category.Description += categoryName;

            Log.Logger.Information("Updated category entry {CategoryId}: {AdditionalInfo}", category.Id, categoryName);

            await _applicationDbContext.SaveChangesAsync();            
        }
    }
}
