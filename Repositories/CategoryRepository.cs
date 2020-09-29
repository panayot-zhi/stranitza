using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Utility;

namespace stranitza.Repositories
{
    public static class CategoryRepository
    {
        public static async Task<List<CategoryViewModel>> GetCategoryIndexViewModelAsync(this DbSet<StranitzaCategory> dbSet, string sortPropertyName, SortOrder sortOrder)
        {
            var query = dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(sortPropertyName))
            {
                query = query.OrderBy(sortPropertyName, sortOrder);
            }

            return await query.Select(x => new CategoryViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,

                EPagesCount = x.EPages.Count,
                AllSourcesCount = x.Sources.Count,

                LastUpdated = x.LastUpdated,
                DateCreated = x.DateCreated,

            }).ToListAsync();

        }        

        public static async Task<IEnumerable<FilterCategoryViewModel>> GetCategoryFilterViewModelAsync(this DbSet<StranitzaCategory> dbSet)
        {
            return await dbSet.Select(x => new FilterCategoryViewModel()
            {
                Category = x.Name,
                CategoryId = x.Id,
                Count = x.Sources.Count

            }).ToListAsync();
        }

        public static async Task<IEnumerable<FilterCategoryViewModel>> GetCategoryFilterForAuthorViewModelAsync(this DbSet<StranitzaCategory> dbSet, string authorId)
        {
            return await dbSet.Select(x => new FilterCategoryViewModel()
            {
                Category = x.Name,
                CategoryId = x.Id,
                Count = x.Sources.Count(y => y.AuthorId == authorId)

            }).ToListAsync();
        }

        public static async Task<CategoryViewModel> GetCategoryViewModelAsync(this DbSet<StranitzaCategory> dbSet, int id)
        {
            return await dbSet.Select(x => new CategoryViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,

                EPagesCount = x.EPages.Count,
                AllSourcesCount = x.Sources.Count,

                LastUpdated = x.LastUpdated,
                DateCreated = x.DateCreated,

            }).SingleOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<StranitzaCategory> CreateCategoryAsync(this DbSet<StranitzaCategory> dbSet, CategoryViewModel vModel)
        {
            var entry = new StranitzaCategory()
            {
                Description = vModel.Description,
                Name = vModel.Name,                
            };

            await dbSet.AddAsync(entry);

            return entry;
        }

        public static async Task<StranitzaCategory> UpdateCategoryAsync(this DbSet<StranitzaCategory> dbSet, CategoryViewModel vModel)
        {
            var entry = await dbSet.FindAsync(vModel.Id);

            dbSet.Attach(entry);

            entry.Description = vModel.Description;
            entry.Name = vModel.Name;

            return entry;
        }
    }
}