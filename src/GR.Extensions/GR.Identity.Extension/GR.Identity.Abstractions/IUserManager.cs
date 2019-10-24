﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using GR.Core.Helpers;

namespace GR.Identity.Abstractions
{
    public interface IUserManager<TUser> where TUser : ApplicationUser
    {
        /// <summary>
        /// User manager
        /// </summary>
        UserManager<TUser> UserManager { get; }
        /// <summary>
        /// Role manager
        /// </summary>
        RoleManager<ApplicationRole> RoleManager { get; }

        /// <summary>
        /// Identity context
        /// </summary>
        IIdentityContext IdentityContext { get; }

        /// <summary>
        /// Get the tenant id of current user
        /// </summary>
        Guid? CurrentUserTenantId { get; }
        /// <summary>
        /// Get the current user
        /// </summary>
        /// <returns></returns>
        Task<ResultModel<ApplicationUser>> GetCurrentUserAsync();

        /// <summary>
        /// Get roles from claims
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetRolesFromClaims();

        /// <summary>
        /// Get request ip address
        /// </summary>
        /// <returns></returns>
        string GetRequestIpAdress();

        /// <summary>
        /// Add roles to user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        Task<ResultModel> AddToRolesAsync(ApplicationUser user, ICollection<string> roles);

        /// <summary>
        /// Get user roles
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IEnumerable<ApplicationRole>> GetUserRolesAsync(ApplicationUser user);
    }
}