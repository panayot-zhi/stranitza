using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Repositories;
using stranitza.Utility;

namespace stranitza.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Ajax]
        [AllowAnonymous]
        public async Task<IActionResult> GetIssueComments(int? id, int? parentId, int? offset)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            return await GetComments(null,id, null, parentId, offset);
        }

        [Ajax]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostComments(int? id, int? parentId, int? offset)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            return await GetComments(id, null, null, parentId, offset);
        }

        [Ajax]
        [AllowAnonymous]
        public async Task<IActionResult> GetEPageComments(int? id, int? parentId, int? offset)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            return await GetComments(null, null, id, parentId, offset);
        }

        private async Task<IActionResult> GetComments(int? postId, int? issueId, int? epageId, 
            int? parentId, int? offset)
        {
            ApplicationUser user;

            // NOTE: Limit topLevel comments to 10
            // limit nested comments to 3
            var limit = parentId.HasValue ? 3 : 10;
            var topLevel = await _context.StranitzaComments.GetCommentsWrappedAsync(postId, issueId, epageId, parentId, limit, offset ?? 0);
            topLevel.CurrentUserDisplayName = await _userManager.GetDisplayName(User);

            foreach (var comment in topLevel.Comments)
            {
                // resolve display name for topLevel comment
                user = await _userManager.FindByIdAsync(comment.UploaderId);
                comment.UploaderDisplayName = StranitzaExtensions.GetDisplayName(user);
                comment.UploaderAvatarPath = StranitzaExtensions.GetAvatarPath(user);

                if (comment.ModeratorId != null)
                {
                    // resolve display name for topLevel comment moderator
                    user = await _userManager.FindByIdAsync(comment.ModeratorId);
                    comment.ModeratorDisplayName = StranitzaExtensions.GetDisplayName(user);
                }
            }

            if (parentId.HasValue)
            {
                // topLevel is second level when ParentId is present
                // break execution and return results
                return PartialView("_Comments", topLevel);
            }

            foreach (var parent in topLevel.Comments)
            {
                parent.Children = await _context.StranitzaComments.GetCommentsWrappedAsync(postId, issueId, epageId, parent.Id, 3, 0);

                foreach (var child in parent.Children.Comments)
                {
                    // resolve display names of nested comment
                    user = await _userManager.FindByIdAsync(child.UploaderId);
                    child.UploaderDisplayName = StranitzaExtensions.GetDisplayName(user);
                    child.UploaderAvatarPath = StranitzaExtensions.GetAvatarPath(user);

                    if (child.ModeratorId != null)
                    {
                        // resolve display name of nested comment moderator
                        user = await _userManager.FindByIdAsync(child.ModeratorId);
                        child.ModeratorDisplayName = StranitzaExtensions.GetDisplayName(user);
                    }
                }
            }

            return PartialView("_Comments", topLevel);
        }

        [Ajax]
        [HttpPost]
        [StranitzaAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommentViewModel vModel)
        {
            if (ModelState.IsValid)
            {
                var comment = await _context.StranitzaComments.CreateCommentAsync(vModel, User.GetUserId());

                await _context.SaveChangesAsync();

                vModel.Id = comment.Id;
                vModel.UploaderId = comment.AuthorId;
                vModel.DateCreated = comment.DateCreated;

                var applicationUser = await _userManager.GetUserAsync(User);
                vModel.UploaderDisplayName = StranitzaExtensions.GetDisplayName(applicationUser);
                vModel.UploaderAvatarPath = StranitzaExtensions.GetAvatarPath(applicationUser);

                return PartialView("_Comment", vModel);
            }

            return BadRequest();
        }

        [Ajax]
        [HttpPost]
        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Edit(CommentViewModel vModel)
        {
            var comment = await _context.StranitzaComments.UpdateCommentAsync(vModel, User.GetUserId());
            if (comment == null)
            {
                return NotFound();
            }

            await _context.SaveChangesAsync();

            // refill viewModel
            vModel.ParentId = comment.ParentId;
            vModel.UploaderId = comment.AuthorId;

            var uploader = await _userManager.FindByIdAsync(vModel.UploaderId);
            vModel.UploaderDisplayName = StranitzaExtensions.GetDisplayName(uploader);
            vModel.UploaderAvatarPath = StranitzaExtensions.GetAvatarPath(uploader);

            vModel.ModeratorId = comment.ModeratorId;

            var moderator = await _userManager.FindByIdAsync(vModel.ModeratorId);
            vModel.ModeratorDisplayName = StranitzaExtensions.GetDisplayName(moderator);

            vModel.Content = comment.Content;

            vModel.LastUpdated = comment.LastUpdated;
            vModel.DateCreated = comment.DateCreated;

            return PartialView("_Comment", vModel);
        }

        [Ajax]
        [HttpPost]
        [StranitzaAuthorize(StranitzaRoles.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            var comment = await _context.StranitzaComments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.StranitzaComments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}