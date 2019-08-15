﻿using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ST.Core.Helpers;
using ST.Files.Abstraction.Models.ViewModels;
using ST.Files.Box.Abstraction;

namespace ST.Files.Razor.Controllers
{
    public class FileBoxController : Controller
    {
        /// <summary>
        /// Inject file service
        /// </summary>
        private readonly IFileBoxManager _fileManager;

        public FileBoxController(IFileBoxManager fileManager)
        {
            _fileManager = fileManager;
        }

        public IActionResult FileBox()
        {
            return View();
        }

        /// <summary>
        /// Get file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/[controller]/[action]")]
        [HttpGet]
        public FileResult GetFile(Guid id)
        {
            var response = _fileManager.GetFileById(id);
            return PhysicalFile(Path.Combine(response.Result.Path, response.Result.FileName), "text/plain");
        }

        /// <summary>
        /// Upload/Update file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public JsonResult Upload(Guid id)
        {
            var file = new UploadFileViewModel
            {
                File = Request.Form.Files.FirstOrDefault(),
                Id = id
            };
            var response = _fileManager.AddFile(file);
            return Json(response);
        }

        /// <summary>
        /// Multiple Upload
        /// </summary>
        /// <returns></returns>
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public JsonResult UploadMultiple()
        {
            var response = Request.Form.Files.Select(item => new UploadFileViewModel
            {
                File = item,
                Id = Guid.Empty
            }).Select(file => _fileManager.AddFile(file)).ToList();

            return Json(response);
        }

        /// <summary>
        /// Delete file logical
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/[controller]/[action]")]
        [HttpPost]
        [Produces("application/json", Type = typeof(ResultModel))]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(new {message = "Fail to delete file!", success = false});

            var res = _fileManager.DeleteFile(Guid.Parse(id));
            return Json(new {message = "Form was delete with success!", success = res.IsSuccess});
        }

        /// <summary>
        /// Restore File
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/[controller]/[action]")]
        [HttpPost]
        [Produces("application/json", Type = typeof(ResultModel))]
        public JsonResult Restore(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(new {message = "Fail to restore file!", success = false});

            var res = _fileManager.RestoreFile(Guid.Parse(id));
            return Json(new {message = "Form was delete with success!", success = res.IsSuccess});
        }

        /// <summary>
        /// Delete file permanently
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/[controller]/[action]")]
        [HttpPost]
        [Produces("application/json", Type = typeof(ResultModel))]
        public JsonResult DeletePermanent(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(new {message = "Fail to delete form!", success = false});

            var res = _fileManager.DeleteFilePermanent(Guid.Parse(id));
            return Json(new {message = "Form was delete with success!", success = res.IsSuccess});
        }
    }
}
