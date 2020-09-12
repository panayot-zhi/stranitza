using System.Threading.Tasks;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Repositories;

namespace stranitza.Services
{
    public class ELibraryService
    {
        private readonly ApplicationDbContext _db;

        public ELibraryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<StranitzaEPage> CreateEPageAsync(EPageCreateViewModel vModel, string uploaderId, string uploaderUserName)
        {
            // create epage
            var entry = await _db.StranitzaEPages.CreateEPageAsync(vModel, uploaderId);

            // find author
            var author = await _db.Users.FindAuthorAsync(entry.FirstName, entry.LastName);

            // assign author, so it can propagate to source also
            entry.AuthorId = author?.Id;

            // save epage
            await _db.SaveChangesAsync();

            // create source
            var source = await _db.StranitzaSources.CreateSourceAsync(entry, uploaderUserName);

            // save source
            await _db.SaveChangesAsync();

            // attach for update
            _db.StranitzaEPages.Attach(entry);

            // assign source
            entry.SourceId = source.Id;            
            
            // save epage
            await _db.SaveChangesAsync();

            return entry;
        }
    }
}
