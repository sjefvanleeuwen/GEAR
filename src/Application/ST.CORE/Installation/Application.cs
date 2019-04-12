using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ST.Configuration.Seed;
using ST.CORE.Extensions;
using ST.CORE.ViewModels.InstallerModels;
using ST.DynamicEntityStorage;
using ST.DynamicEntityStorage.Abstractions;
using ST.Entities.Data;
using ST.Entities.Extensions;
using ST.Entities.Services;
using ST.Identity.Abstractions;
using ST.Identity.Data;
using ST.Identity.Data.MultiTenants;
using ST.Procesess.Data;

namespace ST.CORE.Installation
{
	public static class Application
	{
		/// <summary>
		/// Get file path
		/// </summary>
		public static string AppSettingsFilepath(IHostingEnvironment hostingEnvironment)
		{
			var path = "appsettings.json";
			if (hostingEnvironment.IsDevelopment())
			{
				path = "appsettings.Development.json";
			}
			else if (hostingEnvironment.IsEnvironment("Stage"))
			{
				path = "appsettings.Stage.json";
			}
			return path;
		}

		/// <summary>
		/// Get settings
		/// </summary>
		public static AppSettingsModel.RootObject Settings(IHostingEnvironment hostingEnvironment)
		{
			try
			{
				using (var r = new StreamReader(AppSettingsFilepath(hostingEnvironment)))
				{
					var json = r.ReadToEnd();
					return JsonConvert.DeserializeObject<AppSettingsModel.RootObject>(json);
				}
			}
			catch
			{
				return default;
			}
		}


		/// <summary>
		/// Run
		/// </summary>
		/// <param name="args"></param>
		internal static void MigrateAndRun(string[] args)
		{
			BuildWebHost(args)
				.Migrate()
				.Run();
		}

		/// <summary>
		/// Init migrations
		/// </summary>
		/// <param name="args"></param>
		public static void InitMigrations(string[] args)
		{
			BuildWebHost(args)
				.Migrate();
		}

		/// <summary>
		/// Migrate Web host extension
		/// </summary>
		/// <param name="webHost"></param>
		/// <returns></returns>
		private static IWebHost Migrate(this IWebHost webHost)
		{
			webHost.MigrateDbContext<EntitiesDbContext>((context, services) =>
				{
					var conf = services.GetService<IConfiguration>();
					EntitiesDbContextSeed.SeedAsync(context, conf, Configuration.Settings.TenantId)
						.Wait();
				})
				.MigrateDbContext<ProcessesDbContext>()
				.MigrateDbContext<PersistedGrantDbContext>()
				.MigrateDbContext<ApplicationDbContext>((context, services) =>
			   {
				   new ApplicationDbContextSeed()
					   .SeedAsync(context, services)
					   .Wait();
			   })
				.MigrateDbContext<ConfigurationDbContext>((context, services) =>
				{
					var config = services.GetService<IConfiguration>();
					var env = services.GetService<IHostingEnvironment>();
					var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
					IdentityServerConfigDbSeed.SeedAsync(context, applicationDbContext, config, env)
						.Wait();
				});

			return webHost;
		}

		/// <summary>
		/// Run application
		/// </summary>
		/// <param name="args"></param>
		public static void Run(string[] args)
		{
			DynamicService<EntitiesDbContext>.TenantId = Configuration.Settings.TenantId;
			BuildWebHost(args).Run();
		}

		/// <summary>
		/// Is system configured
		/// </summary>
		/// <param name="hostingEnvironment"></param>
		/// <returns></returns>
		public static bool IsConfigured(IHostingEnvironment hostingEnvironment)
		{
			var settings = Settings(hostingEnvironment);
			return settings != null && settings.IsConfigured;
		}

		/// <summary>
		/// Create dynamic tables
		/// </summary>
		/// <param name="tenantId"></param>
		/// <param name="schemaName"></param>
		public static void CreateDynamicTables(Guid tenantId, string schemaName = null)
		{
			var entitiesList = new List<EntitiesDbContextSeed.SeedEntity>
			{
				EntitiesDbContextSeed.ReadData(Path.Combine(AppContext.BaseDirectory, "SysEntities.json")),
				EntitiesDbContextSeed.ReadData(Path.Combine(AppContext.BaseDirectory, "CustomEntities.json")),
				EntitiesDbContextSeed.ReadData(Path.Combine(AppContext.BaseDirectory, "ProfileEntities.json"))
			};

			foreach (var item in entitiesList)
			{
				if (item.SynchronizeTableViewModels == null) continue;
				foreach (var ent in item.SynchronizeTableViewModels)
				{
					if (!IoC.Resolve<EntitiesDbContext>().Table.Any(s => s.Name == ent.Name && s.TenantId == tenantId))
					{
						IoC.Resolve<EntitySynchronizer>().SynchronizeEntities(ent, tenantId, schemaName);
					}
				}
			}

			//Seed EntityFrameWork entities
			var entities = TablesService.GetEntitiesFromDbContexts(typeof(ApplicationDbContext), typeof(EntitiesDbContext));

			foreach (var ent in entities)
			{
				if (!IoC.Resolve<EntitiesDbContext>().Table.Any(s => s.Name == ent.Name && s.TenantId == tenantId))
				{
					IoC.Resolve<EntitySynchronizer>().SynchronizeEntities(ent, tenantId, ent.Schema);
				}
			}
		}

		/// <summary>
		/// Create dynamic tables extension
		/// </summary>
		/// <param name="tenant"></param>
		public static void CreateDynamicTables(this Tenant tenant)
		{
			CreateDynamicTables(tenant.Id, tenant.MachineName);
		}

		/// <summary>
		/// Seed dynamic data 
		/// </summary>
		/// <returns></returns>
		public static async Task SeedDynamicDataAsync()
		{
			//Seed notifications types
			await EntitiesDbContextSeed.SeedNotificationTypesAsync();

			//Sync default menus
			await MenuManager.SyncMenuItemsAsync();

			//Sync web pages
			await PageManager.SyncWebPagesAsync();

			//Sync templates
			await TemplateManager.SeedAsync();

			//Sync nomenclatures
			await NomenclatureManager.SyncNomenclaturesAsync();
		}

		/// <summary>
		/// On application start
		/// </summary>
		/// <param name="app"></param>
		internal static async void OnApplicationStarted(IApplicationBuilder app)
		{
			using (var serviceScope = app.ApplicationServices
				.GetRequiredService<IServiceScopeFactory>()
				.CreateScope())
			{
				var env = serviceScope.ServiceProvider.GetService<IHostingEnvironment>();
				var context = serviceScope.ServiceProvider.GetService<EntitiesDbContext>();
				//var service = serviceScope.ServiceProvider.GetService<IDynamicService>();
				var isConfigured = Application.IsConfigured(env);

				if (isConfigured && context.Database.CanConnect())
				{
					var permissionService = serviceScope.ServiceProvider.GetService<IPermissionService>();
					await permissionService.RefreshCache();
					//await service.RegisterInMemoryDynamicTypes();
				}
			}
		}

		/// <summary>
		/// Build web host
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		private static IWebHost BuildWebHost(string[] args)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddEnvironmentVariables()
				.AddJsonFile("appsettings.json", optional: false)
				.Build();

			return WebHost.CreateDefaultBuilder(args)
				.UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
				.UseConfiguration(config)
				.StartLogging()
				.CaptureStartupErrors(true)
				.UseStartup<Startup>()
				.Build();
		}

	}
}
