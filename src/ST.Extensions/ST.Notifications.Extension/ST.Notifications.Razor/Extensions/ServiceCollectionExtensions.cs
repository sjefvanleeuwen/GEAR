﻿using Microsoft.Extensions.DependencyInjection;
using ST.Notifications.Abstractions.ServiceBuilder;
using ST.Notifications.Razor.Helpers;

namespace ST.Notifications.Razor.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register ui module
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static INotificationSubscriptionServiceCollection AddNotificationRazorUIModule(this INotificationSubscriptionServiceCollection services)
        {
            services.Services.ConfigureOptions(typeof(NotificationRazorFileConfiguration));
            return services;
        }
    }
}