﻿using GR.Core.Abstractions;
using IdentityServer4.EntityFramework.Interfaces;

namespace GR.Identity.Clients.Abstractions
{
    public interface IClientsContext : IConfigurationDbContext, IDbContext
    {

    }
}