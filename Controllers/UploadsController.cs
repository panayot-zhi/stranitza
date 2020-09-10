using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stranitza.Models.Database;

namespace stranitza.Controllers
{
    public class UploadsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UploadsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Page(int id, bool thumb = false)
        {
            var page = _context.StranitzaPages
                .Include(x => x.PageFile).SingleOrDefault(x => x.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            if (!page.IsAvailable)
            {
                return Forbid();
            }

            var file = page.PageFile;
            var path = thumb ? file.ThumbPath : file.FilePath;
            return new PhysicalFileResult(path, file.MimeType);
        }

        public IActionResult Avatar(int id)
        {
            var file = _context.StranitzaFiles
                .SingleOrDefault(x => x.Id == id);

            if (file == null)
            {
                return NotFound();
            }

            return new PhysicalFileResult(file.FilePath, file.MimeType);
        }
    }
}