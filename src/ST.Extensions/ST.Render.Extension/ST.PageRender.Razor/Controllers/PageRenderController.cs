using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ST.DynamicEntityStorage.Abstractions;
using ST.DynamicEntityStorage.Abstractions.Extensions;
using ST.Identity.Data;
using ST.PageRender.Razor.Extensions;
using ST.PageRender.Razor.Helpers;
using ST.PageRender.Razor.ViewModels.PageViewModels;
using ST.Core;
using ST.Core.Attributes;
using ST.Core.Extensions;
using ST.Core.Helpers;
using ST.DynamicEntityStorage.Abstractions.Enums;
using ST.DynamicEntityStorage.Abstractions.Helpers;
using ST.Entities.Abstractions.Constants;
using ST.Forms.Abstractions;
using ST.Identity.Abstractions;
using ST.PageRender.Abstractions;
using ST.PageRender.Abstractions.Configurations;
using ST.PageRender.Abstractions.Models.ViewModels;
using ST.PageRender.Razor.Attributes;

namespace ST.PageRender.Razor.Controllers
{
    [Authorize]
    public class PageRenderController : Controller
    {
        #region InjectRegion

        private readonly IDynamicPagesContext _pagesContext;

        /// <summary>
        /// App Context
        /// </summary>
        private readonly ApplicationDbContext _appContext;

        /// <summary>
        /// Inject Data Service
        /// </summary>
        private readonly IDynamicService _service;

        /// <summary>
        /// Inject page render
        /// </summary>
        private readonly IPageRender _pageRender;

        /// <summary>
        /// Inject menu dataService
        /// </summary>
        private readonly IMenuService _menuService;

        /// <summary>
        /// Inject form context
        /// </summary>
        private readonly IFormContext _formContext;

        /// <summary>
        /// Inject user manager
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="service"></param>
        /// <param name="pageRender"></param>
        /// <param name="menuService"></param>
        /// <param name="userManager"></param>
        /// <param name="formContext"></param>
        /// <param name="pagesContext"></param>
        public PageRenderController(ApplicationDbContext appContext,
            IDynamicService service,
            IPageRender pageRender,
            IMenuService menuService, UserManager<ApplicationUser> userManager, IFormContext formContext, IDynamicPagesContext pagesContext)
        {
            _appContext = appContext;
            _service = service;
            _menuService = menuService;
            _userManager = userManager;
            _formContext = formContext;
            _pagesContext = pagesContext;
            _pageRender = pageRender;
        }

        /// <summary>
        /// Get page by page name
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        [AuthorizePage]
        [AllowAnonymous]
        public async Task<IActionResult> Index([Required] Guid pageId)
        {
            var page = await _pageRender.GetPageAsync(pageId);
            if (page == null) return NotFound();
            ViewBag.Page = page;
            ViewData["IsDynamicPage"] = true;
            return View();
        }

        /// <summary>
        /// Get All Entities
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetEntities()
        {
            var entities = _pagesContext.Table.Where(x => !x.IsDeleted).ToList();

            return new JsonResult(entities);
        }

        /// <summary>
        /// Get All Entities
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetRoles()
        {
            var roles = _appContext.Roles.Where(x => !x.IsDeleted).ToList();

            return new JsonResult(roles);
        }

        /// <summary>
        /// Get All view Models
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetViewModels()
        {
            var viewModels = _pagesContext.ViewModels.Where(x => !x.IsDeleted).ToList();

            return new JsonResult(viewModels);
        }

        /// <summary>
        /// Get input select values
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetInputSelectValues(Guid fieldId)
        {
            if (Guid.Empty == fieldId) return Json(new ResultModel());

            var field = _pagesContext.TableFields.FirstOrDefault(x => x.Id.Equals(fieldId));
            if (field == null) return Json(new ResultModel());
            var config = await _pagesContext.TableFieldConfigValues
                .Include(x => x.TableFieldConfig)
                .ThenInclude(x => x.TableFieldType)
                .FirstOrDefaultAsync(x => x.TableModelFieldId.Equals(fieldId)
                                          && x.TableFieldConfig.Code == TableFieldConfigCode.Reference.ForeingTable);
            if (config == null) return Json(new ResultModel());
            if (!config.TableFieldConfig.TableFieldType.Name.Equals("EntityReference")) return Json(new ResultModel());
            var table = config.Value;
            var instance = _service.Table(table);
            return Json(await instance.GetAllWithInclude<object>(filters: new List<Filter>
            {
                new Filter
                {
                    Value = false,
                    Criteria = Criteria.Equals,
                    Parameter = nameof(BaseModel.IsDeleted)
                }
            }));
        }

        /// <summary>
        /// Get view model by id
        /// </summary>
        /// <param name="viewModelId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetViewModelById([Required] Guid viewModelId)
        {
            var obj = _pagesContext.ViewModels
                .Include(x => x.TableModel)
                .Include(x => x.ViewModelFields)
                .ThenInclude(x => x.TableModelFields)
                .ThenInclude(x => x.TableFieldConfigValues)
                .FirstOrDefault(x => !x.IsDeleted && x.Id.Equals(viewModelId));

            if (obj != null)
            {
                obj.ViewModelFields = obj.ViewModelFields.OrderBy(x => x.Order);
            }

            return Json(new ResultModel
            {
                IsSuccess = true,
                Result = obj
            }, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        /// <summary>
        /// Get example object of entity
        /// </summary>
        /// <param name="viewModelId"></param>
        /// <returns></returns>
        [HttpGet]
        [AjaxOnly]
        [Authorize(Roles = Settings.SuperAdmin)]
        public JsonResult GetJsonExampleOfEntity([Required] Guid viewModelId)
        {
            var entity = _pagesContext.ViewModels.Include(x => x.TableModel).FirstOrDefault(x => x.Id.Equals(viewModelId));
            if (entity == null) return Json(default(ResultModel));
            var objType = _service.Table(entity.TableModel.Name).Type;
            var obj = Activator.CreateInstance(objType);
            var referenceFields = obj.GetType().GetProperties()
                .Where(x => !x.PropertyType.GetTypeInfo()?.FullName?.StartsWith("System") ?? false)
                .ToList();
            foreach (var refField in referenceFields)
            {
                var refPropName = refField.Name;
                try
                {
                    var refTypeProperty = obj.GetType().GetProperty(refPropName);
                    if (refTypeProperty == null) continue;
                    var newInstance = Activator.CreateInstance(refTypeProperty.PropertyType);
                    refTypeProperty.SetValue(obj, newInstance);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return Json(new { row = obj });
        }

        /// <summary>
        /// Get all blocks
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetBlocks()
        {
            var blocks = _pagesContext.Blocks.Include(x => x.BlockCategory).Where(x => !x.IsDeleted).ToList();
            var result = blocks.Select(x => new
            {
                x.Id,
                BlockName = x.Name,
                Html = x.HtmlCode,
                Css = x.CssCode,
                Category = x.BlockCategory.Name,
                Icon = x.FaIcon
            });
            return new JsonResult(result);
        }

        /// <summary>
        /// Get scripts as list for page by id
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetScripts(Guid pageId)
        {
            if (pageId == Guid.Empty) return Json(default(IEnumerable<string>));
            var scripts = new HashSet<string>();
            var page = _pagesContext.Pages.Include(x => x.PageScripts).FirstOrDefault(x => x.Id.Equals(pageId));

            if (page == null) return Json(default(IEnumerable<string>));

            if (!page.IsLayout && page.LayoutId != null)
            {
                var layout = _pagesContext.Pages.Include(x => x.PageScripts).FirstOrDefault(x => x.Id.Equals(page.LayoutId));

                if (layout != null && layout.PageScripts.Any())
                {
                    var extracted = layout.PageScripts.OrderBy(x => x.Order).Where(x => !string.IsNullOrEmpty(x.Script))
                        .Select(x => ExtractSrcFromScript(x.Script)).ToList();
                    extracted.ForEach(x => { scripts.Add(x); });
                }
            }

            if (!page.PageScripts.Any()) return new JsonResult(scripts.ToList());
            {
                var extracted = page.PageScripts.OrderBy(x => x.Order).Where(x => !string.IsNullOrEmpty(x.Script))
                    .Select(x => ExtractSrcFromScript(x.Script)).ToList();
                extracted.ForEach(x => { scripts.Add(x); });
            }

            return new JsonResult(scripts.ToList());
        }


        /// <summary>
        /// Get scripts as list for page by id
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetStyles(Guid pageId)
        {
            if (pageId == Guid.Empty) return Json(default(IEnumerable<string>));
            var styles = new HashSet<string>();
            var page = _pagesContext.Pages.Include(x => x.PageStyles).FirstOrDefault(x => x.Id.Equals(pageId));

            if (page == null) return Json(default(IEnumerable<string>));

            if (!page.IsLayout && page.LayoutId != null)
            {
                var layout = _pagesContext.Pages.Include(x => x.PageStyles).FirstOrDefault(x => x.Id.Equals(page.LayoutId));

                if (layout != null && layout.PageStyles.Any())
                {
                    var extracted = layout.PageStyles.OrderBy(x => x.Order).Where(x => !string.IsNullOrEmpty(x.Script))
                        .Select(x => x.Script).ToList();
                    extracted.ForEach(x => { styles.Add(x); });
                }
            }

            if (!page.PageStyles.Any()) return new JsonResult(styles.ToList());
            {
                var extracted = page.PageStyles.OrderBy(x => x.Order).Where(x => !string.IsNullOrEmpty(x.Script))
                    .Select(x => x.Script).ToList();
                extracted.ForEach(x => { styles.Add(x); });
            }

            return new JsonResult(styles.ToList());
        }

        /// <summary>
        /// Extract Src from script
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        private static string ExtractSrcFromScript(string script)
        {
            try
            {
                var sp = script.Split("src=\"")?[1].Split("\"")?.FirstOrDefault();
                return sp;
            }
            catch
            {
                //Ignore
            }

            return string.Empty;
        }

        /// <summary>
        /// Get menu item roles 
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = Settings.SuperAdmin)]
        public async Task<JsonResult> GetMenuItemRoles([Required] Guid menuId)
        {
            if (menuId == Guid.Empty) return Json(new ResultModel());
            var roles = await _menuService.GetMenuRoles(menuId);

            return Json(roles);
        }

        /// <summary>
        /// Get menus
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetMenus(Guid? menuId = null)
        {
            if (menuId == null)
            {
                menuId = MenuManager.NavBarId;
            }

            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            var req = await _menuService.GetMenus(menuId, roles);
            return Json(req);
        }

        /// <summary>
        /// Update roles
        /// </summary>
        /// <param name="menuId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = Settings.SuperAdmin)]
        public async Task<JsonResult> UpdateMenuItemRoleAccess([Required] Guid menuId, IList<string> roles)
        {
            return Json(await _menuService.UpdateMenuItemRoleAccess(menuId, roles));
        }


        /// <summary>
        /// Get list data by entity id
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetListData(Guid entityId)
        {
            var table = _pagesContext.Table.Include(x => x.TableFields)
                .FirstOrDefault(x => x.Id.Equals(entityId));
            if (table == null) return Json(null);
            var instance = _service.Table(table.Name);
            return Json(await instance.GetAllWithInclude<object>());
        }

        /// <summary>
        /// Load paged data with ajax
        /// </summary>
        /// <param name="param"></param>
        /// <param name="viewModelId"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        [HttpPost]
        [AjaxOnly]
        public async Task<JsonResult> LoadPagedData(DTParameters param, Guid viewModelId, ICollection<ListFilter> filters)
        {
            if (viewModelId == Guid.Empty) return Json(default(DTResult<object>));
            var viewModel = await _pagesContext.ViewModels
                .Include(x => x.TableModel)
                .ThenInclude(x => x.TableFields)
                .Include(x => x.ViewModelFields)
                .ThenInclude(x => x.TableModelFields)
                .Include(x => x.ViewModelFields)
                .ThenInclude(x => x.Configurations)
                .FirstOrDefaultAsync(x => x.Id.Equals(viewModelId));

            if (viewModel == null) return Json(default(DTResult<object>));
            filters?.ToList().ForEach(x =>
            {
                x.SetValue();
                x.AdaptTypes();
            });

            var fields = viewModel.ViewModelFields.OrderBy(x => x.Order).ToList();
            var sortColumn = param.SortOrder;
            try
            {
                var colIndex = Convert.ToInt32(param.Order[0].Column);
                var columnIndex = colIndex == 0 ? colIndex : colIndex - 1;
                if (fields.Count - 1 > columnIndex)
                {
                    var field = fields.ElementAt(columnIndex);
                    if (field != null)
                    {
                        var column =
                            viewModel.TableModel.TableFields.FirstOrDefault(x => x.Id == field.TableModelFieldsId);
                        sortColumn = column != null
                            ? $"{column.Name ?? field.Name} {param.SortOrder}"
                            : $"{field.Name} {param.SortOrder}";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var roles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

            var (data, recordsCount) = await _service.Filter(viewModel.TableModel.Name, param.Search.Value, sortColumn,
                param.Start,
                param.Length, x => x.SortByUserRoleAccess(roles, Settings.SuperAdmin), filters);

            var final = await LoadManyToManyReferences(data, viewModel);

            var finalResult = new DTResult<object>
            {
                Draw = param.Draw,
                Data = final.ToList(),
                RecordsFiltered = recordsCount,
                RecordsTotal = data.Count
            };
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatString = Settings.Date.DateFormat
            };
            return Json(finalResult, serializerSettings);
        }

        /// <summary>
        /// Set many to many entity references
        /// </summary>
        /// <param name="data"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<IEnumerable<dynamic>> LoadManyToManyReferences(IList<object> data, ViewModel model)
        {
            Arg.NotNull(model, nameof(ViewModel));
            if (data == null) return data;
            var dicData = data.Select(x => new Dictionary<string, object>(x.ToDictionary())).ToList();

            try
            {
                foreach (var field in model.ViewModelFields)
                {
                    if (field.VirtualDataType != ViewModelVirtualDataType.ManyToMany) continue;
                    if (!field.Configurations.Any()) continue;
                    var storageEntity = field.Configurations.FirstOrDefault(x => x.ViewModelFieldCodeId == ViewModelConfigCode.MayToManyStorageEntityName);
                    var referenceEntity = field.Configurations.FirstOrDefault(x => x.ViewModelFieldCodeId == ViewModelConfigCode.MayToManyStorageEntityName);
                    var referencePropName = field.Configurations.FirstOrDefault(x => x.ViewModelFieldCodeId == ViewModelConfigCode.MayToManyReferencePropertyName);
                    var propName = field.Configurations.FirstOrDefault(x => x.ViewModelFieldCodeId == ViewModelConfigCode.MayToManyStorageSenderPropertyName);
                    if (storageEntity == null || referenceEntity == null || referencePropName == null ||
                        propName == null) continue;
                    foreach (var item in dicData)
                    {
                        var referenceData = await _service.Table(storageEntity.Value).GetAllWithInclude<object>(filters: new List<Filter>
                        {
                            new Filter(nameof(BaseModel.IsDeleted), false),
                            new Filter(referencePropName.Value, item["Id"].ToString().ToGuid())
                        });
                        item.Add($"{storageEntity.Value}Reference", referenceData.Result);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return dicData;
        }

        /// <summary>
        /// Get all forms
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetForms()
        {
            var forms = _formContext.Forms.Where(x => !x.IsDeleted).ToList();

            return new JsonResult(forms);
        }

        /// <summary>
        /// Get all pages
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetPages()
        {
            var pages = _pagesContext.Pages
                .Include(x => x.Settings)
                .Where(x => !x.IsDeleted && !x.IsLayout)
                .Select(x => new
                {
                    Id = x.Path,
                    x.Settings.Name
                }).ToList();

            return new JsonResult(pages);
        }

        /// <summary>
        /// Load data from entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<JsonResult> LoadDataFromEntity(string entityName)
        {
            var list = await _service.Table(entityName).GetAllWithInclude<object>();
            return new JsonResult(list.Result);
        }

        /// <summary>
        /// Get page script
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> GetPageScript(Guid pageId)
        {
            if (pageId == Guid.Empty) return string.Empty;
            var page = await _pageRender.GetPageAsync(pageId);
            return page == null ? string.Empty : page.Settings?.JsCode;
        }

        /// <summary>
        /// Post Form
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> PostForm(PostFormViewModel model)
        {
            var result = new ResultModel
            {
                IsSuccess = false,
                Errors = new List<IErrorModel>()
            };

            if (model == null)
            {
                result.Errors.Add(new ErrorModel("Null", "Data is not defined!"));
                return Json(result);
            }

            var form = _formContext.Forms.FirstOrDefault(x => x.Id.Equals(model.FormId));
            if (form == null)
            {
                result.Errors.Add(new ErrorModel("Null", "Form not found!"));
                return Json(result);
            }

            var table = _pagesContext.Table.Include(x => x.TableFields)
                .FirstOrDefault(x => x.Id.Equals(form.TableId));
            if (table == null)
            {
                result.Errors.Add(new ErrorModel("Null", "Form entity reference not found"));
                return Json(result);
            }

            if (model.IsEdit && !model.SystemFields.Any())
            {
                result.Errors
                    .Add(new ErrorModel("Fail", "No object id passed on form, try to refresh page and try again"));
                return Json(result);
            }

            var id = model.SystemFields?.FirstOrDefault(x => x.Key == "Id");

            var instance = _service.Table(table.Name);
            var fields = table.TableFields.ToList();
            var obj = Activator.CreateInstance(instance.Type);

            if (model.IsEdit)
            {
                if (id == null)
                {
                    result.Errors
                        .Add(new ErrorModel("Fail", "No object id passed on form, try to refresh page and try again"));
                    return Json(result);
                }

                var oldObj = await instance.GetById<object>(id.Value.ToGuid());
                if (!oldObj.IsSuccess)
                {
                    result.Errors
                        .Add(new ErrorModel("Fail", "Data missed, check if this data exist!"));
                    return Json(result);
                }

                obj = oldObj.Result;
            }

            foreach (var item in model.Data)
            {
                var field = fields.FirstOrDefault(x => x.Id.Equals(Guid.Parse(item.Key)));
                if (field == null) continue;
                try
                {
                    var prop = obj.GetType().GetProperty(field.Name);
                    if (prop == null) continue;

                    if (prop.PropertyType == typeof(Guid))
                    {
                        if (item.Value == null)
                        {
                            prop.SetValue(obj, null);
                        }
                        else
                        {
                            prop.SetValue(obj, Guid.Parse(item.Value));
                        }
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        bool.TryParse(item.Value, out var value);
                        prop.SetValue(obj, value);
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        int.TryParse(item.Value, out var value);
                        prop.SetValue(obj, value);
                    }
                    else
                    {
                        prop.SetValue(obj, item.Value);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            var req = (model.IsEdit) ? await instance.Update(obj) : await instance.Add(obj);

            if (req.IsSuccess)
            {
                result.IsSuccess = true;
                result.Result = new
                {
                    IdOfCreatedObject = req.Result,
                    form.RedirectUrl
                };
                return Json(result);
            }


            return Json(result);
        }

        /// <summary>
        /// Delete page by id
        /// </summary>
        /// <param name="viewModelId"></param>
        /// <param name="ids"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [HttpPost, Produces("application/json", Type = typeof(ResultModel))]
        [AjaxOnly]
        [Authorize(Roles = Settings.SuperAdmin)]
        public async Task<JsonResult> DeleteItemsFromDynamicEntity(Guid viewModelId, IEnumerable<string> ids, bool mode = true)
        {
            if (ids == null) return Json(new { message = "Fail to delete!", success = false });
            var viewModel = _pagesContext.ViewModels.Include(x => x.TableModel)
                .FirstOrDefault(x => x.Id.Equals(viewModelId));
            if (viewModel == null) return Json(new { message = "Fail to delete!", success = false });
            try
            {
                foreach (var id in ids)
                {
                    if (string.IsNullOrEmpty(id)) throw new Exception("Selected row not found!");
                    if (mode)
                    {
                        await _service.Table(viewModel.TableModel.Name).Delete<object>(Guid.Parse(id));
                    }
                    else
                    {
                        await _service.Table(viewModel.TableModel.Name).Restore<object>(Guid.Parse(id));
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new { message = e.Message, success = false });
            }

            return Json(new { message = "Items was deleted!", success = true });
        }

        /// <summary>
        /// Delete page by id
        /// </summary>
        /// <param name="viewModelId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, Produces("application/json", Type = typeof(ResultModel))]
        [AjaxOnly]
        [Authorize(Roles = Settings.SuperAdmin)]
        public async Task<JsonResult> RestoreItemFromDynamicEntity(Guid viewModelId, string id)
        {
            if (string.IsNullOrEmpty(id) || viewModelId == Guid.Empty)
                return Json(new { message = "Fail to restore!", success = false });
            var viewModel = _pagesContext.ViewModels.Include(x => x.TableModel)
                .FirstOrDefault(x => x.Id.Equals(viewModelId));
            if (viewModel == null) return Json(new { message = "Fail to restore!", success = false });
            var response = await _service.Table(viewModel.TableModel.Name).Restore<object>(Guid.Parse(id));
            if (!response.IsSuccess) return Json(new { message = "Fail to restore!", success = false });

            return Json(new { message = "Item was restored!", success = true });
        }
    }
}