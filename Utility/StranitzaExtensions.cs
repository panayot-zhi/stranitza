using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Serilog;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;

namespace stranitza.Utility
{
    public static class StranitzaExtensions
    {
        #region Extensions

        public static string Join<T>(this IEnumerable<T> entry)
        {
            return ((T[])entry).Join();
        }

        public static string Join<T>(this T[] entry)
        {
            if (entry == null)
            {
                return null;
            }
            
            if (!entry.Any())
            {
                return string.Empty;
            }

            return string.Join(",", entry);
        }

        public static T[] Separate<T>(this string entry)
        {
            if (entry == null)
            {
                return null;
            }

            if (entry == string.Empty)
            {
                return Array.Empty<T>();
            }

            return entry.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => (T)Convert.ChangeType(x, typeof(T))).ToArray();
        }

        public static bool HasProperty(this EntityEntry entry, string property)
        {
            return entry.Properties.Any(x => x.Metadata.Name == property);
        }

        public static string Extension(this IFormFile formFile)
        {
            return GetFileExtension(formFile.FileName);
        }

        public static string Capitalize(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return source[0].ToString().ToUpper() + source.Substring(1);
        }

        public static string GetGallerySlide(this IUrlHelper url, int issueId, int slideNumber)
        {            
            return $"{url.Action("Details", "Issues", new { id = issueId })}#lg=1&slide={slideNumber}";
        }

        public static string GetPdfPage(this IUrlHelper url, int issueId, int pageNumber)
        {            
            return $"{url.Action("PreviewPdf", "Issues", new { id = issueId })}#page={pageNumber}";
        }

        public static string GetPostImage(this IUrlHelper url, string imageFileName, bool absolute = false)
        {
            var relativeUrl = url.Content($"~/{StranitzaConstants.UploadsFolderName}/{imageFileName}");
            if (!absolute)
            {
                return relativeUrl;
            }

            var absoluteUrl = new Uri(url.ActionContext.HttpContext.Request.GetDisplayUrl());
            return new Uri(absoluteUrl, relativeUrl).ToString();
        }

        public static string GatherInternals(this Exception ex, int introspectionLevel = 5)
        {
            if (ex == null)
            {
                return string.Empty;
            }

            if (introspectionLevel == 0)
            {
                return "...";
            }

            return ex.Message + " > " + GatherInternals(ex.InnerException, --introspectionLevel);            
        }

        public static string GetFullPath(this IStatusCodeReExecuteFeature me)
        {
            return $"{me.OriginalPathBase}{me.OriginalPath}{me.OriginalQueryString}";
        }

        public static bool IsAjax(this HttpRequest request, string httpVerb = "")
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request object is Null.");
            }

            if (!string.IsNullOrEmpty(httpVerb))
            {
                if (request.Method != httpVerb)
                {
                    return false;
                }
            }

            if (request.Headers != null)
            {
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            }

            return false;
        }

        /*public static void AddModalMessage(this ITempDataDictionary tempData, string message, ModalMessageType type)
        {
            AddModalMessage(tempData, message, type.ToString().ToLowerInvariant());
        }*/

        public static void AddModalMessage(this ITempDataDictionary tempData, string message, string type = null)
        {
            if (tempData == null)
            {
                Log.Logger.Warning("An attempt was made to store a modal message in TempData, but TempData is not enabled (null).");
                return;
            }

            var typeString = type ?? "info";
            var jsonMessage = JsonConvert.SerializeObject(new
            {
                content = HttpUtility.JavaScriptStringEncode(message),
                type = typeString
            });

            tempData["ModalMessage"] = jsonMessage;
        }

        public static void Add(this ZipArchive zip, byte[] file, string filename)
        {
            var zipItem = zip.CreateEntry(filename);
            /*using (var memory = new MemoryStream(file))*/
            using (var entryStream = zipItem.Open())
            using (var zipFileBinary = new BinaryWriter(entryStream))
            {
                zipFileBinary.Write(file);
                /*memory.CopyTo(entryStream);*/
            }
        }

        public static void Add(this ZipArchive zip, string filepath, string filename)
        {
            zip.Add(File.ReadAllBytes(filepath), filename);
        }

        public static string[] GatherErrors(this ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(x => x.Errors.Select(y => y.ErrorMessage)).ToArray();
        }

        public static void AddIdentityError(this ModelStateDictionary modelState, IdentityError error)
        {
            modelState.AddModelError(error.Code, error.Description);
        }

        public static void AddIdentityErrors(this ModelStateDictionary modelState, IEnumerable<IdentityError> errors)
        {
            foreach (var identityError in errors)
            {
                modelState.AddIdentityError(identityError);
            }
        }
        public static void AddIdentityErrors(this ITempDataDictionary tempData, IEnumerable<IdentityError> errors)
        {
            foreach (var identityError in errors)
            {
                tempData.AddModalMessage($"[{identityError.Code}]: {identityError.Description}", "danger");
            }
        }


        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Name);
        }

        public static string GetProfilePicture(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(StranitzaClaimTypes.Picture);
        }

        public static bool? GetVerifiedEmail(this ClaimsPrincipal user)
        {
            if (bool.TryParse(user.FindFirstValue(StranitzaClaimTypes.VerifiedEmail), out var result))
            {
                return result;
            }

            return null;
        }

        public static async Task<string> GetDisplayName(this UserManager<ApplicationUser> userManager, ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated)
            {
                return null;
            }

            var applicationUser = await userManager.GetUserAsync(user);
            return GetDisplayName(applicationUser);
        }

        public static string GetAvatarPath(this IUrlHelper urlHelper, ApplicationUser user, StranitzaAvatarType? avatarType = null)
        {
            if (avatarType == null)
            {
                avatarType = user.AvatarType;
            }

            switch (avatarType)
            {
                case StranitzaAvatarType.Gravatar:
                    return GetGravatarUrl(user.Email);
                case StranitzaAvatarType.Facebook:
                    return user.FacebookAvatarPath;
                case StranitzaAvatarType.Twitter:
                    return user.TwitterAvatarPath;
                case StranitzaAvatarType.Google:
                    return user.GoogleAvatarPath;
                case StranitzaAvatarType.Internal:
                    return string.IsNullOrEmpty(user.InternalAvatarPath) 
                        ? urlHelper.Content("~/images/default-user.png")
                        : urlHelper.Content(user.InternalAvatarPath);
                default:
                    return urlHelper.Content("~/images/default-user.png");
            }
        }

        /// <summary>
        /// бр. {releaseNumber}/{releaseYear} ({issueNumber})
        /// </summary>
        public static string GetIssueTitle(this StranitzaIssue issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"бр. {releaseNumber}/{issue.ReleaseYear} ({issue.IssueNumber})";
        }

        /// <summary>
        /// Брой {releaseNumber} / {releaseYear} ({issueNumber})
        /// </summary>
        public static string GetIssueTitlePrefixed(this IssueIndexViewModel issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"Брой {releaseNumber} / {issue.ReleaseYear} ({issue.IssueNumber})";
        }

        /// <summary>
        /// {releaseNumber} / {releaseYear} ({issueNumber})
        /// </summary>
        public static string GetIssueTitle(this IssueIndexViewModel issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber} / {issue.ReleaseYear} ({issue.IssueNumber})";
        }

        /// <summary>
        /// {releaseNumber}/{releaseYear} ({issueNumber})
        /// </summary>
        public static string GetIssueTitle(this IssueDetailsViewModel issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber}/{issue.ReleaseYear} ({issue.IssueNumber})";
        }

        /// <summary>
        /// {releaseNumber} ({issueNumber})
        /// </summary>
        public static string GetIssueTitleShort(this IssueDetailsViewModel issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber} ({issue.IssueNumber})";
        }

        /// <summary>
        /// {releaseNumber} / {releaseYear} г. ({issueNumber})
        /// </summary>
        public static string GetIssueTitleLong(this IssueDetailsViewModel issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber} / {issue.ReleaseYear} г. ({issue.IssueNumber})";
        }

        /// <summary>
        /// {releaseNumber} ({issueNumber})
        /// </summary>
        public static string GetIssueTitleShort(this IssueEditViewModel issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber} ({issue.IssueNumber})";
        }

        /// <summary>
        /// {releaseNumber} / {releaseYear} г. ({issueNumber})
        /// </summary>
        public static string GetIssueTitleLong(this IssueEditViewModel issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber} / {issue.ReleaseYear} г. ({issue.IssueNumber})";
        }

        /// <summary>
        /// {releaseNumber} / {releaseYear} ({issueNumber})
        /// </summary>
        public static string GetIssueTitle(this PageViewModel issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber} / {issue.ReleaseYear} ({issue.IssueNumber})";
        }

        /// <summary>
        /// {releaseNumber} ({issueNumber})
        /// </summary>
        public static string GetIssueTitleShort(this IssuePagesViewModel issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber} ({issue.IssueNumber})";
        }

        /// <summary>
        /// {releaseNumber} / {releaseYear} ({issueNumber})
        /// </summary>
        public static string GetIssueTitle(this IssuePagesViewModel issue)
        {
            var releaseNumber = issue.ReleaseNumber > 0 ? issue.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber} / {issue.ReleaseYear} ({issue.IssueNumber})";
        }

        /// <summary>
        /// {releaseNumber}/{releaseYear}
        /// </summary>
        public static string GetIssueTitleShort(this SourceIndexViewModel source)
        {
            var releaseNumber = source.ReleaseNumber > 0 ? source.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber}/{source.ReleaseYear}";
        }

        /// <summary>
        /// {releaseNumber}/{releaseYear}
        /// </summary>
        public static string GetIssueTitleShort(this SourceDetailsViewModel source)
        {
            var releaseNumber = source.ReleaseNumber > 0 ? source.ReleaseNumber.ToString() : "+";
            return $"{releaseNumber}/{source.ReleaseYear}";
        }

        /// <summary>
        /// Generates a fully qualified URL to an action method by using the specified action name, controller name and
        /// route values.
        /// </summary>
        /// <param name="url">The URL helper.</param>
        /// <param name="actionName">The name of the action method.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>The absolute URL.</returns>
        public static string AbsoluteAction(
            this IUrlHelper url,
            string actionName,
            string controllerName,
            object routeValues = null)
        {
            return url.Action(actionName, controllerName, routeValues, url.ActionContext.HttpContext.Request.Scheme);
        }

        /// <summary>
        /// Generates a fully qualified URL to the specified content by using the specified content path. Converts a
        /// virtual (relative) path to an application absolute path.
        /// </summary>
        /// <param name="url">The URL helper.</param>
        /// <param name="contentPath">The content path.</param>
        /// <returns>The absolute URL.</returns>
        public static string AbsoluteContent(
            this IUrlHelper url,
            string contentPath)
        {
            HttpRequest request = url.ActionContext.HttpContext.Request;
            return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content(contentPath)).ToString();
        }

        /// <summary>
        /// Generates a fully qualified URL to the specified route by using the route name and route values.
        /// </summary>
        /// <param name="url">The URL helper.</param>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>The absolute URL.</returns>
        public static string AbsoluteRouteUrl(
            this IUrlHelper url,
            string routeName,
            object routeValues = null)
        {
            return url.RouteUrl(routeName, routeValues, url.ActionContext.HttpContext.Request.Scheme);
        }
        
        public static async Task UpdateUserAvatarPathAsync(this UserManager<ApplicationUser> userManager, ExternalLoginInfo info)
        {
            var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            var principal = info.Principal;
            var profilePicture = principal.GetProfilePicture();

            switch (info.LoginProvider)
            {
                case "Facebook":
                    if (user.FacebookAvatarPath != profilePicture)
                    {
                        user.FacebookAvatarPath = profilePicture;
                        await userManager.UpdateAsync(user);
                    }
                    break;
                case "Twitter":
                    if (user.TwitterAvatarPath != profilePicture)
                    {
                        user.TwitterAvatarPath = profilePicture;
                        await userManager.UpdateAsync(user);
                    }
                    break;
                case "Google":
                    if (user.GoogleAvatarPath != profilePicture)
                    {
                        user.GoogleAvatarPath = profilePicture;
                        await userManager.UpdateAsync(user);
                    }
                    break;
                default:
                    throw new StranitzaException("The login provider is not supported.");
            }
        }

        public static void ClearExternalAvatarPath(this UserManager<ApplicationUser> userManager, ApplicationUser user, string loginProvider)
        {
            switch (loginProvider)
            {
                case "Facebook":
                    user.FacebookAvatarPath = null;
                    break;
                case "Twitter":
                    user.TwitterAvatarPath = null;
                    break;
                case "Google":
                    user.GoogleAvatarPath = null;
                    break;
                default:
                    throw new StranitzaException("The login provider is not supported.");
            }
        }

        public static AuthenticationScheme ByName(this IList<AuthenticationScheme> list, string name)
        {
            return list?.SingleOrDefault(x => x.Name == name);
        }

        public static AuthenticationScheme Facebook(this IList<AuthenticationScheme> list) { return list?.ByName("Facebook"); }

        public static AuthenticationScheme Twitter(this IList<AuthenticationScheme> list) { return list?.ByName("Twitter"); }

        public static AuthenticationScheme Google(this IList<AuthenticationScheme> list) { return list?.ByName("Google"); }

        public static UserLoginInfo ByName(this IList<UserLoginInfo> list, string name)
        {
            return list?.SingleOrDefault(x => x.LoginProvider == name);
        }

        public static UserLoginInfo Facebook(this IList<UserLoginInfo> list) { return list?.ByName("Facebook"); }

        public static UserLoginInfo Twitter(this IList<UserLoginInfo> list) { return list?.ByName("Twitter"); }

        public static UserLoginInfo Google(this IList<UserLoginInfo> list) { return list?.ByName("Google"); }

        #endregion

        #region Static

        public static PostEvictionCallbackRegistration RegisterDefaultPostEvictionCallback()
        {
            return new PostEvictionCallbackRegistration()
            {
                EvictionCallback = (key, value, reason, state) =>
                {
                    Log.Logger.Information("Cache item with the key {CacheItemKey} has been evicted from the cache: {EvictionReason}",
                        key, reason);
                }
            };
        }

        internal static string Md5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = md5.ComputeHash(bytes);

                var builder = new StringBuilder();
                foreach (var c in hash)
                {
                    builder.Append(c.ToString("X2"));
                }

                return builder.ToString();
            }
        }

        public static string GetDisplayName(ApplicationUser user)
        {
            string displayName;
            switch (user.DisplayNameType)
            {
                case StranitzaDisplayNameType.UserName:
                    displayName = user.UserName;
                    break;
                case StranitzaDisplayNameType.Names:
                    displayName = user.Names;
                    break;
                case StranitzaDisplayNameType.Anonymous:
                    displayName = "Анонимен";
                    break;
                case StranitzaDisplayNameType.NamesAndUserName:
                    displayName = $"{user.Names} ({user.UserName})";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (user.DisplayEmail)
            {
                displayName += $" <{user.Email}>";
            }

            return displayName;
        }

        public static string GetAnonymousGravatar(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                email = Guid.NewGuid().ToString();
            }

            return GetGravatarUrl(email) + "&f=y";
        }

        public static string GetGravatarUrl(string email)
        {
            return $"https://www.gravatar.com/avatar/{ Md5Hash(email).ToLowerInvariant() }?s=120&d=mp";
        }

        public static string StripHtmlTags(string html)
        {
            var plainText = html;

            plainText = Regex.Replace(plainText, "\r", string.Empty);
            plainText = Regex.Replace(plainText, "\n", string.Empty);
            plainText = Regex.Replace(plainText, "\t", string.Empty);
            plainText = Regex.Replace(plainText, "&.*?;", string.Empty);
            plainText = Regex.Replace(plainText, "<.*?>", "|");

            return plainText.Split("|", StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }

        public static string GetMimeType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        public static string GetIp(HttpContext httpContext)
        {
            var result = string.Empty;

            // first try to get IP address from the forwarded header
            if (httpContext.Request.Headers != null)
            {
                // the X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating IP address of a client
                // connecting to a web server through an HTTP proxy or load balancer

                var forwardedHeader = httpContext.Request.Headers["X-Forwarded-For"];
                if (!StringValues.IsNullOrEmpty(forwardedHeader))
                {
                    result = forwardedHeader.FirstOrDefault();
                }
            }

            // if this header not exists try get connection remote IP address
            if (string.IsNullOrEmpty(result) && httpContext.Connection.RemoteIpAddress != null)
            {
                result = httpContext.Connection.RemoteIpAddress.ToString();
            }

            return result;
        }

        public static string GetUserAgent(HttpContext httpContext)
        {
            return httpContext.Request.Headers["User-Agent"].ToString();
        }

        public static string GetFileExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            return fileName.Substring(fileName.LastIndexOf(".", StringComparison.Ordinal) + 1);
        }

        public static StranitzaAvatarType GetAvatarType(string loginProvider)
        {
            switch (loginProvider)
            {
                case "Facebook":
                    return StranitzaAvatarType.Facebook;
                case "Twitter":
                    return StranitzaAvatarType.Twitter;
                case "Google":
                    return StranitzaAvatarType.Google;
                default:
                    return StranitzaAvatarType.Default;
            }
        }

        #endregion
    }

    public static class QueryableExtensions
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName, SortOrder sortOrder, IComparer<object> comparer = null)
        {
            switch (sortOrder)
            {
                case SortOrder.Desc:
                    return CallOrderedQueryable(query, "OrderByDescending", propertyName, comparer);

                case SortOrder.Unknown:
                case SortOrder.Asc:

                default:
                    return CallOrderedQueryable(query, "OrderBy", propertyName, comparer);
            }
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string propertyName, SortOrder sortOrder, IComparer<object> comparer = null)
        {
            switch (sortOrder)
            {
                case SortOrder.Desc:
                    return CallOrderedQueryable(query, "ThenByDescending", propertyName, comparer);

                case SortOrder.Unknown:
                case SortOrder.Asc:

                default:
                    return CallOrderedQueryable(query, "ThenBy", propertyName, comparer);
            }
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "OrderBy", propertyName, comparer);
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "OrderByDescending", propertyName, comparer);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "ThenBy", propertyName, comparer);
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "ThenByDescending", propertyName, comparer);
        }

        /// <summary>
        /// Builds the Queryable functions using a TSource property name.
        /// </summary>
        public static IOrderedQueryable<T> CallOrderedQueryable<T>(this IQueryable<T> query, string methodName, string propertyName,
                IComparer<object> comparer = null)
        {
            var param = Expression.Parameter(typeof(T), "x");

            var body = propertyName.Split('.').Aggregate<string, Expression>(param, Expression.PropertyOrField);

            return comparer != null
                ? (IOrderedQueryable<T>)query.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new[] { typeof(T), body.Type },
                        query.Expression,
                        Expression.Lambda(body, param),
                        Expression.Constant(comparer)
                    )
                )
                : (IOrderedQueryable<T>)query.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new[] { typeof(T), body.Type },
                        query.Expression,
                        Expression.Lambda(body, param)
                    )
                );
        }
    }

}
