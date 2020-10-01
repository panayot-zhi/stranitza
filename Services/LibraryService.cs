using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Utility;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using stranitza.Repositories;
using Image = SixLabors.ImageSharp.Image;

namespace stranitza.Services
{
    // TODO: Implement a mechanic for the current user to be able to download certain issues
    // TODO: Figure out a way to delete thumb pdf if all the pages are available

    public class LibraryService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IConfiguration _applicationConfiguration;

        public LibraryService(ApplicationDbContext applicationDbContext, IConfiguration configuration)
        {
            _applicationDbContext = applicationDbContext;
            _applicationConfiguration = configuration;
        }

        public async Task CreateIssueRecord(DirectoryInfo issueDirectory)
        {            
            var directoryNameInformation = issueDirectory.Name.Split('-');

            var issueReleaseYear = int.Parse(directoryNameInformation[0]);
            var issueReleaseNumber = int.Parse(directoryNameInformation[1]);

            var issue = _applicationDbContext.StranitzaIssues.SingleOrDefault(x =>
                x.ReleaseNumber == issueReleaseNumber & x.ReleaseYear == issueReleaseYear);

            if (issue != null)
            {
                Log.Logger.Information("Information for issue {IssueReleaseNumber}/{IssueReleaseYear} already exists in the database.",
                    issueReleaseNumber, issueReleaseYear);
                return;
            }

            Log.Logger.Information("Generating issue from folder information {IssueDirectory}.", issueDirectory);

            issue = new StranitzaIssue()
            {
                ReleaseYear = issueReleaseYear,
                ReleaseNumber = issueReleaseNumber,
                Pages = new List<StranitzaPage>()
            };

            await _applicationDbContext.AddAsync(issue);

            bool issueNumberResolved = false;
            foreach (var fileInfo in issueDirectory.GetFiles("*", SearchOption.TopDirectoryOnly))
            {
                if (fileInfo.Extension.ToUpperInvariant() == ".PDF")
                {
                    if (fileInfo.Name.StartsWith("_"))
                    {
                        // a redacted and reduced pdf file for download
                        issue.PdfFileReduced = await CreatePdfReducedFileRecord(fileInfo);

                        continue;
                    }

                    issue.PdfFilePreview = await CreatePdfFileRecord(fileInfo);
                    issue.AvailablePages = new[] { 1, 3 }; // immediately make the cover and index page available

                    CreateThumbPdfFile(issue);
                    AssignPdfPagesCount(issue);
                    //StampPdf(issue.PdfFile, issue.GetIssueTitle());

                    continue;
                }

                if (fileInfo.Extension.ToUpperInvariant() == ".ZIP")
                {
                    issue.ZipFile = await CreateZipFileRecord(fileInfo);

                    await _applicationDbContext.SaveChangesAsync();

                    continue;
                }

                // all other files should be images                

                var page = await CreatePageRecord(fileInfo, issue);
                //issue.Pages.Add(page);

                if (!issueNumberResolved)
                {
                    issue.IssueNumber = int.Parse(fileInfo.Name.Split('-')[1]);
                    issueNumberResolved = true;
                }
            }

            var imagePagesCount = issue.Pages.Count;

            if (issue.PagesCount == 0)
            {
                issue.PagesCount = imagePagesCount;
            }

            /*if (issueReleaseYear <= 2007)
            {
                // NOTE: All issues after that year should have pdf files
                // all gallery pages should be available by default
                issue.IsAvailable = true;
            }
                NOTE: All imported issues should be immediately available
            */

            issue.IsAvailable = true;

            var hasCoverPage = issue.CoverPage != null;            
            if (!hasCoverPage && imagePagesCount > 0)
            {
                var firstPage = issue.Pages.First();
                firstPage.Type = StranitzaPageType.Cover;                
            }

            var hasIndexPage = issue.IndexPage != null;
            if (!hasIndexPage && imagePagesCount > 1)
            {
                var secondPage = issue.Pages.Skip(1).First();
                secondPage.Type = StranitzaPageType.Index;                
            }

            var logIssueInformation = 
$@"
Resolved issue variables:
N. {issue.IssueNumber}
ReleaseNumber: {issue.ReleaseNumber}
ReleaseYear: {issue.ReleaseYear}
pdf: {issue.PdfFilePreview?.FilePath}
pdf (reduced): {issue.PdfFileReduced?.FilePath}
pages: {issue.PagesCount}
cover: {issue.CoverPage?.Id}
index: {issue.IndexPage?.Id}";

            Log.Logger.Debug(logIssueInformation);

            //await _applicationDbContext.AddAsync(issue);
            await _applicationDbContext.SaveChangesAsync();

            Log.Logger.Information("Issue generated successfully: #{IssueId}", issue.Id);
        }

        public async Task<StranitzaFile> CreateZipFileRecord(FileSystemInfo fileInfo)
        {
            var file = new StranitzaFile()
            {
                Title = fileInfo.Name.Replace(fileInfo.Extension, string.Empty),
                Extension = fileInfo.Extension.TrimStart('.'),
                MimeType = StranitzaExtensions.GetMimeType(fileInfo.Name),
                FilePath = fileInfo.FullName,
                FileName = fileInfo.Name
            };

            var thumbFilePath = Path.Combine(file.FilePath.Replace(file.FileName, string.Empty), 
                    StranitzaConstants.ThumbnailsFolderName, file.FileName);

            if (File.Exists(thumbFilePath))
            {
                file.ThumbPath = thumbFilePath;
            }

            await _applicationDbContext.AddAsync(file);
            await _applicationDbContext.SaveChangesAsync();

            return file;
        }

        public async Task<StranitzaIssue> SaveIssueRecord(IssueCreateViewModel vModel)
        {
            using (var transaction = await _applicationDbContext.Database.BeginTransactionAsync())
            {
                var entry = await _applicationDbContext.StranitzaIssues.CreateIssueAsync(vModel);

                await _applicationDbContext.SaveChangesAsync();

                if (vModel.PdfFile != null)
                {
                    var pdfFile = await SavePdfFile(vModel.PdfFile, entry.ReleaseYear, entry.ReleaseNumber, entry.IssueNumber);
                    var pdfFileRecord = await CreatePdfFileRecord(pdfFile);
                    entry.AvailablePages = new[] { 1, 3 }; // immediately make the cover and index page available
                    entry.PdfFilePreview = pdfFileRecord;

                    CreateThumbPdfFile(entry);
                    AssignPdfPagesCount(entry);
                    StampPdf(entry.PdfFilePreview, entry.GetIssueTitle());

                    var reducedPdfFile = CreateReducedPdfFile(entry);
                    entry.PdfFileReduced = await CreatePdfReducedFileRecord(reducedPdfFile);
                    StampPdf(entry.PdfFileReduced, entry.GetIssueTitle());
                }

                //await _applicationDbContext.SaveChangesAsync();

                entry.Pages = new List<StranitzaPage>();
                var pageFiles = vModel.PageFiles.OrderBy(x => x.FileName).ToArray();

                if (pageFiles.Length < 3)
                {
                    // if the uploaded pages were less than 3                                
                    // the auto-assigning feature won't assign cover and 
                    // index properly, assign them manually to page defaults
                    var firstFormFile = pageFiles.First();
                    var coverPage = await SavePageFile(firstFormFile, StranitzaConstants.DefaultCoverPageNumber, 
                        entry.ReleaseYear, entry.ReleaseNumber, entry.IssueNumber);
                    await CreatePageRecord(coverPage, entry);

                    var lastFormFile = pageFiles.Last();
                    var indexPage = await SavePageFile(lastFormFile, StranitzaConstants.DefaultIndexPageNumber,
                        entry.ReleaseYear, entry.ReleaseNumber, entry.IssueNumber);
                    await CreatePageRecord(indexPage, entry);
                }
                else
                {
                    for (var index = 0; index < pageFiles.Length; index++)
                    {
                        // don't use zero based index
                        var pageNumber = index + 1;
                        var formFile = pageFiles[index];
                        var pageFile = await SavePageFile(formFile, pageNumber, entry.ReleaseYear, entry.ReleaseNumber,
                            entry.IssueNumber);
                        var pageFileRecord = await CreatePageRecord(pageFile, entry);
                    }
                }

                await _applicationDbContext.SaveChangesAsync();

                // create zip from available pages
                var zipFileInfo = await CreateZipFiles(entry);
                entry.ZipFile = await CreateZipFileRecord(zipFileInfo);

                await _applicationDbContext.SaveChangesAsync();

                await AttachSourcesToIssue(entry);

                await transaction.CommitAsync();

                return entry;
            }
        }

        public async Task<StranitzaIssue> UpdateIssueRecord(IssueEditViewModel vModel)
        {
            using (var transaction = await _applicationDbContext.Database.BeginTransactionAsync())
            {
                var entry = await _applicationDbContext.StranitzaIssues.UpdateIssueAsync(vModel);

                await _applicationDbContext.SaveChangesAsync();

                var updateZipFile = false;
                if (vModel.PdfFile != null)
                {
                    // saving of pdf file overwrites the existing one (or creates new if it doesn't exist)
                    var pdfFile = await SavePdfFile(vModel.PdfFile, vModel.ReleaseYear, vModel.ReleaseNumber, vModel.IssueNumber);
                    if (!entry.PdfFilePreviewId.HasValue)
                    {
                        // if there wasn't a pdf file record, create it
                        var pdfFileRecord = await CreatePdfFileRecord(pdfFile);
                        entry.AvailablePages = new[] { 1, 3 }; // immediately make the cover and index page available
                        entry.PdfFilePreview = pdfFileRecord;

                        StampPdf(entry.PdfFilePreview, entry.GetIssueTitle());
                    }
                    else
                    {
                        // NOTE: DateCreated cannot be modified,
                        // but LastUpdated will reflect this change
                    }

                    CreateThumbPdfFile(entry);
                    AssignPdfPagesCount(entry);

                    // recreate the reduced pdf file also
                    var reducedPdfFile = CreateReducedPdfFile(entry);
                    if (!entry.PdfFileReducedId.HasValue)
                    {
                        // create the db file record only if it did not exist previously
                        // the FilePath will point to the replaced pdf if a record was present
                        entry.PdfFileReduced = await CreatePdfReducedFileRecord(reducedPdfFile);
                    }

                    // if there was a fileId previously
                    // we need to load it manually here
                    if (entry.PdfFileReduced == null)
                    {
                        entry.PdfFileReduced = await _applicationDbContext.StranitzaFiles.FindAsync(entry.PdfFileReducedId);
                    }

                    StampPdf(entry.PdfFileReduced, entry.GetIssueTitle());
                    updateZipFile = true;
                }
                else if (entry.PdfFilePreviewId.HasValue && vModel.AvailablePagesChanged)
                {
                    // if there is a pdf file associated with the entry
                    // and the available pages have been meddled with
                    // recreate the thumb pdf file
                    CreateThumbPdfFile(entry);

                    // recreate the reduced pdf file also
                    var reducedPdfFile = CreateReducedPdfFile(entry);
                    if (!entry.PdfFileReducedId.HasValue)
                    {
                        // create the db file record only if it did not exist previously
                        // the FilePath will point to the replaced pdf if a record was present
                        entry.PdfFileReduced = await CreatePdfReducedFileRecord(reducedPdfFile);
                    }

                    // if there was a fileId previously
                    // we need to load it manually here
                    if (entry.PdfFileReduced == null)
                    {
                        entry.PdfFileReduced = await _applicationDbContext.StranitzaFiles.FindAsync(entry.PdfFileReducedId);
                    }

                    StampPdf(entry.PdfFileReduced, entry.GetIssueTitle());
                    updateZipFile = true;
                }

                await _applicationDbContext.SaveChangesAsync();

                if (vModel.PageFiles != null)
                {
                    // calculate the last page number and append new pages after it
                    var pageNumber = entry.Pages.Max(x => x.PageNumber);
                    var pageFiles = vModel.PageFiles.OrderBy(x => x.FileName).ToArray();

                    foreach (var formFile in pageFiles)
                    {
                        pageNumber++;

                        var pageFile = await SavePageFile(formFile, pageNumber, entry.ReleaseYear, entry.ReleaseNumber,
                            entry.IssueNumber);
                        var pageFileRecord = await CreatePageRecord(pageFile, entry);
                    }

                    updateZipFile = true;
                }

                await _applicationDbContext.SaveChangesAsync();

                if (updateZipFile)
                {
                    // create zip from available pages always on update
                    var zipFileInfo = await CreateZipFiles(entry);
                    if (!entry.ZipFileId.HasValue)
                    {
                        // create the db file record only if it did not exist previously
                        // the FilePath will point to the replaced zip if a record was present
                        entry.ZipFile = await CreateZipFileRecord(zipFileInfo);
                    }
                }

                await _applicationDbContext.SaveChangesAsync();                

                transaction.Commit();

                return entry;
            }
        }

        public void UpdatePageFile(PageViewModel vModel)
        {
            var filePath = vModel.FilePath;
            var thumbPath = vModel.ThumbPath;
            
            var fileTitle = Path.GetFileNameWithoutExtension(filePath);
            var newFileTitle = fileTitle.Substring(0, fileTitle.Length - 3) + vModel.PageNumber.ToString().PadLeft(3, '0');
            var newFileName = Path.GetFileName(filePath).Replace(fileTitle, newFileTitle);

            var newFilePath = filePath.Replace(fileTitle, newFileTitle);
            var newThumbPath = thumbPath.Replace(fileTitle, newFileTitle);

            File.Move(filePath, newFilePath);            
            File.Move(thumbPath, newThumbPath);

            vModel.FilePath = newFilePath;
            vModel.ThumbPath = newThumbPath;
            vModel.FileName = newFileName;
            vModel.FileTitle = newFileTitle;
        }


        public async Task<StranitzaPage> CreatePageRecord(FileInfo fileInfo, StranitzaIssue issue)
        {
            Log.Logger.Information("Generating page from file information {FileInfo}.", fileInfo.FullName);

            var fileName = fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
            var fileINameInformation = fileName.Split('-');
            var pageNumber = int.Parse(fileINameInformation[2]);
            var pageFile = await CreatePageFileRecord(fileInfo);
            var hasCoverPage = issue.CoverPage != null;
            var hasIndexPage = issue.IndexPage != null;

            var page = new StranitzaPage()
            {
                Issue = issue,
                IsAvailable = true, // all image files are by the default - available
                PageFile = pageFile,
                PageNumber = pageNumber,
                SlideNumber = issue.Pages.Count // zero-based
            };

            // номер на страница 1 : страница с изображение на корицата
            // номер на страница 3 : страница със съдържание

            if (!hasCoverPage && pageNumber == StranitzaConstants.DefaultCoverPageNumber)
            {
                page.Type = StranitzaPageType.Cover;
            }
            else if (!hasIndexPage && pageNumber == StranitzaConstants.DefaultIndexPageNumber)
            {
                page.Type = StranitzaPageType.Index;
            }
            else
            {
                // although this is the default,
                // specify it explicitly
                page.Type = StranitzaPageType.Regular;
            }

            var logPageInformation =
$@"
Resolved page variables:
N. {page.PageNumber}
slide: {page.SlideNumber}
type: {page.Type}
img: {page.PageFile.FilePath}
thumb: {page.PageFile.ThumbPath}";

            Log.Logger.Debug(logPageInformation);

            await _applicationDbContext.AddAsync(page);

            return page;

        }

        public async Task<FileInfo> SavePdfFile(IFormFile formFile, int releaseYear, int releaseNumber, int issueNumber)
        {
            var fileExtension = formFile.Extension();
            var fileName = $"{releaseYear}-{issueNumber.ToString().PadLeft(3, '0')}";
            var rootFolderPath = Path.Combine(_applicationConfiguration["RepositoryPath"], StranitzaConstants.IssuesFolderName);
            var directoryPath = Path.Combine(rootFolderPath, releaseYear.ToString(), $"{releaseYear}-{releaseNumber}");
            var filePath = Path.Combine(directoryPath, $"{fileName}.{fileExtension}");

            Directory.CreateDirectory(directoryPath);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }

            return new FileInfo(filePath);
        }

        public async Task<StranitzaFile> CreatePdfFileRecord(FileInfo fileInfo)
        {
            var thumbDirectory = Path.Combine(fileInfo.DirectoryName, StranitzaConstants.ThumbnailsFolderName);
            if (!Directory.Exists(thumbDirectory))
            {
                Directory.CreateDirectory(thumbDirectory);
            }

            var thumbPath = Path.Combine(thumbDirectory, fileInfo.Name);

            // NOTE: Do not create pdf thumb file here,
            // the method that creates it should also overwrite it 
            // if it already exists

            var file = new StranitzaFile()
            {
                Title = fileInfo.Name.Replace(fileInfo.Extension, string.Empty),
                Extension = fileInfo.Extension.TrimStart('.'),
                MimeType = StranitzaExtensions.GetMimeType(fileInfo.Name),
                FilePath = fileInfo.FullName,
                FileName = fileInfo.Name,
                ThumbPath = thumbPath
            };

            await _applicationDbContext.AddAsync(file);
            await _applicationDbContext.SaveChangesAsync();

            return file;
        }

        public async Task<StranitzaFile> CreatePdfReducedFileRecord(FileInfo fileInfo)
        {
            var file = new StranitzaFile()
            {
                Title = fileInfo.Name.Replace(fileInfo.Extension, string.Empty),
                Extension = fileInfo.Extension.TrimStart('.'),
                MimeType = StranitzaExtensions.GetMimeType(fileInfo.Name),
                FilePath = fileInfo.FullName,
                FileName = fileInfo.Name,
                ThumbPath = null
            };

            await _applicationDbContext.AddAsync(file);
            await _applicationDbContext.SaveChangesAsync();

            return file;
        }


        public async Task<FileInfo> SavePageFile(IFormFile formFile, int pageNumber, int releaseYear, int releaseNumber, int issueNumber)
        {
            var fileExtension = formFile.Extension();
            var fileName = $"{releaseYear}-{issueNumber.ToString().PadLeft(3, '0')}-{pageNumber.ToString().PadLeft(3, '0')}";
            var rootFolderPath = Path.Combine(_applicationConfiguration["RepositoryPath"], StranitzaConstants.IssuesFolderName);
            var directoryPath = Path.Combine(rootFolderPath, releaseYear.ToString(), $"{releaseYear}-{releaseNumber}");
            var filePath = Path.Combine(directoryPath, $"{fileName}.{fileExtension}");

            Directory.CreateDirectory(directoryPath);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }

            var thumbDirectory = Path.Combine(directoryPath, StranitzaConstants.ThumbnailsFolderName);
            var thumbPath = Path.Combine(thumbDirectory, $"{fileName}.{fileExtension}");

            Directory.CreateDirectory(thumbDirectory);
            CreateThumbPageFile(filePath, thumbPath);

            return new FileInfo(filePath);
        }

        public void CreateThumbPageFile(string filePath, string thumbPath)
        {
            using (var image = Image.Load(filePath))
            {
                var width = 100;
                var height = 150;

                image.Mutate(x => x.Resize(width, height));
                image.Save(thumbPath);
            }
        }

        public FileInfo CreateReducedPdfFile(StranitzaIssue issue)
        {
            // NOTE: This method should replace an already existing reduced pdf file

            PdfReader pdfReaderTemp = null;
            PdfReader issuePdfReader = null;
            PdfCopy pdfWriter = null;
            Document document = null;

            try
            {
                var pdf = issue.PdfFilePreview;
                if (pdf == null)
                {
                    pdf = _applicationDbContext.StranitzaFiles.Find(issue.PdfFilePreviewId);
                }

                const string fileExtension = "pdf";
                var issueNumber = issue.IssueNumber;
                var releaseYear = issue.ReleaseYear;
                var releaseNumber = issue.ReleaseNumber;

                var fileName = $"_{releaseYear}-{issueNumber.ToString().PadLeft(3, '0')}";
                var rootFolderPath = Path.Combine(_applicationConfiguration["RepositoryPath"], StranitzaConstants.IssuesFolderName);
                var forbiddenPagePdfFilePath = Path.Combine(_applicationConfiguration["RepositoryPath"], StranitzaConstants.ForbiddenPagePdfFileName);
                var directoryPath = Path.Combine(rootFolderPath, releaseYear.ToString(), $"{releaseYear}-{releaseNumber}");
                var filePath = Path.Combine(directoryPath, $"{fileName}.{fileExtension}");

                Directory.CreateDirectory(directoryPath);

                var reducedPdfPages = ResolveAvailablePages(issue.AvailablePages, issue.PagesCount);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    document = new Document();
                    pdfWriter = new PdfCopy(document, stream);
                    pdfReaderTemp = new PdfReader(forbiddenPagePdfFilePath);
                    issuePdfReader = new PdfReader(pdf.FilePath);

                    var forbiddenPage = pdfWriter.GetImportedPage(pdfReaderTemp, 1);

                    document.Open();

                    foreach (var pageNumber in reducedPdfPages)
                    {
                        if (pageNumber > 0)
                        {
                            // add the corresponding page from the original pdf
                            var originalPage = pdfWriter.GetImportedPage(issuePdfReader, pageNumber);
                            pdfWriter.AddPage(originalPage);
                        }
                        else
                        {
                            // add the forbidden page
                            pdfWriter.AddPage(forbiddenPage);
                        }
                    }

                    document.Close();
                    pdfWriter.Close();
                    issuePdfReader.Close();
                    pdfReaderTemp.Close();
                }

                return new FileInfo(filePath);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error occured while trying to make a reduced pdf file at path {ThumbPath}",
                    issue.PdfFilePreview.ThumbPath);

                throw;
            }
            finally
            {
                document?.Close();
                pdfWriter?.Close();
                issuePdfReader?.Close();
                pdfReaderTemp?.Close();

            }
        }

        private static int[] ResolveAvailablePages(int[] availablePages, int allPagesCount)
        {
            var shouldSkip = false;
            var result = new List<int>();
            for (var i = 1; i <= allPagesCount; i++)
            {
                if (availablePages.Contains(i))
                {
                    result.Add(i);
                    shouldSkip = false;
                }
                else
                {
                    if (shouldSkip)
                    {
                        continue;
                    }
                    else
                    {
                        // marks forbidden page
                        result.Add(-1);
                        shouldSkip = true;
                    }
                }
            }

            return result.ToArray();
        }

        public void CreateThumbPdfFile(StranitzaIssue issue)
        {
            PdfReader pdfReaderTemp = null;
            PdfReader issuePdfReader = null;
            PdfStamper issuePdfStamper = null;

            try
            {
                var forbiddenPagePdfFilePath =
                    Path.Combine(_applicationConfiguration["RepositoryPath"], StranitzaConstants.ForbiddenPagePdfFileName);

                var pdf = issue.PdfFilePreview;
                if (pdf == null)
                {
                    pdf = _applicationDbContext.StranitzaFiles.Find(issue.PdfFilePreviewId);
                }

                using (var outputStream = new FileStream(pdf.ThumbPath, FileMode.Create))
                {
                    pdfReaderTemp = new PdfReader(forbiddenPagePdfFilePath);

                    issuePdfReader = new PdfReader(pdf.FilePath);
                    issuePdfStamper = new PdfStamper(issuePdfReader, outputStream);

                    for (int i = 1; i <= issuePdfReader.NumberOfPages; i++)
                    {
                        if (!issue.AvailablePages.Contains(i))
                        {
                            issuePdfStamper.ReplacePage(pdfReaderTemp, 1, i);
                        }
                    }

                    Log.Logger.Information("Thumb PDF file created at path {ThumbPath} successfully, closing readers.",
                        issue.PdfFilePreview.ThumbPath);

                    issuePdfStamper.Close();
                    issuePdfReader.Close();
                    pdfReaderTemp.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error occured while trying to make a thumb pdf file at path {ThumbPath}",
                    issue.PdfFilePreview.ThumbPath);
            }
            finally
            {
                issuePdfStamper?.Close();
                issuePdfReader?.Close();
                pdfReaderTemp?.Close();
            }
        }

        public async Task<FileInfo> CreateZipFiles(StranitzaIssue issue)
        {
            var zipFileName = $"{issue.ReleaseYear}-{issue.ReleaseNumber}.zip";
            var fileEntries = _applicationDbContext.StranitzaPages.Where(x => x.Issue == issue).Select(x => x.PageFile).ToList();
            var rootFolderPath = Path.Combine(_applicationConfiguration["RepositoryPath"], StranitzaConstants.IssuesFolderName);
            var issueFolderPath = Path.Combine(rootFolderPath, $"{issue.ReleaseYear}", $"{issue.ReleaseYear}-{issue.ReleaseNumber}");
            var zipThumbFilePath = Path.Combine(issueFolderPath, StranitzaConstants.ThumbnailsFolderName, zipFileName);
            var zipFilePath = Path.Combine(issueFolderPath, zipFileName);

            var zipFileEntries = new List<StranitzaFile>(fileEntries);
            var zipThumbFileEntries = new List<StranitzaFile>(fileEntries);

            var originalPdfEntry = await _applicationDbContext.StranitzaFiles.FindAsync(issue.PdfFilePreviewId);
            if (originalPdfEntry != null)
            {
                zipFileEntries.Add(originalPdfEntry);

                var reducedPdfEntry = await _applicationDbContext.StranitzaFiles.FindAsync(issue.PdfFileReducedId);
                if (reducedPdfEntry == null)
                {
                    // I sincerely think that this should not be done here

                    var reducedPdfFile = CreateReducedPdfFile(issue);
                    reducedPdfEntry = await CreatePdfReducedFileRecord(reducedPdfFile);

                    _applicationDbContext.StranitzaIssues.Attach(issue);
                    issue.PdfFileReduced = reducedPdfEntry;
                    await _applicationDbContext.SaveChangesAsync();

                    StampPdf(issue.PdfFileReduced, issue.GetIssueTitle());
                }

                zipThumbFileEntries.Add(reducedPdfEntry);
            }

            await CreateZipFile(zipFileEntries, zipFilePath);
            await CreateZipFile(zipThumbFileEntries, zipThumbFilePath);

            return new FileInfo(zipFilePath);
        }

        private static async Task<FileInfo> CreateZipFile(IEnumerable<StranitzaFile> fileEntries, string zipFilePath)
        {
            using (var memory = new MemoryStream())
            {
                using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, false, Encoding.UTF8))
                {
                    foreach (var fileEntry in fileEntries)
                    {
                        zip.Add(fileEntry.FilePath, fileEntry.FileName);
                    }

                    /*using (var outputFileStream = new FileStream(zipFilePath, FileMode.Create))
                    {
                        // revert memory position, begin seeking
                        memory.Seek(0, SeekOrigin.Begin);
                        await memory.CopyToAsync(outputFileStream);
                    }*/
                }

                // NOTE: Whenever this method is called it should
                // RECREATE the zip file, so use with caution

                await File.WriteAllBytesAsync(zipFilePath, memory.ToArray());
            }

            return new FileInfo(zipFilePath);
        }

        public async Task<StranitzaFile> CreatePageFileRecord(FileInfo fileInfo)
        {
            var thumbDirectory = Path.Combine(fileInfo.DirectoryName, StranitzaConstants.ThumbnailsFolderName);
            if (!Directory.Exists(thumbDirectory))
            {
                Directory.CreateDirectory(thumbDirectory);
            }

            var thumbPath = Path.Combine(thumbDirectory, fileInfo.Name);
            if (!File.Exists(thumbPath))
            {
                CreateThumbPageFile(fileInfo.FullName, thumbPath);
            }

            var file = new StranitzaFile()
            {
                Title = fileInfo.Name.Replace(fileInfo.Extension, string.Empty),
                Extension = fileInfo.Extension.TrimStart('.'),
                MimeType = StranitzaExtensions.GetMimeType(fileInfo.Name),
                FilePath = fileInfo.FullName,
                FileName = fileInfo.Name,
                ThumbPath = thumbPath
            };

            await _applicationDbContext.AddAsync(file);
            await _applicationDbContext.SaveChangesAsync();

            return file;
        }

        public async Task DeleteIssueFilesAndRecords(StranitzaIssue entry, IEnumerable<StranitzaFile> files)
        {
            var rootFolderPath = Path.Combine(_applicationConfiguration["RepositoryPath"], StranitzaConstants.IssuesFolderName);
            var directoryPath = Path.Combine(rootFolderPath, entry.ReleaseYear.ToString(), $"{entry.ReleaseYear}-{entry.ReleaseNumber}");

            if (!Directory.Exists(directoryPath))
            {
                Log.Logger.Warning($"Requested deletion of directory '{directoryPath}', but the directory does not exist!");
                return;
            }

            Directory.Delete(directoryPath, recursive: true);
            
            _applicationDbContext.StranitzaFiles.RemoveRange(files);
            
            if (entry.PdfFilePreviewId.HasValue)
            {
                var pdfFile = _applicationDbContext.StranitzaFiles.Single(x => x.Id == entry.PdfFilePreviewId);
                _applicationDbContext.StranitzaFiles.Remove(pdfFile);
            }

            await _applicationDbContext.SaveChangesAsync();

        }

        public async Task DeletePageFileAndRecords(int id)
        {
            var entry = await _applicationDbContext.StranitzaPages.FindAsync(id);
            var file = await _applicationDbContext.StranitzaFiles.FindAsync(entry.PageFileId);

            _applicationDbContext.StranitzaPages.Remove(entry);            

            await _applicationDbContext.SaveChangesAsync();

            _applicationDbContext.StranitzaFiles.Remove(file);

            await _applicationDbContext.SaveChangesAsync();

            File.Delete(file.FilePath);
            File.Delete(file.ThumbPath);
        }

        private static void AssignPdfPagesCount(StranitzaIssue issue)
        {
            PdfReader pdfReader = null;

            try
            {
                pdfReader = new PdfReader(issue.PdfFilePreview.FilePath);
                var numberOfPages = pdfReader.NumberOfPages;

                Log.Logger.Information("Pdf file found, number of pages resolved: {NumberOfPages}", numberOfPages);

                issue.PagesCount = numberOfPages;

                pdfReader.Close();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error occured while trying to read pdf file at path {PdfFilePath}",
                    issue.PdfFilePreview.FilePath);
            }
            finally
            {
                pdfReader?.Close();
            }
        }

        public async Task<bool> AttachIssueToSource(StranitzaSource source)
        {
            if (source.IssueId.HasValue)
            {
                Log.Logger.Warning($"Източникът ({source.Id}) вече е свързан с конкретен брой ({source.IssueId})!");
                
                return false;
            }

            if (source.EPageId.HasValue)
            {
                Log.Logger.Warning($"Източникът ({source.Id}) вече е свързан с е-страница ({source.EPageId})!");

                return false;
            }

            var issue = await _applicationDbContext.StranitzaIssues.SingleOrDefaultAsync(x =>
                x.ReleaseNumber == source.ReleaseNumber && x.ReleaseYear == source.ReleaseYear);

            if (issue != null)
            {
                _applicationDbContext.Attach(source);
                source.IssueId = issue.Id;

                return true;
            }

            return false;
        }

        public async Task<int> AttachSourcesToIssue(StranitzaIssue issue)
        {
            var sources = _applicationDbContext.StranitzaSources
                .Where(x => !x.EPageId.HasValue && x.ReleaseNumber == issue.ReleaseNumber && x.ReleaseYear == issue.ReleaseYear).ToList();

            if (!sources.Any())
            {
                return 0;
            }

            sources.ForEach(source =>
            {
                _applicationDbContext.Attach(source);
                source.IssueId = issue.Id;
            });

            return await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<byte[]> GetPreviewPdfForUser(ClaimsPrincipal user, StranitzaFile pdfEntry, bool thumb = false)
        {
            // NOTE: Raise level if necessary

            if (user.IsAtLeast(StranitzaRoles.Editor))
            {
                if (!thumb)
                {
                    return await File.ReadAllBytesAsync(pdfEntry.FilePath);
                }
            }

            return await File.ReadAllBytesAsync(pdfEntry.ThumbPath);
        }

        public async Task<FileContentResult> GetDownloadPdfForUser(ClaimsPrincipal user, StranitzaIssue issue, bool reduced = false)
        {
            byte[] content;
            string fileDownloadName;

            // NOTE: Raise level if necessary

            if (issue.PdfFullyAvailable)
            {
                // The pdf file is fully available, which means the reduced and original are identical
                var fullPdfFileEntry = await _applicationDbContext.StranitzaFiles.FindAsync(issue.PdfFilePreviewId);
                content = await GetPreviewPdfForUser(user, fullPdfFileEntry, false);
                fileDownloadName = fullPdfFileEntry.FileName;
            }
            else if (user.IsAtLeast(StranitzaRoles.Editor) && !reduced)
            {
                // User has requested the full, original pdf and has the rights to do so
                var fullPdfFileEntry = await _applicationDbContext.StranitzaFiles.FindAsync(issue.PdfFilePreviewId);
                content = await GetPreviewPdfForUser(user, fullPdfFileEntry, false);
                fileDownloadName = fullPdfFileEntry.FileName;
            }
            else
            {
                // User doesn't have the rights to access the full, original pdf or has requested the reduced one
                var reducedPdfFileEntry = await _applicationDbContext.StranitzaFiles.FindAsync(issue.PdfFileReducedId);
                if (reducedPdfFileEntry == null || !File.Exists(reducedPdfFileEntry.FilePath))
                {
                    var reducedPdfFile = CreateReducedPdfFile(issue);
                    reducedPdfFileEntry = await CreatePdfReducedFileRecord(reducedPdfFile);

                    _applicationDbContext.StranitzaIssues.Attach(issue);
                    issue.PdfFileReduced = reducedPdfFileEntry;
                    await _applicationDbContext.SaveChangesAsync();

                    StampPdf(issue.PdfFileReduced, issue.GetIssueTitle());
                }

                content = await File.ReadAllBytesAsync(reducedPdfFileEntry.FilePath);
                fileDownloadName = reducedPdfFileEntry.FileName;
            }

            return new FileContentResult(content, "application/octet-stream")
            {
                FileDownloadName = fileDownloadName
            };
        }

        public async Task DeleteZipAsync(StranitzaIssue issue)
        {
            var zipFileEntry = await _applicationDbContext.StranitzaIssues.Where(x => x.Id == issue.Id)
                .Select(x => x.ZipFile).SingleOrDefaultAsync();

            if (zipFileEntry == null)
            {
                throw new StranitzaException("Няма запис за такъв файл в базата данни за този брой.");
            }

            _applicationDbContext.StranitzaFiles.Remove(zipFileEntry);

            await _applicationDbContext.SaveChangesAsync();

            File.Delete(zipFileEntry.FilePath);
            File.Delete(zipFileEntry.ThumbPath);
        }

        public async Task<byte[]> GetZipForUser(ClaimsPrincipal user, StranitzaIssue issue, bool thumb = false)
        {
            var zipFileEntry = await _applicationDbContext.StranitzaIssues.Where(x => x.Id == issue.Id)
                .Select(x => x.ZipFile).SingleOrDefaultAsync();

            if (zipFileEntry == null)
            {
                // create zip from available pages
                var zipFileInfo = await CreateZipFiles(issue);
                if (!issue.ZipFileId.HasValue)
                {
                    _applicationDbContext.StranitzaIssues.Attach(issue);

                    // create the db file record only if it did not exist previously
                    // the FilePath will point to the replaced zip if a record was present
                    issue.ZipFile = await CreateZipFileRecord(zipFileInfo);

                    await _applicationDbContext.SaveChangesAsync();
                }
            }
            else
            {
                issue.ZipFile = zipFileEntry;
            }

            // NOTE: Raise level if necessary

            if (user.IsAtLeast(StranitzaRoles.Editor))
            {
                if (!thumb)
                {
                    return await File.ReadAllBytesAsync(issue.ZipFile.FilePath);
                }
            }

            return await File.ReadAllBytesAsync(issue.ZipFile.ThumbPath);
        }

        public void StampPdf(StranitzaFile pdfFile, string title = null)
        {
            // try stamp info in the pdf
            PdfReader reader = null;
            PdfStamper stamper = null;

            if (title == null)
            {
                title = pdfFile.FileName;
            }

            try
            {
                using (var memory = new MemoryStream())
                {
                    reader = new PdfReader(pdfFile.FilePath);
                    stamper = new PdfStamper(reader, memory);

                    /*
                        Add pdf meta data for this issue                        
                     */

                    var moreInfo = reader.Info;

                    moreInfo["Title"] = title;
                    moreInfo["Author"] = StranitzaConstants.PdfAuthor;
                    moreInfo["Subject"] = StranitzaConstants.PdfSubject;
                    moreInfo["Keywords"] = StranitzaConstants.PdfKeywords;

                    stamper.MoreInfo = moreInfo;

                    stamper.Close();
                    reader.Close();

                    // overwrite the existing file
                    File.WriteAllBytes(pdfFile.FilePath, memory.ToArray());
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error occured while trying to stamp the pdf with meta information ({pdfFile}).",
                    pdfFile);
            }
            finally
            {
                stamper?.Close();
                reader?.Close();
            }
        }
    }
}
