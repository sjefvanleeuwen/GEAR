﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GR.Core.Extensions;
using GR.ECommerce.Abstractions.Enums;
using GR.Identity.Abstractions;
using GR.Identity.Profile.Abstractions;
using GR.Identity.Profile.Abstractions.Models.AddressModels;
using GR.Orders.Abstractions;
using GR.Orders.Abstractions.Models;
using GR.Orders.Abstractions.ViewModels.CheckoutViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GR.ECommerce.Razor.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        #region Injectable

        /// <summary>
        /// Inject user manager
        /// </summary>
        private readonly IUserManager<GearUser> _userManager;

        /// <summary>
        /// Inject order service
        /// </summary>
        private readonly IOrderProductService<Order> _orderProductService;

        /// <summary>
        /// Inject user address service
        /// </summary>
        private readonly IUserAddressService _userAddressService;

        #endregion

        public CheckoutController(IUserManager<GearUser> userManager, IOrderProductService<Order> orderProductService, IUserAddressService userAddressService)
        {
            _userManager = userManager;
            _orderProductService = orderProductService;
            _userAddressService = userAddressService;
        }

        /// <summary>
        /// Checkout page
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Shipping(Guid? orderId)
        {
            if (orderId == null) return NotFound();
            var userRequest = await _userManager.GetCurrentUserAsync();
            if (!userRequest.IsSuccess) return NotFound();
            var orderRequest = await _orderProductService.GetOrderByIdAsync(orderId);
            if (!orderRequest.IsSuccess) return NotFound();

            var wasInvoicedRequest = await _orderProductService.ItWasInTheStateAsync(orderId, OrderState.Invoiced);
            if (wasInvoicedRequest.IsSuccess && wasInvoicedRequest.Result) return NotFound();
            var addressesRequest = await _userAddressService.GetUserAddressesAsync(userRequest.Result.Id);
            var model = new CheckoutShippingViewModel
            {
                Order = orderRequest.Result,
                Addresses = addressesRequest.Result?.ToList() ?? new List<Address>()
            };

            return View(model);
        }

        /// <summary>
        /// Set up shipment and billing address
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Shipping([Required]CheckoutShippingViewModel model)
        {
            if (model == null) return NotFound();

            if (model.Order != null)
            {
                var addressUpdateRequest = await _orderProductService.SetOrderBillingAddressAndShipmentAsync(model.Order?.Id, model.ShipmentAddress, model.BillingAddressId);
                if (addressUpdateRequest.IsSuccess)
                    return RedirectToAction("Payment", new { OrderId = model.Order.Id });
                ModelState.AppendResultModelErrors(addressUpdateRequest.Errors);
            }

            var userRequest = await _userManager.GetCurrentUserAsync();
            if (!userRequest.IsSuccess) return NotFound();
            var orderRequest = await _orderProductService.GetOrderByIdAsync(model.Order?.Id);
            if (!orderRequest.IsSuccess) return NotFound();
            if (orderRequest.Result.OrderState != OrderState.New) return NotFound();
            var addressesRequest = await _userAddressService.GetUserAddressesAsync(userRequest.Result.Id);
            model.Order = orderRequest.Result;
            model.Addresses = addressesRequest.Result;
            return View(model);
        }

        /// <summary>
        /// Make payment
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Payment(Guid? orderId)
        {
            var orderRequest = await _orderProductService.GetOrderByIdAsync(orderId);
            if (!orderRequest.IsSuccess) return NotFound();
            var wasInvoicedRequest = await _orderProductService.ItWasInTheStateAsync(orderId, OrderState.Invoiced);
            if (!wasInvoicedRequest.IsSuccess || !wasInvoicedRequest.Result)
                return RedirectToAction(nameof(Shipping), new { OrderId = orderId });

            return View(orderRequest.Result);
        }

        /// <summary>
        /// Success
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Success(Guid? orderId)
        {
            var orderRequest = await _orderProductService.GetOrderByIdAsync(orderId);
            if (!orderRequest.IsSuccess) return NotFound();
            return View(orderRequest.Result);
        }


        /// <summary>
        /// Success
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Fail(Guid? orderId)
        {
            var orderRequest = await _orderProductService.GetOrderByIdAsync(orderId);
            if (!orderRequest.IsSuccess) return NotFound();
            return View(orderRequest.Result);
        }
    }
}