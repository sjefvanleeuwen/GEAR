using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ST.Cache.Abstractions;
using ST.Core;
using ST.Core.Helpers;
using ST.DynamicEntityStorage.Abstractions.Extensions;
using ST.Entities.Abstractions.Models.Tables;
using ST.Entities.Constants;
using ST.Entities.Data;
using ST.Entities.Services;
using ST.Entities.Services.Abstraction;
using ST.Entities.Settings;
using ST.Entities.Utils;
using ST.Entities.ViewModels.Table;
using ST.Identity.Abstractions;
using ST.Identity.Attributes;
using ST.Identity.Data;
using ST.Identity.Data.Permissions;
using ST.MultiTenant.Helpers;
using ST.MultiTenant.Services.Abstractions;
using ST.Notifications.Abstractions;

namespace ST.Cms.Controllers.Entity
{
	/// <inheritdoc />
	/// <summary />
	/// <summary>
	/// Forms manipulation
	/// </summary>
	public class TableController : BaseController
	{
		/// <summary>
		/// Inject logger
		/// </summary>
		private readonly ILogger<TableController> _logger;

		public TableController(IConfiguration configuration, EntitiesDbContext context, ApplicationDbContext applicationDbContext,
			UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, INotify<ApplicationRole> notify,
			IOrganizationService organizationService,
			ILogger<TableController> logger, IHostingEnvironment env, ICacheService cacheService)
			: base(context, applicationDbContext, userManager, roleManager, notify, organizationService, cacheService)
		{
			_logger = logger;
			ConnectionString = DbUtil.GetConnectionString(configuration, env);
		}

		private (DbProviderType, string) ConnectionString { get; set; }

		/// <summary>
		/// Create table
		/// </summary>
		/// <returns></returns>

		// GET: Users/Create
		[AuthorizePermission(PermissionsConstants.CorePermissions.BpmTableCreate)]
		public IActionResult Create()
		{
			var model = new CreateTableViewModel
			{
				EntityTypes = Context.EntityTypes.Where(x => !x.IsDeleted).ToList()
			};
			return View(model);
		}

		/// <summary>
		/// Get table dataService
		/// </summary>
		/// <returns></returns>
		[NonAction]
		private ITablesService GetSqlService()
		{
			return ConnectionString.Item1.Equals(DbProviderType.MsSqlServer) ?
															new TablesService()
															: ConnectionString.Item1.Equals(DbProviderType.PostgreSql)
															? new NpgTablesService()
															: null;
		}

		/// <summary>
		/// Create a table
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[AuthorizePermission(PermissionsConstants.CorePermissions.BpmTableCreate)]
		public async Task<IActionResult> Create(CreateTableViewModel model)
		{
			if (!ModelState.IsValid) return View(model);
			var entityType =
				await Context.EntityTypes.FirstOrDefaultAsync(x => x.Id == Guid.Parse(model.SelectedTypeId));
			if (entityType == null) return View(model);
			{
				var m = new CreateTableViewModel
				{
					Name = model.Name,
					EntityType = entityType.Name,
					Description = model.Description,
					TenantId = CurrentUserTenantId
				};
				try
				{
					var table = m.Adapt<TableModel>();
					await Context.Table.AddAsync(table);
					await Context.SaveChangesAsync();
					var sqlService = GetSqlService();
					var response = sqlService.CreateSqlTable(table, ConnectionString.Item2);
					if (response.Result)
						return RedirectToAction("Edit", "Table", new { id = table.Id, tab = "one" });
				}
				catch (Exception e)
				{
					ModelState.AddModelError("fail", e.Message);
					return View(model);
				}

				return View(model);
			}
		}

		/// <summary>
		/// List with entity
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[AuthorizePermission(PermissionsConstants.CorePermissions.BpmTableRead)]
		public IActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// List of tables
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult OrderList(DTParameters param)
		{
			var filtered = Context.Filter<TableModel>(param.Search.Value, param.SortOrder, param.Start,
				param.Length,
				out var totalCount);


			var orderList = filtered.Select(o => new TableModel
			{
				Id = o.Id,
				Name = o.Name,
				IsDeleted = o.IsDeleted,
				Author = o.Author,
				Created = o.Created,
				ModifiedBy = o.ModifiedBy,
				Description = o.Description,
				EntityType = o.EntityType,
				Changed = o.Changed,
				IsSystem = o.IsSystem,
				TableFields = o.TableFields
			});

			var finalResult = new DTResult<TableModel>
			{
				Draw = param.Draw,
				Data = orderList.ToList(),
				RecordsFiltered = totalCount,
				RecordsTotal = filtered.Count
			};

			return Json(finalResult);
		}

		/// <param name="id"></param>
		/// <param name="tab"></param>
		/// <returns></returns>
		[HttpGet]
		[AuthorizePermission(PermissionsConstants.CorePermissions.BpmTableUpdate)]
		public async Task<IActionResult> Edit(Guid id, string tab)
		{
			var table = await Context.Table.Include(x => x.TableFields).FirstOrDefaultAsync(x => x.Id == id);
			if (table == null) return NotFound();
			var model = table.Adapt<UpdateTableViewModel>();

			model.Groups = await Context.TableFieldGroups.Include(s => s.TableFieldTypes).ToListAsync();
			model.TabName = tab;
			return View(model);

		}

		/// <summary>
		/// Update table fields
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[AuthorizePermission(PermissionsConstants.CorePermissions.BpmTableUpdate)]
		public IActionResult Edit(UpdateTableViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				Context.Table.Update(model.Adapt<TableModel>());
				Context.SaveChanges();
				return RedirectToAction(nameof(Index), "Table");
			}
			catch
			{
				ModelState.AddModelError(string.Empty, "Something went wrong on server");
				return View(model);
			}
		}

		/// <summary>
		/// Change status is deleted in true
		/// </summary>
		/// <param name="tableId"></param>
		/// <returns></returns>
		[HttpDelete]
		public JsonResult RemoveTableModel(Guid tableId)
		{
			var response = new ResultModel
			{
				Errors = new List<IErrorModel>()
			};
			var table = Context.Table.FirstOrDefault(x => x.Id == tableId);
			if (table == null)
			{
				response.Errors.Add(new ErrorModel("fail", "Entity not found!"));
				return Json(response);
			}
			try
			{
				Context.Table.Remove(table);
				Context.SaveChanges();
				response.IsSuccess = true;
			}
			catch
			{
				response.Errors.Add(new ErrorModel("fail", "Fail to save data"));
			}
			return Json(response);
		}

		/// <summary>
		/// Get view for add field
		/// </summary>
		/// <param name="id"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[HttpGet]
		public async Task<IActionResult> AddField(Guid id, string type)
		{
			var entitiesList = await Context.Table.ToListAsync();
			var fieldType = await Context.TableFieldTypes.FirstOrDefaultAsync(x => x.Name == type.Trim());
			var fieldTypeConfig = Context.TableFieldConfigs.Where(x => x.TableFieldTypeId == fieldType.Id);
			var configurations = new List<FieldConfigViewModel>();
			foreach (var item in fieldTypeConfig)
			{
				if (item.Code != "9999")
					configurations.Add(new FieldConfigViewModel
					{
						Name = item.Name,
						Type = item.Type,
						ConfigId = item.Id,
						Description = item.Description,
						ConfigCode = item.Code
					});
			}

			var model = new CreateTableFieldViewModel
			{
				TableId = id,
				Configurations = configurations,
				TableFieldTypeId = fieldType.Id,
				DataType = fieldType.DataType,
				Parameter = type,
				EntitiesList = entitiesList.Select(x => x.Name).ToList()
			};

			return View(model);
		}

		/// <summary>
		/// Add new field to entity
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> AddField(CreateTableFieldViewModel field)
		{
			var entitiesList = await Context.Table.ToListAsync();
			var table = Context.Table.FirstOrDefault(x => x.Id == field.TableId);
			var tableName = table?.Name;
			var schema = table?.EntityType;
			field.EntitiesList = entitiesList.Select(x => x.Name).ToList();
			if (table == null)
			{
				ModelState.AddModelError(Guid.NewGuid().ToString(), "Table not found");
				return View(field);
			}
			var fieldTypeConfig = Context.TableFieldConfigs.Where(x => x.TableFieldTypeId == field.TableFieldTypeId).Select(item => new FieldConfigViewModel
			{
				Name = item.Name,
				Type = item.Type,
				ConfigId = item.Id,
				Description = item.Description,
				ConfigCode = item.Code
			}).ToList();

			if (field.Parameter == FieldType.EntityReference)
			{
				var foreignSchema = fieldTypeConfig.FirstOrDefault(x => x.ConfigCode == "9999");
				var foreignTable = Context.Table.FirstOrDefault(x => x.Name == field.Configurations.FirstOrDefault(y => y.Name == FieldConfig.ForeingTable).Value);
				if (foreignSchema != null)
				{
					if (foreignTable != null) foreignSchema.Value = foreignTable.EntityType;
					field.Configurations.Add(foreignSchema);
				}
			}

			var sqlService = GetSqlService();
			field = field.CreateSqlField(ConnectionString);
			var insertField = sqlService.AddFieldSql(field, tableName, ConnectionString.Item2, true, schema);
			// Save field model in the dataBase
			if (!insertField.Result)
			{
				ModelState.AddModelError(string.Empty, "Fail to apply changes to database!");
				return View(field);
			}
			var configValues = new List<TableFieldConfigValues>();
			var model = new TableModelFields
			{
				DataType = field.DataType,
				TableId = field.TableId,
				Description = field.Description,
				Name = field.Name,
				DisplayName = field.DisplayName,
				AllowNull = field.AllowNull,
				Synchronized = true,
				TableFieldTypeId = field.TableFieldTypeId,
			};
			foreach (var item in field.Configurations)
			{
				configValues.Add(new TableFieldConfigValues
				{
					TableFieldConfigId = item.ConfigId,
					TableModelFieldId = model.Id,
					Value = item.Value,
				});
			}

			model.TableFieldConfigValues = configValues;

			try
			{
				Context.TableFields.Add(model);
				Context.SaveChanges();
				return RedirectToAction("Edit", "Table", new { id = field.TableId, tab = "two" });
			}
			catch
			{
				ModelState.AddModelError(string.Empty, "Fail to apply changes to database!");
			}


			return View(field);
		}

		/// <summary>
		/// Get view for add fields
		/// </summary>
		/// <param name="type"></param>
		/// <param name="fieldId"></param>
		/// <returns></returns>
		[HttpGet]
		public async Task<IActionResult> EditField(Guid fieldId, Guid type)
		{
			var fieldType = await Context.TableFieldTypes.FirstOrDefaultAsync(x => x.Id == type);
			var fieldTypeConfig = Context.TableFieldConfigs.Where(x => x.TableFieldTypeId == fieldType.Id);
			// Clean Code
			var field = await Context.TableFields.FirstOrDefaultAsync(x => x.Id == fieldId);
			var configFields = Context.TableFieldConfigValues
				.Where(x => x.TableModelFieldId == fieldId)
				.Select(y => new FieldConfigViewModel
				{
					Name = fieldTypeConfig.Single(x => x.Id == y.TableFieldConfigId).Name,
					Type = fieldTypeConfig.Single(x => x.Id == y.TableFieldConfigId).Type,
					ConfigId = y.TableFieldConfigId,
					Description = fieldTypeConfig.Single(x => x.Id == y.TableFieldConfigId).Description,
					Value = y.Value
				}).ToList();

			var model = new CreateTableFieldViewModel
			{
				Id = fieldId,
				TableId = field.TableId,
				Configurations = configFields,
				TableFieldTypeId = field.TableFieldTypeId,
				DataType = field.DataType,
				Parameter = fieldType.Name,
				Name = field.Name,
				DisplayName = field.DisplayName,
				AllowNull = field.AllowNull,
				Description = field.Description
			};
			return View(model);
		}

		/// <summary>
		/// Create a field
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> EditField([Required]CreateTableFieldViewModel field)
		{
			var table = Context.Table.FirstOrDefault(x => x.Id == field.TableId);
			if (table == null) return NotFound();
			if (field.Id == Guid.Empty) return NotFound();
			switch (field.Parameter)
			{
				case FieldType.EntityReference:
					field.DataType = TableFieldDataType.Guid;
					break;
				case FieldType.Boolean:
					FieldConfigViewModel defaultBool = null;
					foreach (var c in field.Configurations)
					{
						if (c.Name != FieldConfig.DefaultValue) continue;
						defaultBool = c;
						break;
					}

					if (defaultBool?.Value != null && defaultBool.Value.Trim() == "on") defaultBool.Value = "1";
					if (defaultBool?.Value != null && defaultBool.Value.Trim() == "off") defaultBool.Value = "0";
					break;
				case FieldType.DateTime:
				case FieldType.Date:
				case FieldType.Time:
					FieldConfigViewModel defaultTime = null;
					foreach (var c in field.Configurations)
					{
						if (c.Name != FieldConfig.DefaultValue) continue;
						defaultTime = c;
						break;
					}

					if (defaultTime?.Value != null && defaultTime.Value.Trim() == "on")
						defaultTime.Value = "CURRENT_TIMESTAMP";
					if (defaultTime?.Value != null && defaultTime.Value.Trim() == "off") defaultTime.Value = null;
					break;
			}
			var fieldType = await Context.TableFieldTypes.FirstOrDefaultAsync(x => x.DataType == field.DataType);
			var fieldTypeConfig = Context.TableFieldConfigs.Where(x => x.TableFieldTypeId == fieldType.Id);
			field.Configurations = Context.TableFieldConfigValues
				.Where(x => x.TableModelFieldId == field.Id)
				.Select(y => new FieldConfigViewModel
				{
					Name = fieldTypeConfig.Single(x => x.Id == y.TableFieldConfigId).Name,
					Type = fieldTypeConfig.Single(x => x.Id == y.TableFieldConfigId).Type,
					ConfigId = y.TableFieldConfigId,
					Description = fieldTypeConfig.Single(x => x.Id == y.TableFieldConfigId).Description,
					Value = y.Value
				}).ToList();
			var sqlService = GetSqlService();
			var updateStructure = sqlService.AddFieldSql(field, table.Name, ConnectionString.Item2, false, table.EntityType);
			// Save field model structure in the dataBase
			if (updateStructure.IsSuccess)
			{
				var configValues = new List<TableFieldConfigValues>();
				var model = new TableModelFields
				{
					Id = field.Id,
					DataType = field.DataType,
					TableId = field.TableId,
					Description = field.Description,
					Name = field.Name,
					DisplayName = field.DisplayName,
					AllowNull = field.AllowNull,
					Synchronized = true,
					TableFieldTypeId = field.TableFieldTypeId,
				};
				foreach (var item in field.Configurations)
				{
					configValues.Add(new TableFieldConfigValues
					{
						TableFieldConfigId = item.ConfigId,
						TableModelFieldId = model.Id,
						Value = item.Value,
					});
				}

				model.TableFieldConfigValues = configValues;
				try
				{
					Context.TableFields.Update(model);
					Context.SaveChanges();
					return RedirectToAction("Edit", "Table", new { id = field.TableId, tab = "two" });
				}
				catch
				{
					ModelState.AddModelError("Fail", "Fail on update data to database!");
				}

				//foreach (var item in model.TableFieldConfigValues)
				//{
				//	Context.TableFieldConfigValues.Update(item);
				//	Context.SaveChanges();
				//}
			}

			return View(field);
		}

		/// <summary>
		/// Delete field
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult DeleteField(string id)
		{
			var field = Context.TableFields.First(x => x.Id == Guid.Parse(id));
			var table = Context.Table.First(x => x.Id == field.TableId);
			ITablesService sqlService = GetSqlService();
			var checkColumn = sqlService.CheckColumnValues(ConnectionString.Item2, table.Name, table.EntityType, field.Name);
			if (checkColumn.Result)
			{
				return Json(false);
			}
			else
			{
				var fieldType = Context.TableFieldTypes.First(x => x.Name == FieldType.EntityReference).Id;
				if (field.TableFieldTypeId == fieldType)
				{
					// ?? posibil eroare
					var configtype = Context.TableFieldConfigs.First(x => x.TableFieldTypeId == fieldType).Id;
					var configValue = Context.TableFieldConfigValues.First(x =>
						x.TableFieldConfigId == configtype && x.TableModelFieldId == field.Id).Value;
					sqlService.DropConstraint(ConnectionString.Item2, table.Name, table.EntityType, configValue, field.Name);
				}

				var dropColumn = sqlService.DropColumn(ConnectionString.Item2, table.Name, table.EntityType, field.Name);
				if (!dropColumn.Result) return Json(false);
				Context.TableFields.Remove(field);
				Context.SaveChanges();
				return Json(true);
			}
		}

		/// <summary>
		/// Delete table
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpPost]
		[AuthorizePermission(PermissionsConstants.CorePermissions.BpmTableDelete)]
		public JsonResult DeleteTable(Guid? id)
		{
			if (!id.HasValue)
			{
				return Json(new { success = false, message = "Id not found" });
			}

			var table = Context.Table.First(x => x.Id == id);
			ITablesService sqlService = GetSqlService();
			var checkColumn = sqlService.CheckTableValues(ConnectionString.Item2, table.Name, table.EntityType);
			if (checkColumn.Result)
			{
				return Json(new { success = false, message = "Table has value" });
			}
			else
			{
				var isUsedForms = Context.Forms.Any(x => x.TableId == id);
				var isUsedProfiles = ApplicationDbContext.Profiles.Any(x => x.EntityId == id);
				if (isUsedForms || isUsedProfiles)
				{
					return Json(new { success = false, message = "Table is used" });
				}

				try
				{
					sqlService.DropTable(ConnectionString.Item2, table.Name, table.EntityType);
					Context.Table.Remove(table);
					Context.SaveChanges();
					return Json(new { success = true, message = "Delete success" });
				}
				catch (Exception e)
				{
					_logger.LogError(e.Message);
					return Json(new { success = false, message = "Same error on delete!" });
				}
			}
		}
	}
	public static class FieldExtension
	{
		public static CreateTableFieldViewModel CreateSqlField(this CreateTableFieldViewModel field, (DbProviderType, string) connection)
		{
			switch (field.Parameter)
			{
				case FieldType.EntityReference:
					field.DataType = TableFieldDataType.Guid;
					break;
				case FieldType.Boolean:
					FieldConfigViewModel defaultBool = null;
					foreach (var c in field.Configurations)
					{
						if (c.Name != FieldConfig.DefaultValue) continue;
						defaultBool = c;
						break;
					}

					if (defaultBool?.Value != null && defaultBool.Value.Trim() == "on") defaultBool.Value = "1";
					if (defaultBool?.Value != null && defaultBool.Value.Trim() == "off") defaultBool.Value = "0";
					break;
				case FieldType.DateTime:
				case FieldType.Date:
				case FieldType.Time:
					FieldConfigViewModel defaultTime = null;
					foreach (var c in field.Configurations)
					{
						if (c.Name != FieldConfig.DefaultValue) continue;
						defaultTime = c;
						break;
					}

					if (defaultTime?.Value != null && defaultTime.Value.Trim() == "on")
						defaultTime.Value = "CURRENT_TIMESTAMP";
					if (defaultTime?.Value != null && defaultTime.Value.Trim() == "off") defaultTime.Value = null;
					break;
				case FieldType.File:
					field.DataType = TableFieldDataType.Guid;
					var foreignTable = field.Configurations.FirstOrDefault(s => s.Name == "ForeingTable");
					if (foreignTable != null)
					{
						foreignTable.Value = "FileReferences";
					}

					break;
			}
			return field;
		}
	}
}