using System;
using Anemoi.BuildingBlock.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Anemoi.Identity.Application.Configurations;
using Anemoi.Identity.Domain.Models;
using Anemoi.Identity.Infrastructure.DataContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Identity.Infrastructure.Installers;

public sealed class IdentityInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<User, Role>(config =>
            {
                var passwordConfiguration = configuration
                    .GetSection(nameof(PasswordConfiguration))
                    .Get<PasswordConfiguration>()!;
                var (includeLowercase, includeUppercase, includeNumeric, includeSpecial, passwordLength) =
                    passwordConfiguration;
                // config.SignIn.RequireConfirmedEmail = true;
                // config.SignIn.RequireConfirmedPhoneNumber = true;
                config.Password.RequireDigit = includeNumeric;
                config.Password.RequiredLength = passwordLength;
                config.Password.RequireLowercase = includeLowercase;
                config.Password.RequireUppercase = includeUppercase;
                config.Password.RequireNonAlphanumeric = includeSpecial;
                // SEC-011: brute-force lockout (new users default LockoutEnabled = true,
                // see IdentityMapper.ToUser). 5 failed attempts -> 5-minute lockout.
                config.Lockout.AllowedForNewUsers = true;
                config.Lockout.MaxFailedAccessAttempts = 5;
                config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<DataProtectorTokenProvider<User>>("email");
    }
}