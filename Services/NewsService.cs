using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using stranitza.Models.Database;
using stranitza.Utility;

namespace stranitza.Services
{
    public class NewsService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IConfiguration _applicationConfiguration;

        public NewsService(ApplicationDbContext context, IConfiguration configuration)
        {
            _applicationDbContext = context;
            _applicationConfiguration = configuration;
        }

        public async Task<StranitzaFile> SaveAndCreatePostImageFileRecord(IFormFile formFile)
        {
            var rootFolderPath = Path.Combine(_applicationConfiguration["RepositoryPath"], StranitzaConstants.UploadsFolderName);
            var fileName = StranitzaExtensions.Md5Hash(formFile.FileName + "-" + DateTime.Now).ToLowerInvariant();            
            var fileExtension = formFile.FileName.Split(".", StringSplitOptions.RemoveEmptyEntries).Last();
            var filePath = Path.Combine(rootFolderPath, fileName + "." + fileExtension);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }

            var file = new StranitzaFile()
            {
                Title = formFile.FileName.Replace("." + fileExtension, string.Empty),
                MimeType = formFile.ContentType,
                Extension = fileExtension,                
                FilePath = filePath,
                FileName = fileName,
                ThumbPath = null
            };

            await _applicationDbContext.AddAsync(file);
            await _applicationDbContext.SaveChangesAsync();

            return file;
        }

        public async Task DeletePostImageFileAndRecord(int id)
        {
            var file = await _applicationDbContext.StranitzaFiles.FindAsync(id);

            if (file == null)
            {
                throw new ArgumentException($"Could not find StranitzaFile with the id of {id}!");
            }

            File.Delete(file.FilePath);

            _applicationDbContext.StranitzaFiles.Remove(file);

            await _applicationDbContext.SaveChangesAsync();
        }
    }
}
