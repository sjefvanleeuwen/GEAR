using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST.BaseBusinessRepository;
using ST.BaseRepository;
using ST.CORE.Attributes;
using ST.CORE.Models;
using ST.Entities.Data;
using ST.Entities.Extensions;
using ST.Entities.Models.ViewModels;

namespace ST.CORE.Controllers.Entity
{
	public class ViewModelController : Controller
	{
		/// <summary>
		/// Context
		/// </summary>
		private readonly EntitiesDbContext _context;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="context"></param>
		public ViewModelController(EntitiesDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Index view
		/// </summary>
		/// <returns></returns>
		public IActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Edit
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult Edit(Guid id)
		{
			var viewModel = _context.ViewModels.FirstOrDefault(x => x.Id.Equals(id));
			if (viewModel == null) return NotFound();
			return View(viewModel);
		}

		/// <summary>
		/// Edit template of view model field
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult TemplateEdit([Required]Guid id)
		{
			var viewModelField = _context.ViewModelFields.FirstOrDefault(x => x.Id.Equals(id));
			if (viewModelField == null) return NotFound();
			return View(viewModelField);
		}

		/// <summary>
		/// Template edit
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult TemplateEdit(ViewModelFields model)
		{
			if (model.Id != null)
			{
				var dataModel = _context.ViewModelFields.FirstOrDefault(x => x.Id.Equals(model.Id));
				if (dataModel == null)
				{
					ModelState.AddModelError(string.Empty, "Invalid data entry!");
					return View(model);
				}

				dataModel.Template = model.Template;
				_context.Update(dataModel);
				try
				{
					_context.SaveChanges();
					return RedirectToAction("Index");
				}
				catch { }
			}
			ModelState.AddModelError(string.Empty, "Invalid data entry!");
			return View(model);
		}


		/// <summary>
		/// Update view model name
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult Edit([Required] ViewModel model)
		{
			if (model.Id != null)
			{
				var dataModel = _context.ViewModels.FirstOrDefault(x => x.Id.Equals(model.Id));
				if (dataModel == null)
				{
					ModelState.AddModelError(string.Empty, "Invalid data entry!");
					return View(model);
				}

				dataModel.Name = model.Name;
				_context.Update(dataModel);
				try
				{
					_context.SaveChanges();
					return RedirectToAction("Index");
				}
				catch { }
			}
			ModelState.AddModelError(string.Empty, "Invalid data entry!");
			return View(model);
		}


		/// <summary>
		/// Create view model
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public JsonResult GenerateViewModel([Required]Guid entityId)
		{
			var table = _context.Table.Include(x => x.TableFields).FirstOrDefault(x => x.Id.Equals(entityId));
			if (table == null) return Json(default(ResultModel));
			var id = Guid.NewGuid();
			var fields = new List<ViewModelFields>();
			var model = new ViewModel
			{
				Id = id,
				Name = $"{table.Name}_{id}",
				TableModel = table
			};
			var c = 0;
			var props = typeof(BaseModel).GetProperties().ToList();

			fields.AddRange(props.Select(x => new ViewModelFields
			{
				Order = c++,
				Name = x.Name,
				Template = $"`${{row.{x.Name[0].ToString().ToLower()}{x.Name.Substring(1, x.Name.Length - 1)}}}`"
			}));

			fields.AddRange(table.TableFields.Select(x => new ViewModelFields
			{
				Order = c++,
				Name = x.Name,
				TableModelFields = x,
				Template = $"`${{row.{x.Name[0].ToString().ToLower()}{x.Name.Substring(1, x.Name.Length - 1)}}}`"
			}));

			model.ViewModelFields = fields;
			_context.ViewModels.Add(model);
			try
			{
				_context.SaveChanges();
				return Json(new ResultModel { IsSuccess = true, Result = id });
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return Json(default(ResultModel));
			}
		}

		/// <summary>
		/// Update styles of page
		/// </summary>
		/// <param name="styles"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult UpdateViewModelFields([Required]IEnumerable<ViewModelFields> items)
		{
			return UpdateItems(items.ToList());
		}

		/// <summary>
		/// Update items
		/// </summary>
		/// <typeparam name="TItem"></typeparam>
		/// <param name="items"></param>
		/// <returns></returns>
		[NonAction]
		private JsonResult UpdateItems<TItem>(IList<TItem> items) where TItem : ViewModelFields
		{
			var result = new ResultModel();
			if (!items.Any())
			{
				result.IsSuccess = true;
				return Json(result);
			}

			var pageId = items.First().ViewModelId;
			var pageScripts = _context.Set<TItem>().Where(x => x.ViewModelId.Equals(pageId)).ToList();

			foreach (var prev in pageScripts)
			{
				var up = items.FirstOrDefault(x => x.Id.Equals(prev.Id));
				if (up == null)
				{
					_context.Set<TItem>().Remove(prev);
				}
				else if (prev.Order != up.Order || prev.Name != up.Name)
				{
					prev.Name = up.Name;
					prev.Order = up.Order;
					_context.Set<TItem>().Update(prev);
				}
			}

			var news = items.Where(x => x.Id == Guid.Empty).Select(x => new
			{
				ViewModelId = pageId,
				x.Name,
				x.Order
			}).Adapt<IEnumerable<TItem>>().ToList();

			if (news.Any())
			{
				_context.Set<TItem>().AddRange(news);
			}

			try
			{
				_context.SaveChanges();
				result.IsSuccess = true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}

			return new JsonResult(result);
		}

		/// <summary>
		/// Order fields
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult OrderFields([Required]Guid id)
		{
			if (Guid.Empty == id) return NotFound();
			ViewBag.Data = _context.ViewModelFields
				.Where(x => x.ViewModelId.Equals(id))
				.Include(x => x.TableModelFields)
				.OrderBy(x => x.Order).ToList();
			return View();
		}


		/// <summary>
		/// Load page types with ajax
		/// </summary>
		/// <param name="param"></param>
		/// <param name="entityId"></param>
		/// <returns></returns>
		[HttpPost]
		[AjaxOnly]
		public JsonResult LoadViewModels(DTParameters param, Guid entityId)
		{
			var filtered = _context.Filter<ViewModel>(param.Search.Value, param.SortOrder, param.Start,
				param.Length,
				out var totalCount, x => (entityId != Guid.Empty && x.TableModelId == entityId) || entityId == Guid.Empty);


			var sel = filtered.Select(x => new
			{
				x.Author,
				x.Changed,
				x.Created,
				x.Id,
				x.IsDeleted,
				x.ModifiedBy,
				x.Name,
				Table = _context.Table.FirstOrDefault(y => y.Id.Equals(x.TableModelId))?.Name
			}).Adapt<IEnumerable<object>>();

			var finalResult = new DTResult<object>
			{
				draw = param.Draw,
				data = sel.ToList(),
				recordsFiltered = totalCount,
				recordsTotal = filtered.Count()
			};
			return Json(finalResult);
		}

		/// <summary>
		/// Delete page type by id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[Route("api/[controller]/[action]")]
		[ValidateAntiForgeryToken]
		[HttpPost, Produces("application/json", Type = typeof(ResultModel))]
		public JsonResult Delete(string id)
		{
			if (string.IsNullOrEmpty(id)) return Json(new { message = "Fail to delete view model!", success = false });
			var page = _context.ViewModels.FirstOrDefault(x => x.Id.Equals(Guid.Parse(id)));
			if (page == null) return Json(new { message = "Fail to delete view model!", success = false });

			try
			{
				_context.ViewModels.Remove(page);
				_context.SaveChanges();
				return Json(new { message = "View model was delete with success!", success = true });
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			return Json(new { message = "Fail to delete view model!", success = false });
		}
	}
}