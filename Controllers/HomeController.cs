﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Dynamic;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using stranitza.Repositories;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Utility;
using Newtonsoft.Json;
using Serilog;

namespace stranitza.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMailSender _mailSender;
        private readonly EmailSettings _emailSettings;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IMailSender mailSender, IOptions<EmailSettings> emailSettings,
            IWebHostEnvironment environment, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _mailSender = mailSender;
            _environment = environment;
            _configuration = configuration;
            _emailSettings = emailSettings.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(CacheProfileName = StranitzaCacheProfile.Monthly)]
        public async Task<IActionResult> About()
        {
            var filter = new UserFilterViewModel()
            {
                Type = UserFilterType.Editors
            };

            ViewBag.Editors = await _userManager.GetUsersPagedAsync(filter: filter, pageIndex: null);

            return View();
        }

        [ResponseCache(CacheProfileName = StranitzaCacheProfile.Monthly)]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(CacheProfileName = StranitzaCacheProfile.Monthly)]
        public IActionResult Cookies()
        {
            return View();
        }

        [ResponseCache(CacheProfileName = StranitzaCacheProfile.Monthly)]
        public IActionResult Help()
        {
            return View();
        }

        [ResponseCache(CacheProfileName = StranitzaCacheProfile.Monthly)]
        public IActionResult TaC()
        {
            return View();
        }

        [ResponseCache(CacheProfileName = StranitzaCacheProfile.Monthly)]
        public IActionResult FAQ()
        {
            return View();
        }

        [Ajax]
        public IActionResult Ping()
        {
            return Ok(DateTime.Now);
        }

        [ResponseCache(CacheProfileName = StranitzaCacheProfile.Yearly)]
        public IActionResult IssueNotFound()
        {
            return View("IssueNotFound");
        }

#if DEBUG

        [StranitzaAuthorize(StranitzaRoles.Administrator, andAbove: false)]
        public async Task<IActionResult> Test(string id)
        {
            id = id?.ToLowerInvariant();

            switch (id)
            {
                case "email":
                    {
                        return await TestSendEmail();
                    }
                case "challenge":
                    {
                        return Challenge();
                    }
                case "unauthorized":
                    {
                        return Unauthorized();
                    }
                case "notfound":
                    {
                        return NotFound();
                    }
                case "forbid":
                    {
                        return Forbid();
                    }
                case "forbidden":
                    {
                        return StatusCode(403);
                    }
                case "issuenotfound":
                    {
                        return RedirectToActionPreserveMethod("IssueNotFound");
                    }
                case "issuenotfoundlocal":
                    {
                        return LocalRedirectPreserveMethod("/Home/IssueNotFound");
                    }
                default:
                    {
                        var innerEx = new StranitzaException("StranitzaException.");
                        throw new Exception("Simulated exception.", innerEx);
                    }
            }
        }

        private async Task<IActionResult> TestSendEmail()
        {
            try
            {
                const string templateName = "TestEmail";
                var to = _mailSender.EmailSettings.MailAdmin;
                var callbackUrl = Url.AbsoluteAction("Index", "Home");

                await _mailSender.SendMailAsync(to, "Коле, получи ли?", templateName,
                    new
                    {
                        ButtonLink = callbackUrl
                    });

                dynamic viewModel = new ExpandoObject();

                viewModel.ButtonLink = callbackUrl;

                viewModel.Logo = _mailSender.GetLogoUri();
                viewModel.PreviewEmailLink = _mailSender.GetEmailPreviewLink(templateName, viewModel);

                return View("~/Views/Emails/TestEmail.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Възникна грешка при изпращането на EMAIL. {ex.GatherInternals()} {ex}");
            }
        }

#endif

        public IActionResult Email(string id, string data)
        {
            var base64EncodedBytes = Convert.FromBase64String(data);
            var jsonViewModel = Encoding.UTF8.GetString(base64EncodedBytes);
            dynamic viewModel = JsonConvert.DeserializeObject(jsonViewModel);

            viewModel.PreviewEmailLink = "#";

            ViewBag.Local = true;

            return View($"~/Views/Emails/{id}.cshtml", viewModel);
        }

        [ResponseCache(CacheProfileName = "NoCache")]
        public IActionResult Image(string id)
        {
            var file = _environment.WebRootFileProvider.GetFileInfo($"images/{id}");
            if (file == null || !file.Exists)
            {
                return StatusCode(404);
            }

            var userAgent = StranitzaExtensions.GetUserAgent(HttpContext);
            var clientIp = StranitzaExtensions.GetIp(HttpContext);

            Log.Logger.Information($"Serving image '{id}' to {{{userAgent}}} at {clientIp}.");

            var mimeType = StranitzaExtensions.GetMimeType(file.Name);
            return new PhysicalFileResult(file.PhysicalPath, mimeType);
        }

        [ResponseCache(CacheProfileName = "NoCache")]
        [StranitzaAuthorize(StranitzaRoles.Administrator, andAbove: false)]
        public async Task<IActionResult> Logs(bool zipFiles = false, bool preview = false)
        {
            var serilogPath = _configuration["Serilog:WriteTo:0:Args:path"];
            var logDirectory = Path.GetDirectoryName(serilogPath);
            if (!Directory.Exists(logDirectory))
            {
                return NotFound();
            }

            var filePaths = Directory.GetFiles(logDirectory);

            if (zipFiles)
            {
                using (var memory = new MemoryStream())
                {
                    using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, false, Encoding.UTF8))
                    {
                        foreach (var filePath in filePaths)
                        {
                            var fileInfo = new FileInfo(filePath);
                            var file = await ReadLogFileAsync(filePath);
                            zip.Add(file, fileInfo.Name);
                        }
                    }

                    var zipFile = memory.ToArray();
                    return new FileContentResult(zipFile, "application/octet-stream")
                    {
                        FileDownloadName = $"output-{DateTime.Now:yyyyMMdd-HHmmss}.zip"
                    };
                }
            }

            var lastOutputPath = filePaths.OrderByDescending(x => x).First();
            var lastOutputFile = await ReadLogFileAsync(lastOutputPath);

            if (preview)
            {
                return new FileContentResult(lastOutputFile, "text/plain");
            }

            return new FileContentResult(lastOutputFile, "application/octet-stream")
            {
                FileDownloadName = $"output-{DateTime.Now:yyyyMMdd-HHmmss}.log"
            };
        }

        private async Task<byte[]> ReadLogFileAsync(string filePath)
        {
            // NOTE: FileShare should be for Write also, or else we cannot access the log file
            await using (var fileStream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                await using (var memory = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memory);

                    return memory.ToArray();
                }
            }

        }

        [ResponseCache(CacheProfileName = "NoCache")]
        public IActionResult Error(string code)
        {
            if (Request.IsAjax())
            {
                if ("401".Equals(code))
                {
                    if (!User.Identity.IsAuthenticated)
                    {
                        return Challenge();
                    }
                }

                if (int.TryParse(code, out var candidate))
                {
                    return StatusCode(candidate);
                }

                Log.Logger.Warning($"Processed an AJAX error with an unknown status code ({code}). Returning 500: Internal Server Error");

                return StatusCode(500);
            }

            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (code)
            {
                case "400":
                    {
                        return View("BadRequest");
                    }
                case "401":
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            return Unauthorized();
                        }

                        return Challenge();
                    }
                case "403":
                    {
                        return View("Forbidden");
                    }
                case "404":
                    {
                        var path = "Could not resolve path.";
                        if (statusCodeReExecuteFeature != null)
                        {
                            path = statusCodeReExecuteFeature.GetFullPath();
                            ViewData["Path"] = path;
                        }

                        Log.Logger.Warning("404 NotFound: " + path);

                        // TODO: List here any cases where we need response body brevity
                        var acceptHeaders = Request.Headers["Accept"];
                        if (acceptHeaders.Any(header => header.Contains("image/*")))
                        {
                            return StatusCode((int)HttpStatusCode.NotFound, path);
                        }

                        return View("NotFound");
                    }
            }

            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (string.IsNullOrEmpty(code))
            {
                code = "500"; // internal server error
            }

            var errorViewModel = GetErrorViewModel(exceptionHandlerPathFeature, code);

            return View(errorViewModel);
        }

        protected ErrorViewModel GetErrorViewModel(IExceptionHandlerPathFeature exceptionHandlerPathFeature, string code)
        {
            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            var errorViewModel = new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                StatusCode = code
            };

            if (statusCodeReExecuteFeature != null)
            {
                errorViewModel.Path = statusCodeReExecuteFeature.GetFullPath();
            }
            else if (exceptionHandlerPathFeature != null)
            {
                errorViewModel.Path = exceptionHandlerPathFeature.Path;
                var error = exceptionHandlerPathFeature.Error;
                if (error != null)
                {
                    errorViewModel.Error = error.GatherInternals();
                    errorViewModel.ShortMessage = error.Message;
                    errorViewModel.StackTrace = error.StackTrace;
                }
            }

            return errorViewModel;
        }

        public IActionResult Search(string q)
        {
            return RedirectToAction("Search", "Sources", new { q });
        }
    }
}
