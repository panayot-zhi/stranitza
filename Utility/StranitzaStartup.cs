using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Serilog;
using SixLabors.ImageSharp;
using stranitza.Models.Database;
using stranitza.Services;

namespace stranitza.Utility
{
    public static class StranitzaStartup
    {
        public static void AddStranitzaDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(
                contextLifetime: ServiceLifetime.Transient, optionsAction: options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        }

        public static void AddStranitzaIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            services.Configure<IdentityOptions>(options =>
            {
                // Store settings
                options.Stores.MaxLengthForKeys = 127;

                // Password Strength settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 4;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 7;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = StranitzaConstants.AllowedUserNameCharacters;

                // SignIn options
                options.SignIn.RequireConfirmedEmail = true;
            });

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
            });
        }

        public static void AddStranitzaAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = configuration.GetValue<string>("FacebookAppId");
                    options.AppSecret = configuration.GetValue<string>("FacebookAppSecret");
                    options.Fields.Add("picture.width(150).height(150)");
                    options.ClaimActions.MapCustomJson(StranitzaClaimTypes.Picture,
                        json => json.GetProperty("picture").GetProperty("data").GetString("url"));
                    //options.Events.OnCreatingTicket = OnCreatingTicket;
                })
                .AddTwitter(options =>
                {
                    options.ConsumerKey = configuration.GetValue<string>("TwitterConsumerKey");
                    options.ConsumerSecret = configuration.GetValue<string>("TwitterConsumerSecret");
                    options.ClaimActions.MapJsonKey(StranitzaClaimTypes.Picture, "profile_image_url_https");
                    //options.Events.OnCreatingTicket = OnCreatingTicket;
                    options.RetrieveUserDetails = true;
                })
                .AddGoogle(options =>
                {
                    options.ClientId = configuration.GetValue<string>("GoogleClientId");
                    options.ClientSecret = configuration.GetValue<string>("GoogleClientSecret");
                    options.ClaimActions.MapJsonKey(StranitzaClaimTypes.Picture, "picture");
                    options.ClaimActions.MapJsonKey(StranitzaClaimTypes.VerifiedEmail, "verified_email");
                    //options.Events.OnCreatingTicket = OnCreatingTicket;
                });
        }

        public static void AddStranitzaCookies(this IServiceCollection services)
        {
            // .netCore.stranitza.identity.external
            services.ConfigureExternalCookie(options =>
            {
                options.Cookie.HttpOnly = true;

                // no cookie policy enforces this
                // if you logged in - you gave consent
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".netCore.stranitza.identity.external";
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                // If the LoginPath is not set here,
                // ASP.NET Core will default to /Account/Login
                options.LoginPath = "/Identity/Account/Login";

                // If the LogoutPath is not set here,
                // ASP.NET Core will default to /Account/Logout
                options.LogoutPath = "/Identity/Account/Logout";

                // If the AccessDeniedPath is
                // not set here, ASP.NET Core 
                // will default to 
                // /Account/AccessDenied
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";

                // allow obtaining new ticket
                // on a new user activity
                // stick with the defaults
                // options.SlidingExpiration = true;
            });

            // .netCore.stranitza.identity
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;

                // no cookie policy enforces this
                // if you logged in - you gave consent
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".netCore.stranitza.identity";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                // This interferes with the remember me feature
                //options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

                // If the LoginPath is not set here,
                // ASP.NET Core will default to /Account/Login
                options.LoginPath = "/Identity/Account/Login";

                // If the LogoutPath is not set here,
                // ASP.NET Core will default to /Account/Logout
                options.LogoutPath = "/Identity/Account/Logout";

                // If the AccessDeniedPath is
                // not set here, ASP.NET Core 
                // will default to 
                // /Account/AccessDenied
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";

                // allow obtaining new ticket
                // on a new user activity
                // stick with the defaults
                // options.SlidingExpiration = true;

            });

            // .netCore.stranitza.tempData
            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = false;
                options.Cookie.Name = ".netCore.stranitza.tempData";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // .netCore.stranitza.antiForgery
            services.AddAntiforgery(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".netCore.stranitza.antiForgery";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                // stick with the defaults
                // options.Cookie.Expiration = TimeSpan.FromMinutes(30);
            });
        }

        public static void AddStranitzaCaching(this IServiceCollection services)
        {
            services.AddResponseCaching();

            services.AddControllersWithViews(options =>
            {
                options.CacheProfiles.Add(StranitzaCacheProfile.NoCache, new CacheProfile()
                {
                    Duration = 0,
                    Location = ResponseCacheLocation.None,
                    NoStore = true
                });

                options.CacheProfiles.Add(StranitzaCacheProfile.Hourly, new CacheProfile()
                {
                    Duration = 60 * 60 // 1 hour
                });

                options.CacheProfiles.Add(StranitzaCacheProfile.Weekly, new CacheProfile()
                {
                    Duration = 60 * 60 * 24 * 7 // 7 days
                });

                options.CacheProfiles.Add(StranitzaCacheProfile.Monthly, new CacheProfile()
                {
                    Duration = 60 * 60 * 24 * 30 // 30 days
                });

                options.CacheProfiles.Add(StranitzaCacheProfile.Yearly, new CacheProfile()
                {
                    Duration = 60 * 60 * 24 * 365 // 365 days
                });

            });
        }

        public static void AddStranitzaServices(this IServiceCollection services)
        {
            services.AddTransient<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            services.AddTransient<IMailSender, MailSenderService>();

            services.AddTransient<NewsService>();
            services.AddTransient<StatisticService>();
            services.AddTransient<ELibraryService>();
            services.AddTransient<LibraryService>();
            services.AddTransient<IndexService>();
        }

        public static void ConfigureStranitza(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            var cultureInfo = new CultureInfo("bg-BG");

            // NOTE: Soon...
            //cultureInfo.NumberFormat.CurrencySymbol = "€";

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }

        public static void UseStranitzaFiles(this IApplicationBuilder app, IConfiguration configuration)
        {
            var rootFolderPath = configuration["RepositoryPath"];

            if (!Directory.Exists(rootFolderPath))
            {
                throw new StranitzaException($"No directory found at {rootFolderPath}.");
            }

            /*var jsonFilePath = Path.Combine(configuration["RepositoryPath"], StranitzaConstants.IndexJsonFileName);
            if (!File.Exists(jsonFilePath))
            {
                Log.Logger.Warning("No json index file found at {JsonFilePath}.", jsonFilePath);
            }*/

            var forbiddenPagePdfFilePath = Path.Combine(rootFolderPath, StranitzaConstants.ForbiddenPagePdfFileName);
            if (!File.Exists(forbiddenPagePdfFilePath))
            {
                Log.Logger.Warning("No forbidden page pdf file found at {ForbiddenPagePdfFilePath}.", forbiddenPagePdfFilePath);
            }

            var issuesFolderPath = Path.Combine(rootFolderPath, StranitzaConstants.IssuesFolderName);
            if (!Directory.Exists(issuesFolderPath))
            {
                Directory.CreateDirectory(issuesFolderPath);
            }

            var uploadsFolderPath = Path.Combine(rootFolderPath, StranitzaConstants.UploadsFolderName);
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            app.UseStaticFiles(); // wwwroot
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(uploadsFolderPath),
                RequestPath = new PathString($"/{StranitzaConstants.UploadsFolderName}")
            });
        }
    }
}
