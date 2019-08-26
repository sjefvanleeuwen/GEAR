﻿using Microsoft.AspNetCore.Mvc;
using ST.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ST.TaskManager.Abstractions;
using ST.TaskManager.Abstractions.Helpers;
using ST.TaskManager.Abstractions.Models.ViewModels;


namespace ST.TaskManager.Razor.Controllers
{
    [Route("api/[controller]/[action]")]
    public sealed class TaskManagerController : Controller
    {
        /// <summary>
        /// Inject file service
        /// </summary>
        private readonly ITaskManager _taskManager;

        public TaskManagerController(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }


        /// <summary>
        /// Index page
        /// </summary>
        /// <returns></returns>
        [HttpGet("/TaskManager")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Produces("application/json", Type = typeof(ResultModel<GetTaskViewModel>))]
        public async Task<JsonResult> GetTask(Guid id)
        {
            if (id == Guid.Empty) return Json(ExceptionHandler.ReturnErrorModel(ExceptionMessagesEnum.NullParameter));

            var response = await _taskManager.GetTaskAsync(id);
            return Json(response);
        }

        [HttpGet]
        [Produces("application/json", Type = typeof(ResultModel<List<GetTaskViewModel>>))]
        public async Task<JsonResult> GetUserTasks(Guid id)
        {
            if (id == Guid.Empty) return Json(ExceptionHandler.ReturnErrorModel(ExceptionMessagesEnum.NullParameter));

            var response = await _taskManager.GetTasksAsync(id);
            return Json(response);
        }

        [HttpGet]
        [Produces("application/json", Type = typeof(ResultModel<List<TaskItemViewModel>>))]
        public async Task<JsonResult> GetTaskItems(Guid id)
        {
            if (id == Guid.Empty) return Json(ExceptionHandler.ReturnErrorModel(ExceptionMessagesEnum.NullParameter));

            var response = await _taskManager.GetTaskItemsAsync(id);
            return Json(response);
        }

        [HttpPost]
        [Produces("application/json", Type = typeof(ResultModel<CreateTaskViewModel>))]
        public async Task<JsonResult> CreateTask(CreateTaskViewModel model)
        {
            if (model == null) return Json(ExceptionHandler.ReturnErrorModel(ExceptionMessagesEnum.NullParameter));

            var response = await _taskManager.CreateTaskAsync(model);
            return Json(response);
        }

        [HttpPost]
        [Produces("application/json", Type = typeof(ResultModel<UpdateTaskViewModel>))]
        public async Task<JsonResult> UpdateTask(UpdateTaskViewModel model)
        {
            if (model == null || model.Id == Guid.Empty) return Json(ExceptionHandler.ReturnErrorModel(ExceptionMessagesEnum.NullParameter));

            var response = await _taskManager.UpdateTaskAsync(model);
            return Json(response);
        }

        [HttpPost]
        [Produces("application/json", Type = typeof(ResultModel))]
        public async Task<JsonResult> DeleteTask(Guid id)
        {
            if (id == Guid.Empty) return Json(ExceptionHandler.ReturnErrorModel(ExceptionMessagesEnum.NullParameter));

            var response = await _taskManager.DeleteTaskAsync(id);
            return Json(response);
        }

        [HttpPost]
        [Produces("application/json", Type = typeof(ResultModel))]
        public async Task<JsonResult> DeleteTaskPermanent(Guid id)
        {
            if (id == Guid.Empty) return Json(ExceptionHandler.ReturnErrorModel(ExceptionMessagesEnum.NullParameter));

            var response = await _taskManager.DeletePermanentTaskAsync(id);
            return Json(response);
        }

        [HttpPost]
        [Produces("application/json", Type = typeof(ResultModel<TaskItemViewModel>))]
        public async Task<JsonResult> CreateTaskItem(TaskItemViewModel model)
        {
            if (model == null) return Json(ExceptionHandler.ReturnErrorModel(ExceptionMessagesEnum.NullParameter));

            var response = await _taskManager.CreateTaskItemAsync(model);
            return Json(response);
        }

        [HttpPost]
        [Produces("application/json", Type = typeof(ResultModel<TaskItemViewModel>))]
        public async Task<JsonResult> UpdateTaskItem(TaskItemViewModel model)
        {
            if (model == null || model.Id == Guid.Empty) return Json(ExceptionHandler.ReturnErrorModel(ExceptionMessagesEnum.NullParameter));

            var response = await _taskManager.UpdateTaskItemAsync(model);
            return Json(response);
        }

        [HttpPost]
        [Produces("application/json", Type = typeof(ResultModel))]
        public async Task<JsonResult> DeleteTaskItem(Guid id)
        {
            if (id == Guid.Empty) return Json(ExceptionHandler.ReturnErrorModel(ExceptionMessagesEnum.NullParameter));

            var response = await _taskManager.DeleteTaskItemAsync(id);
            return Json(response);
        }
    }
}
