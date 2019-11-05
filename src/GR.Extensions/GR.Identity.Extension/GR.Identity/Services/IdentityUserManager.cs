﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using GR.Core;
using GR.Core.Extensions;
using GR.Core.Helpers;
using GR.Core.Helpers.Responses;
using GR.Identity.Abstractions;
using GR.Identity.Abstractions.Extensions;
using GR.Identity.Abstractions.Models.AddressModels;
using Microsoft.EntityFrameworkCore;

namespace GR.Identity.Services
{
    public class IdentityUserManager : IUserManager<ApplicationUser>
    {
        /// <inheritdoc />
        /// <summary>
        /// Inject user manager
        /// </summary>
        public UserManager<ApplicationUser> UserManager { get; }

        /// <inheritdoc />
        /// <summary>
        /// Inject role manager
        /// </summary>
        public RoleManager<ApplicationRole> RoleManager { get; }

        /// <inheritdoc />
        /// <summary>
        /// Identity context
        /// </summary>
        public IIdentityContext IdentityContext { get; }

        /// <summary>
        /// Inject context accessor
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityUserManager(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, RoleManager<ApplicationRole> roleManager, IIdentityContext identityContext)
        {
            UserManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            RoleManager = roleManager;
            IdentityContext = identityContext;
        }

        /// <inheritdoc />
        /// <summary>
        /// Get current user
        /// </summary>
        /// <returns></returns>
        public async Task<ResultModel<ApplicationUser>> GetCurrentUserAsync()
        {
            var result = new ResultModel<ApplicationUser>();
            if (_httpContextAccessor.HttpContext == null) return result;
            var user = await UserManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            result.IsSuccess = user != null;
            result.Result = user;
            return result;
        }

        /// <inheritdoc />
        /// <summary>
        /// Get roles from claims
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetRolesFromClaims()
        {
            var roles = _httpContextAccessor.HttpContext.User.Claims
                .Where(x => x.Type.Equals("role") || x.Type.EndsWith("role")).Select(x => x.Value)
                .ToList();
            return roles;
        }

        /// <inheritdoc />
        /// <summary>
        /// Get request ip address
        /// </summary>
        /// <returns></returns>
        public string GetRequestIpAdress()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString();
        }

        /// <inheritdoc />
        /// <summary>
        /// Tenant id
        /// </summary>
        public virtual Guid? CurrentUserTenantId
        {
            get
            {
                Guid? val = _httpContextAccessor?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "tenant")?.Value
                                ?.ToGuid() ?? GearSettings.TenantId;
                var userManager = IoC.Resolve<UserManager<ApplicationUser>>();
                if (val != Guid.Empty) return val;
                var user = userManager.GetUserAsync(_httpContextAccessor?.HttpContext?.User).GetAwaiter()
                    .GetResult();
                if (user == null) return null;
                var userClaims = userManager.GetClaimsAsync(user).GetAwaiter().GetResult();
                val = userClaims?.FirstOrDefault(x => x.Type == "tenant" && !string.IsNullOrEmpty(x.Value))?.Value?.ToGuid();

                return val;
            }
        }


        /// <inheritdoc />
        /// <summary>
        /// Add roles to user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel> AddToRolesAsync(ApplicationUser user, ICollection<string> roles)
        {
            var result = new ResultModel();
            var defaultRoles = new Collection<string> { GlobalResources.Roles.USER, GlobalResources.Roles.ANONIMOUS_USER };

            if (user == null || roles == null)
            {
                result.Errors.Add(new ErrorModel(string.Empty, "Bad parameters"));
                return result;
            }

            var exist = await UserManager.FindByEmailAsync(user.Email);
            if (exist == null)
            {
                result.Errors.Add(new ErrorModel(string.Empty, "User not found"));
                return result;
            }

            foreach (var defaultRole in defaultRoles)
            {
                if (roles.Contains(defaultRole)) continue;
                roles.Add(defaultRole);
            }

            var existentRoles = await UserManager.GetRolesAsync(exist);

            var newRoles = roles.Where(x => !existentRoles.Contains(x)).ToList();

            var serviceResult = await UserManager.AddToRolesAsync(exist, newRoles);

            if (serviceResult.Succeeded)
            {
                result.IsSuccess = true;
            }
            else result.AppendIdentityErrors(serviceResult.Errors);

            return result;
        }

        /// <inheritdoc />
        /// <summary>
        /// Get user roles
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApplicationRole>> GetUserRolesAsync(ApplicationUser user)
        {
            if (user == null) throw new NullReferenceException();
            var roles = await UserManager.GetRolesAsync(user);
            return roles.Select(async x => await RoleManager.FindByNameAsync(x)).Select(x => x.Result);
        }

        /// <summary>
        /// Disable user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ResultModel> DisableUserAsync(Guid? userId)
        {
            var response = new ResultModel();
            if (userId == null) return response;
            var user = await UserManager.Users.FirstOrDefaultAsync(x => x.Id.ToGuid().Equals(userId));
            if (user == null) return response;
            if (CurrentUserTenantId != user.TenantId) return response;
            user.IsDisabled = true;
            var request = await UserManager.UpdateAsync(user);
            return request.ToResultModel<object>().ToBase();
        }

        /// <summary>
        /// Get user addresses
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ResultModel<IEnumerable<Address>>> GetUserAddressesAsync(Guid? userId)
        {
            if (userId == null) return new NotFoundResultModel<IEnumerable<Address>>();
            var addresses = await IdentityContext.Addresses
                .Include(x => x.Country)
                .Include(x => x.StateOrProvince)
                .Where(x => x.ApplicationUserId.ToGuid().Equals(userId))
                .ToListAsync();
            return new ResultModel<IEnumerable<Address>>
            {
                IsSuccess = true,
                Result = addresses
            };
        }
    }
}