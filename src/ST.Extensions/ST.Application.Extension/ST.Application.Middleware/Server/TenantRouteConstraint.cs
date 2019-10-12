﻿using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ST.Cache.Abstractions;
using ST.Identity.CacheModels;

namespace ST.Application.Middleware.Server
{
    public class TenantRouteConstraint : IRouteConstraint
    {
        /// <inheritdoc />
        /// <summary>
        /// Match url tenant
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="route"></param>
        /// <param name="routeKey"></param>
        /// <param name="values"></param>
        /// <param name="routeDirection"></param>
        /// <returns></returns>
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var site = values[routeKey]?.ToString();
            if (!UrlRewrite.ExcludeAssets(site)) return false;
            var cacheService = httpContext.RequestServices.GetService<ICacheService>();
            try
            {
                var tenant = cacheService.Get<TenantSettings>($"_tenant_{site}").GetAwaiter().GetResult();
                return tenant != null && tenant.AllowAccess;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }
    }
}