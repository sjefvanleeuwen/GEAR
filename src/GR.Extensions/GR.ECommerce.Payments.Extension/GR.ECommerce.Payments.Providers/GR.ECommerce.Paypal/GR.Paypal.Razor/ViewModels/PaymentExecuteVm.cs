﻿using Newtonsoft.Json;

namespace GR.Paypal.Razor.ViewModels
{
    public class PaymentExecuteVm
    {
        [JsonProperty("paymentID")] public string PaymentId { get; set; }
        [JsonProperty("payerID")] public string PayerId { get; set; }
    }
}