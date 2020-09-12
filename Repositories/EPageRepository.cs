﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Utility;

namespace stranitza.Repositories
{
    public static class EPageRepository
    {
        public static async Task<EPageViewModel> GetEPagesByYearAsync(this DbSet<StranitzaEPage> dbSet, int? year)
        {
            if (!dbSet.Any())
            {
                return new EPageViewModel()
                {
                    CurrentYear = DateTime.Now.Year,
                    EPagesByCategory = new Dictionary<string, List<EPageIndexViewModel>>()
                };
            }

            if (!year.HasValue)
            {
                // NOTE: The default year, if no year is passed should be the current one
                year = DateTime.Now.Year;  //dbSet.Max(x => x.ReleaseYear);
            }            

            var epages = await dbSet
                .Include(x => x.Category)
                .Include(x => x.Author)
                .Include(x => x.Uploader)
                .Where(x => x.ReleaseYear == year)
                .OrderBy(x => x.DateCreated)
                .Select(x => new EPageIndexViewModel()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Title = x.Title,

                    ReleaseYear = x.ReleaseYear,
                    ReleaseNumber = x.ReleaseNumber,
                    IsTranslation = x.IsTranslation,
                    Description = x.Description,
                    Notes = x.Notes,

                    UploaderId = x.UploaderId,               
                    CategoryId = x.CategoryId,               
                    AuthorId = x.AuthorId,    

                    AuthorNames = null,     // TODO: Revise, do we really need this?                    
                    UploaderNames = null,   // TODO: Revise, do we really need this?
                    CategoryName = x.Category.Name,

                    DateCreated = x.DateCreated

                }).GroupBy(x => x.CategoryName).ToDictionaryAsync(x => x.Key, group => group.ToList());
            
            return new EPageViewModel()
            {
                CurrentYear = year.Value,
                EPagesByCategory = epages
            };
        }

        public static async Task<CategoryEPagesViewModel> GetEPagesByCategoryAsync(this DbSet<StranitzaEPage> dbSet, int categoryId,
            int? pageIndex, string sortPropertyName, SortOrder sortOrder, int pageSize = 10)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var query = dbSet                
                .Include(x => x.Author)
                .Include(x => x.Uploader)
                .Where(x => x.CategoryId == categoryId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sortPropertyName))
            {
                query = query.OrderBy(sortPropertyName, sortOrder);
            }

            var count = query.Count();
            var epages = await query           
                .Select(x => new EPageIndexViewModel()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Title = x.Title,

                    ReleaseYear = x.ReleaseYear,
                    ReleaseNumber = x.ReleaseNumber,
                    IsTranslation = x.IsTranslation,
                    Description = x.Description,
                    Notes = x.Notes,

                    UploaderId = x.UploaderId,
                    CategoryId = x.CategoryId,
                    AuthorId = x.AuthorId,

                    AuthorNames = x.Author != null ? x.Author.UserName : null,
                    UploaderNames = x.Uploader.Names + $" ({x.Uploader.UserName})",                    

                    DateCreated = x.DateCreated

                }).Skip((pageIndex.Value - 1) * pageSize).Take(pageSize).ToListAsync();

            return new CategoryEPagesViewModel(count, pageIndex.Value, pageSize)
            {                
                Records = epages
            };
        }

        public static async Task<EPageDeleteViewModel> GetEPageForDeleteAsync(this DbSet<StranitzaEPage> dbSet, int id)
        {
            return await dbSet
                .Include(x => x.Author)
                .Include(x => x.Uploader)
                // Maybe just only the id will do?
                //.Include(x => x.Source)   
                .Include(x => x.Category)
                .Select(x => new EPageDeleteViewModel()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Title = x.Title,
                    ReleaseYear = x.ReleaseYear,
                    ReleaseNumber = x.ReleaseNumber,
                    IsTranslation = x.IsTranslation,
                    Description = x.Description,

                    UploaderId = x.UploaderId,
                    UploaderUserName = x.Uploader.UserName,
                    CategoryId = x.CategoryId,                                        
                    SourceId = x.SourceId,
                    AuthorId = x.AuthorId,
                    AuthorUserName = x.AuthorId != null ? 
                        x.Author.UserName : null,

                    DateCreated = x.DateCreated                    

                }).SingleOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<EPageDetailsViewModel> GetEPageForDetailsAsync(this DbSet<StranitzaEPage> dbSet, int id)
        {
            return await dbSet
                .Include(x => x.Author)
                .Include(x => x.Uploader)
                // Maybe just only the id will do?
                //.Include(x => x.Source)   
                .Include(x => x.Category)
                .Select(x => new EPageDetailsViewModel()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Title = x.Title,
                    ReleaseYear = x.ReleaseYear,
                    ReleaseNumber = x.ReleaseNumber,
                    IsTranslation = x.IsTranslation,

                    CommentsCount = x.Comments.Count,

                    UploaderId = x.UploaderId,
                    UploaderNames = x.Uploader.Names,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    SourceId = x.SourceId,
                    AuthorId = x.AuthorId,
                    AuthorNames = x.AuthorId != null ?
                        x.Author.Names : null,
                                        
                    Notes = x.Notes,
                    Content = x.Content,
                    Description = x.Description,
                    DateCreated = x.DateCreated

                }).SingleOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<StranitzaEPage> CreateEPageAsync(this DbSet<StranitzaEPage> dbSet, EPageCreateViewModel vModel, string uploaderId)
        {
            var releaseNumber = 0;
            var year = DateTime.Now.Year;            

            if (dbSet.Any(x => x.ReleaseYear == year))
            {
                releaseNumber = dbSet.Where(x => x.ReleaseYear == year).Max(x => x.ReleaseNumber);
            }

            var entry = new StranitzaEPage()
            {
                FirstName = vModel.FirstName,
                LastName = vModel.LastName,
                Title = vModel.Title,
                CategoryId = vModel.CategoryId,
                Description = vModel.Description,
                Content = vModel.Content,
                Notes = vModel.Notes,
                IsTranslation = vModel.IsTranslation,
                
                ReleaseYear = year,
                ReleaseNumber = ++releaseNumber,

                UploaderId = uploaderId                
            };

            await dbSet.AddAsync(entry);

            return entry;
        }
    }
}
