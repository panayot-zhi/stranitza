using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using stranitza.Services;
using stranitza.Utility;

namespace stranitza
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var services = builder.Services;
            var environment = builder.Environment;
            var host = builder.Host;

            host.UseSerilog(ConfigureLogger);

            Log.Logger.Information("Adding features to the web application builder...");

            services.AddStranitzaDatabase(configuration);
            services.AddStranitzaIdentity();
            services.AddStranitzaAuthentication(configuration);
            services.AddStranitzaCookies();
            services.AddStranitzaServices();
            services.AddStranitzaCaching();

            services.AddRouting(option =>
            {
                option.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
                option.LowercaseUrls = true;
            });

            Log.Logger.Information("Configuring application...");

            services.ConfigureStranitza(configuration);

            var app = builder.Build();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (!environment.IsDevelopment())
            {
                // The default HSTS value is 30 days.
                // You may want to change this for production
                // scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseResponseCaching();
            app.UseExceptionHandler("/Home/Error");
            app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

            app.UseStranitzaFiles(configuration);

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // NOTE: Suspend this for now
            // using (var servicesScope = app.Services.CreateScope())
            // {
            //     var serviceProvider = servicesScope.ServiceProvider;
            //     PopulateDatabaseService.EnsureCriticalRoles(serviceProvider).Wait();
            //     PopulateDatabaseService.EnsureCriticalUsers(serviceProvider, configuration).Wait();
            //     PopulateDatabaseService.LoadIssuesFromRootFolder(serviceProvider, configuration).Wait();
            //     PopulateDatabaseService.LoadIndexFromRootFolder(serviceProvider, configuration).Wait();
            // }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });

            Log.Logger.Information("Application running...");

            app.Run();
        }

        private static void ConfigureLogger(HostBuilderContext hostBuilderContext, LoggerConfiguration loggerConfiguration)
        {
            // NOTE: Read configuration from appsettings.json
            loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration);
        }
    }
}
