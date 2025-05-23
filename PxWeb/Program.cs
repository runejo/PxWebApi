using System.Linq;
using System.Text;

using AspNetCoreRateLimit;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using Px.Abstractions.Interfaces;

using PxWeb.Code;
using PxWeb.Code.Api2;
using PxWeb.Code.Api2.Cache;
using PxWeb.Code.Api2.DataSelection;
using PxWeb.Code.Api2.DataSource;
using PxWeb.Code.Api2.NewtonsoftConfiguration;
using PxWeb.Code.Api2.SavedQueryBackend;
using PxWeb.Code.Api2.Serialization;
using PxWeb.Code.BackgroundWorker;
using PxWeb.Filters.Api2;
using PxWeb.Helper.Api2;
using PxWeb.Mappers;
using PxWeb.Middleware;


namespace PxWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var builder = WebApplication.CreateBuilder(args);

            // Only use Log4Net provider
            builder.Logging.ClearProviders();
            builder.Logging.AddLog4Net();

            // Add services to the container.
            Console.WriteLine("Starting!");

            // needed to load configuration from appsettings.json
            builder.Services.AddOptions();

            // needed to store rate limit counters and ip rules
            builder.Services.AddMemoryCache();

            //load general configuration from appsettings.json
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            builder.Services.AddInMemoryRateLimiting();

            // configuration (resolvers, counter key builders)
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            builder.Services.AddSingleton<IPxCache, PxCache>();
            builder.Services.AddSingleton<ILinkCreator, LinkCreator>();
            builder.Services.AddSingleton<ISelectionHandler, SelectionHandler>();
            builder.Services.AddSingleton<IPlacementHandler, PlacementHandler>();
            builder.Services.AddSingleton<IControllerStateProvider, ControllerStateProvider>();

            builder.Services.AddPxDataSource(builder);

            builder.Services.Configure<PxApiConfigurationOptions>(builder.Configuration.GetSection("PxApiConfiguration"));
            builder.Services.Configure<AdminProtectionConfigurationOptions>(builder.Configuration.GetSection("AdminProtection"));
            builder.Services.Configure<CacheMiddlewareConfigurationOptions>(builder.Configuration.GetSection("CacheMiddleware"));
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

            builder.Services.AddSavedQuery(builder);

            builder.Services.AddTransient<IDataWorkflow, DataWorkflow>();
            builder.Services.AddTransient<ISavedQueryBackendProxy, SavedQueryBackendProxy>();
            builder.Services.AddTransient<IPxApiConfigurationService, PxApiConfigurationService>();
            builder.Services.AddTransient<IAdminProtectionConfigurationService, AdminProtectionConfigurationService>();
            builder.Services.AddTransient<ICacheMiddlewareConfigurationService, CacheMiddlewareConfigurationService>();
            builder.Services.AddTransient<ILanguageHelper, LanguageHelper>();
            builder.Services.AddTransient<IFolderResponseMapper, FolderResponseMapper>();
            builder.Services.AddTransient<IDatasetMapper, DatasetMapper>();
            builder.Services.AddTransient<ITablesResponseMapper, TablesResponseMapper>();
            builder.Services.AddTransient<ITableResponseMapper, TableResponseMapper>();
            builder.Services.AddTransient<IPxHost, PxWebHost>();
            builder.Services.AddTransient<ISerializeManager, SerializeManager>();
            builder.Services.AddTransient<ICodelistMapper, CodelistMapper>();
            builder.Services.AddTransient<ICodelistResponseMapper, CodelistResponseMapper>();
            builder.Services.AddTransient<ISelectionResponseMapper, SelectionResponseMapper>();
            builder.Services.AddTransient<IDefaultSelectionAlgorithm, Bjarte3>();

            builder.Services.AddHostedService<LongRunningService>();
            builder.Services.AddSingleton<BackgroundWorkerQueue>();

            builder.Services.AddPxSearchEngine(builder);

            var langList = builder.Configuration.GetSection("PxApiConfiguration:Languages")
                .AsEnumerable()
                .Where(p => p.Value != null && p.Key.ToLower().Contains("id"))
                .Select(p => p.Value ?? "")
                .ToList();


            builder.Services.AddControllers(x =>
                x.Filters.Add(new LangValidationFilter(langList))
                )
                .AddNewtonsoftJson(opts =>
            {
                //opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                opts.SerializerSettings.ContractResolver = new BaseFirstContractResolver();
                opts.SerializerSettings.Converters.Add(new StringEnumConverter
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                });
                opts.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                opts.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ssZ"; // UTC
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            // builder.Services.AddEndpointsApiExplorer(); //only needed for minimal APIS according to
            // https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio
            builder.Services.AddSwaggerGen(c =>
            {
                // Sort endpoints
                c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}");
                c.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "PxWebApi",
                    Version = "v2-beta"
                }
                );
            });

            builder.Services.AddSwaggerGenNewtonsoftSupport();

            // Handle CORS configuration from appsettings.json
            bool corsEnbled = builder.Services.ConfigurePxCORS(builder);

            // Bind the configuration to the PxApiConfigurationOptions class
            var pxApiConfiguration = new PxApiConfigurationOptions();
            builder.Configuration.Bind("PxApiConfiguration", pxApiConfiguration);

            var app = builder.Build();

            app.UseMiddleware<GlobalRoutePrefixMiddleware>(pxApiConfiguration.RoutePrefix);
            app.UsePathBase(new PathString(pxApiConfiguration.RoutePrefix));



            app.UseSwagger(options =>
            {
                options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    if (!(pxApiConfiguration.EnableAllEndpointsSwaggerUI || app.Environment.IsDevelopment()))
                    {
                        swaggerDoc.Paths = RemoveAdminEndpoints(swaggerDoc.Paths);
                    }
                    swaggerDoc.Servers = Program.GetOpenApiServers(pxApiConfiguration.BaseURL, pxApiConfiguration.RoutePrefix);
                });
            });



            app.UseSwaggerUI(options =>
                {
                    options.RoutePrefix = string.Empty;
                    options.SwaggerEndpoint("swagger/v2/swagger.json", "PxWebApi 2.0-beta");
                });

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();

            if (corsEnbled)
            {
                app.UseCors();
                app.UseOptions();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseAuthorization();

                app.UseWhen(context => context.Request.Path.StartsWithSegments(pxApiConfiguration.RoutePrefix + "/admin") || context.Request.Path.StartsWithSegments("/admin"), appBuilder =>
                {
                    appBuilder.UseAdminProtectionIpWhitelist();
                    appBuilder.UseAdminProtectionKey();
                });
            }
            app.MapControllers();

            if (!app.Environment.IsDevelopment())
            {
                app.UseIpRateLimiting();
            }

            app.UseWhen(context => !(context.Request.Path.StartsWithSegments(pxApiConfiguration.RoutePrefix + "/admin") || context.Request.Path.StartsWithSegments("/admin")), appBuilder =>
            {
                appBuilder.UseCacheMiddleware();
            });

            app.Run();
        }

        private static OpenApiPaths RemoveAdminEndpoints(OpenApiPaths paths)
        {
            OpenApiPaths openApiPaths = [];
            foreach (var path in paths)
            {
                if (!path.Key.StartsWith("/admin"))
                {
                    openApiPaths.Add(path.Key, path.Value);
                }
            }
            return openApiPaths;
        }

        private static List<OpenApiServer> GetOpenApiServers(string pxApiConfiguration_BaseURL, string pxApiConfiguration_RoutePrepix)
        {
            var part1 = (new Uri(pxApiConfiguration_BaseURL)).PathAndQuery;
            if (part1.Equals("/"))
            {
                part1 = "";    // To aviod double /
            }

            return
            [
                new OpenApiServer { Url = part1 + pxApiConfiguration_RoutePrepix }
            ];

        }
    }
}
