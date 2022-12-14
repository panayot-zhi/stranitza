using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Serilog;
using stranitza.Models.Database;
using stranitza.Services;
using stranitza.Utility;

namespace stranitza
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // this is added by another method:
            // services.AddControllersWithViews
            //services.AddCors();

            services.AddDbContext<ApplicationDbContext>(
                contextLifetime: ServiceLifetime.Transient, optionsAction: options =>
                    options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));

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

             services.AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = Configuration.GetValue<string>("FacebookAppId");
                    options.AppSecret = Configuration.GetValue<string>("FacebookAppSecret");
                    options.Fields.Add("picture.width(150).height(150)");
                    options.ClaimActions.MapCustomJson(StranitzaClaimTypes.Picture, 
                        json => json.GetProperty("picture").GetProperty("data").GetString("url"));
                    //options.Events.OnCreatingTicket = OnCreatingTicket;
                })
                .AddTwitter(options =>
                {
                    options.ConsumerKey = Configuration.GetValue<string>("TwitterConsumerKey");
                    options.ConsumerSecret = Configuration.GetValue<string>("TwitterConsumerSecret");
                    options.ClaimActions.MapJsonKey(StranitzaClaimTypes.Picture, "profile_image_url_https");
                    //options.Events.OnCreatingTicket = OnCreatingTicket;
                    options.RetrieveUserDetails = true;
                })
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration.GetValue<string>("GoogleClientId");
                    options.ClientSecret = Configuration.GetValue<string>("GoogleClientSecret");
                    options.ClaimActions.MapJsonKey(StranitzaClaimTypes.Picture, "picture");
                    options.ClaimActions.MapJsonKey(StranitzaClaimTypes.VerifiedEmail, "verified_email");
                    //options.Events.OnCreatingTicket = OnCreatingTicket;
                });

            services.ConfigureExternalCookie(options =>
            {
                // .netCore.stranitza.identity.external
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

            services.ConfigureApplicationCookie(options =>
            {
                // .netCore.stranitza.identity
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

            // IOptions<T>
            services.AddOptions();

            services.AddRouting(option =>
            {
                option.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
                option.LowercaseUrls = true;
            });

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

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
            });

            /*services.AddSession(options =>
            {
                // Session cookie settings
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = false;
                options.Cookie.Name = ".netCore.stranitza.session";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });*/

            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                // .netCore.stranitza.tempData
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = false;
                options.Cookie.Name = ".netCore.stranitza.tempData";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddAntiforgery(options =>
            {
                // .netCore.stranitza.antiForgery
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".netCore.stranitza.antiForgery";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                // stick with the defaults
                // options.Cookie.Expiration = TimeSpan.FromMinutes(30);
            });

            services.AddTransient<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.AddTransient<IMailSender, MailSenderService>();

            services.AddTransient<NewsService>();
            services.AddTransient<StatisticService>();
            services.AddTransient<ELibraryService>();
            services.AddTransient<LibraryService>();
            services.AddTransient<IndexService>();

            Log.Logger.Information("Services added to the container and configured successfully.");
        }

        // NOTE: Use this for debugging purposes only
         
        /*private Task OnCreatingTicket(TwitterCreatingTicketContext arg)
        {
            return Task.CompletedTask;
        }

        private Task OnCreatingTicket(OAuthCreatingTicketContext arg)
        {
            return Task.CompletedTask;
        }*/

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            var cultureInfo = new CultureInfo("bg-BG");

            // NOTE: Soon...
            //cultureInfo.NumberFormat.CurrencySymbol = "???";

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (!env.IsDevelopment())
            {
                // The default HSTS value is 30 days.
                // You may want to change this for production
                // scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseResponseCaching();
            app.UseExceptionHandler("/Home/Error");
            app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

            PopulateDatabaseService.EnsureCriticalFilesAndFolders(Configuration);

            //app.UseHttpsRedirection();
            app.UseStaticFiles(); // wwwroot
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Configuration.GetValue<string>("RepositoryPath"), StranitzaConstants.UploadsFolderName)),
                RequestPath = new PathString($"/{StranitzaConstants.UploadsFolderName}")
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            PopulateDatabaseService.EnsureCriticalRoles(services).Wait();
            PopulateDatabaseService.EnsureCriticalUsers(services, Configuration).Wait();
            PopulateDatabaseService.LoadIssuesFromRootFolder(services, Configuration).Wait();
            //PopulateDatabaseService.LoadIndexFromRootFolder(services, Configuration).Wait();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });

            Log.Logger.Information("Configuration of the HTTP request pipeline successful.");
        }
    }
}
