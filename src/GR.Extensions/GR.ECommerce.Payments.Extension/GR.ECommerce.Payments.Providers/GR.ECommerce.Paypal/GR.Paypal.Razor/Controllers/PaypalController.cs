﻿using System;
using System.Threading.Tasks;
using GR.Core.Helpers;
using GR.Paypal.Abstractions;
using GR.Paypal.Abstractions.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GR.Paypal.Razor.Controllers
{
    public class PaypalController : Controller
    {
        #region Injectable

        /// <summary>
        /// Inject payment service
        /// </summary>
        private readonly IPaypalPaymentMethodService _paymentMethodManager;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public PaypalController()
        {
            _paymentMethodManager = IoC.Resolve<IPaypalPaymentMethodService>(nameof(PaypalPaymentMethodService));
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// Create payment
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreatePayment(Guid? orderId)
        {
            var hostingDomain = Request.Host.Value;

            var response = await _paymentMethodManager.CreatePayment(hostingDomain, orderId);
            if (response.IsSuccess)
            {
                var paymentId = response.Message;
                return Ok(new { PaymentId = paymentId });
            }

            return BadRequest(response.Message);
        }


        /// <summary>
        /// Execute payment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> ExecutePayment(PaymentExecuteVm model)
        {
            var response = await _paymentMethodManager.ExecutePayment(model);

            if (response.IsSuccess)
            {
                return Ok(new { Status = "success", response.OrderId });
            }

            return BadRequest(response.Message);
        }
    }
}