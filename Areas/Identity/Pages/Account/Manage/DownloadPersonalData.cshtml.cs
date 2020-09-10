using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using stranitza.Models.Database;

namespace stranitza.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;
        private readonly ApplicationDbContext _dbContext;

        public DownloadPersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<DownloadPersonalDataModel> logger, 
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> OnPostAsync(string type)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Няма потребител с този идентификационен номер '{_userManager.GetUserId(User)}'.");
            }

            user.AuthoredEPages = _dbContext.StranitzaEPages.Where(x => x.AuthorId == user.Id).ToList();
            //user.UploadedEPages = _dbContext.StranitzaEPages.Where(x => x.UploaderId == user.Id).ToList();
            user.Sources = _dbContext.StranitzaSources.Where(x => x.AuthorId == user.Id).ToList();
            user.Comments = _dbContext.StranitzaComments.Where(x => x.AuthorId == user.Id).ToList();
            //user.ModeratedComments = _dbContext.StranitzaComments.Where(x => x.ModeratorId == user.Id).ToList();
            //user.Posts = _dbContext.StranitzaPosts.Where(x => x.UploaderId == user.Id).ToList();

            var now = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data [timestamp: {TimeStamp}].", _userManager.GetUserId(User), now);

            // Only include personal data for download
            var personalData = new Dictionary<string, object>();
            var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString());
            }

            personalData.Add("AuthoredEPages", user.AuthoredEPages);
            personalData.Add("Sources", user.Sources);
            personalData.Add("Comments", user.Comments);

            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            };

            var personalDataJson = JsonConvert.SerializeObject(personalData, jsonSerializerSettings);

            Response.Headers.Add("Content-Disposition", $"attachment; filename={user.Id}-{now}.json");
            return new FileContentResult(Encoding.UTF8.GetBytes(personalDataJson), "application/json");
        }
    }
}
