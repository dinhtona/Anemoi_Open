using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Linq.Expressions;
using System.Security.Claims;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Configurations;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Centralize.Api.Controllers.Identity;
using Anemoi.Centralize.Infrastructure.Installers;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Identity.Queries.UserQueries.GetUsers;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Application.Configurations;
using Anemoi.Identity.Application.Cqrs.Commands.IdentityCommands.TokenGenerators;
using Anemoi.Identity.Application.IdentityResults;
using Anemoi.Identity.Domain.Models;
using Lambda.Identity.Application.SeedData;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OneOf;
using Serilog;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class AdministratorAuthorizationBehaviorTests
{
    private const string Issuer = "anemoi-identity";
    private const string Audience = "anemoi-services";

    [Fact]
    public void AdministratorRoleGroupProfile_ShouldContainCriticalAdminPermissions()
    {
        var permissions = SystemRoleProfiles.Admin.Permissions;

        Assert.Contains(Permissions.UserRead, permissions);
        Assert.Contains(Permissions.UserManage, permissions);
        Assert.Contains(Permissions.RoleRead, permissions);
        Assert.Contains(Permissions.RoleManage, permissions);
        Assert.Contains(Permissions.EnvironmentRead, permissions);
        Assert.Contains(Permissions.EnvironmentStartStop, permissions);
        Assert.Contains(Permissions.HrEmployeeView, permissions);
        Assert.Contains(Permissions.HrEmployeeIdentityLink, permissions);
        Assert.Contains(Permissions.NotificationManage, permissions);
        Assert.Contains(Permissions.HrWorkflowManage, permissions);
    }

    [Fact]
    public async Task RegisterAdministratorAsync_ShouldCreateAdministratorRoleGroupMapping_Idempotently()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var userDbRepository = Substitute.For<ISqlRepository<User>>();
        var roleGroupRepository = Substitute.For<ISqlRepository<RoleGroup>>();
        var userMapRoleGroupRepository = Substitute.For<ISqlRepository<UserMapRoleGroup>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var mediator = Substitute.For<IMediator>();
        var logger = Substitute.For<ILogger>();

        var adminUser = new User
        {
            UserId = new UserId(Guid.NewGuid()),
            Email = "admin@anemoi.com"
        };
        var administratorRoleGroup = new RoleGroup
        {
            Id = new RoleGroupId(Guid.NewGuid()),
            Code = SystemRoleProfiles.Admin.Code,
            IsDefault = true,
            RoleGroupClaims = [],
            RoleGroupMapRoles = [],
            UserMapRoleGroups = []
        };

        userDbRepository.GetFirstByConditionAsync(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<Func<IQueryable<User>, IQueryable<User>>>(),
                Arg.Any<CancellationToken>())
            .Returns(adminUser);
        userRepository.GetDirectRolesAsync(adminUser)
            .Returns(
                Task.FromResult<IList<string>>([]),
                Task.FromResult<IList<string>>([SystemRoles.Administrator]));
        userRepository.AddToRolesAsync(adminUser, Arg.Any<IEnumerable<string>>())
            .Returns(Task.FromResult<OneOf<None, Exception>>(None.Value));
        roleGroupRepository.GetFirstByConditionAsync(
                Arg.Any<Expression<Func<RoleGroup, bool>>>(),
                Arg.Any<Func<IQueryable<RoleGroup>, IQueryable<RoleGroup>>>(),
                Arg.Any<CancellationToken>())
            .Returns(administratorRoleGroup);
        userMapRoleGroupRepository.ExistByConditionAsync(
                Arg.Any<Expression<Func<UserMapRoleGroup, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false), Task.FromResult(true));
        userMapRoleGroupRepository.CreateOneAsync(Arg.Any<UserMapRoleGroup>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<OneOf<UserMapRoleGroup, Exception>>(callInfo.Arg<UserMapRoleGroup>()));
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OneOf<None, Exception>>(None.Value));

        using var scope = BuildScope(services =>
        {
            services.AddSingleton(userRepository);
            services.AddSingleton(userDbRepository);
            services.AddSingleton(roleGroupRepository);
            services.AddSingleton(userMapRoleGroupRepository);
            services.AddSingleton(unitOfWork);
            services.AddSingleton(mediator);
            services.AddSingleton(logger);
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"{nameof(SeedUserData)}:{nameof(SeedUserData.SupperAdminUsers)}:0:{nameof(SupperAdminUser.UserName)}"] = "admin@anemoi.com",
                    [$"{nameof(SeedUserData)}:{nameof(SeedUserData.SupperAdminUsers)}:0:{nameof(SupperAdminUser.Password)}"] = "Admin@12345",
                    [$"{nameof(SeedUserData)}:{nameof(SeedUserData.SupperAdminUsers)}:0:{nameof(SupperAdminUser.FirstName)}"] = "Anemoi",
                    [$"{nameof(SeedUserData)}:{nameof(SeedUserData.SupperAdminUsers)}:0:{nameof(SupperAdminUser.LastName)}"] = "Admin"
                })
                .Build());
        });

        await SeedData.RegisterAdministratorAsync(scope);
        await SeedData.RegisterAdministratorAsync(scope);

        await userRepository.Received().AddToRolesAsync(adminUser,
            Arg.Is<IEnumerable<string>>(roles => roles.Single() == SystemRoles.Administrator));
        await userMapRoleGroupRepository.Received(1).CreateOneAsync(
            Arg.Is<UserMapRoleGroup>(mapping =>
                mapping.UserId == adminUser.UserId &&
                mapping.RoleGroupId == administratorRoleGroup.Id),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TokenGeneratorHandler_ShouldEmitAdministratorRoleGroupClaim()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var userClaimRepository = Substitute.For<IUserClaimRepository>();
        var identityPolicyRepository = Substitute.For<ISqlRepository<IdentityPolicy>>();

        var user = new User
        {
            UserId = new UserId(Guid.NewGuid()),
            FirstName = "Anemoi",
            LastName = "Admin",
            Email = "admin@anemoi.com"
        };

        userRepository.GetUserRoleGroupCodesAsync(user)
            .Returns(Task.FromResult<IList<string>>([SystemRoleProfiles.Admin.Code]));
        userRepository.GetEffectiveRolesAsync(user)
            .Returns(Task.FromResult<IList<string>>([Permissions.UserRead]));
        userClaimRepository.GetUserClaimsAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<Claim>>([]));
        identityPolicyRepository.GetManyByConditionAsync(
                Arg.Any<Expression<Func<IdentityPolicy, bool>>>(),
                Arg.Any<Func<IQueryable<IdentityPolicy>, IQueryable<IdentityPolicy>>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<IdentityPolicy>()));

        await using var signingContext = CreateSigningContext();
        var handler = new TokenGeneratorHandler(
            JwtSecurity.GetPrivateSigningCredential(signingContext.PrivateKeyPath),
            new JwtSetting
            {
                Issuer = Issuer,
                Audience = Audience,
                TokenLifetime = TimeSpan.FromMinutes(15),
                RefreshTokenLifetime = TimeSpan.FromDays(1)
            },
            userRepository,
            userClaimRepository,
            identityPolicyRepository);

        var result = await handler.Handle(new TokenGeneratorCommand(user), CancellationToken.None);

        Assert.True(result.IsT0);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AsT0.UserToken);
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == AuthorizationClaimTypes.RoleGroup &&
            claim.Value == SystemRoleProfiles.Admin.Code);
    }

    [Fact]
    public async Task AdministratorRoleGroupToken_ShouldAccessIdentityHasPermissionEndpoint_WhileEmployeeIsForbidden()
    {
        await using var host = await CreateIdentityUserHostAsync();
        var adminToken = CreateSignedToken(host.PrivateKeyPath, AuthorizationPolicies.Internal, SystemRoleProfiles.Admin.Code);
        var employeeToken = CreateSignedToken(host.PrivateKeyPath, AuthorizationPolicies.Internal, SystemRoleProfiles.Employee.Code);

        var adminResponse = await SendGetUsersRequestAsync(host.Client, adminToken);
        var employeeResponse = await SendGetUsersRequestAsync(host.Client, employeeToken);

        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, employeeResponse.StatusCode);
    }

    private static IServiceScope BuildScope(Action<ServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider().CreateScope();
    }

    private static async Task<TestHostContext> CreateIdentityUserHostAsync()
    {
        var keyDirectory = Path.Combine(Path.GetTempPath(), "Anemoi_Open", "AdminAuthorizationTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(keyDirectory);

        var privateKeyPath = Path.Combine(keyDirectory, "development-private.key");
        var publicKeyPath = Path.Combine(keyDirectory, "development-public.crt");
        JwtSecurity.CreateKeyPair(privateKeyPath, publicKeyPath);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{nameof(JwtSetting)}:{nameof(JwtSetting.PublicKeyPath)}"] = publicKeyPath,
            [$"{nameof(JwtSetting)}:{nameof(JwtSetting.Issuer)}"] = Issuer,
            [$"{nameof(JwtSetting)}:{nameof(JwtSetting.Audience)}"] = Audience,
            [$"{nameof(JwtSetting)}:{nameof(JwtSetting.TokenLifetime)}"] = "00:15:00",
        });

        builder.Services.AddControllers().AddApplicationPart(typeof(UserController).Assembly);
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSingleton(CreateIdentitySenderStub());

        new AuthenticationInstaller().InstallerServices(builder.Services, builder.Configuration);
        new AuthorizationInstaller().InstallerServices(builder.Services, builder.Configuration);

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        return new TestHostContext(app, app.GetTestClient(), privateKeyPath, keyDirectory);
    }

    private static ISender CreateIdentitySenderStub()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new PaginationResponse<UserResponse>([], 0)));
        return sender;
    }

    private static async Task<HttpResponseMessage> SendGetUsersRequestAsync(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/identity/User/GetUsers");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    private static string CreateSignedToken(string privateKeyPath, string policyValue, string roleGroup)
    {
        var signingCredentials = JwtSecurity.GetPrivateSigningCredential(privateKeyPath);
        var issuedAt = DateTimeOffset.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(AuthorizationClaimTypes.TokenIssuedAtUtcTicks, issuedAt.UtcTicks.ToString(), ClaimValueTypes.Integer64),
                new Claim("id", Guid.NewGuid().ToString()),
                new Claim("email", "admin@anemoi.test"),
                new Claim(AuthorizationClaimTypes.ApplicationPolicyInternal, policyValue),
                new Claim(AuthorizationClaimTypes.RoleGroup, roleGroup)
            ]),
            Issuer = Issuer,
            Audience = Audience,
            Expires = issuedAt.AddMinutes(15).UtcDateTime,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(descriptor));
    }

    private static SigningContext CreateSigningContext()
    {
        var keyDirectory = Path.Combine(Path.GetTempPath(), "Anemoi_Open", "AdminTokenGeneratorTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(keyDirectory);

        var privateKeyPath = Path.Combine(keyDirectory, "development-private.key");
        var publicKeyPath = Path.Combine(keyDirectory, "development-public.crt");
        JwtSecurity.CreateKeyPair(privateKeyPath, publicKeyPath);

        return new SigningContext(privateKeyPath, keyDirectory);
    }

    private sealed record SigningContext(string PrivateKeyPath, string KeyDirectory) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            TryDelete(KeyDirectory);
            return ValueTask.CompletedTask;
        }
    }

    private sealed record TestHostContext(
        WebApplication App,
        HttpClient Client,
        string PrivateKeyPath,
        string KeyDirectory)
        : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await App.DisposeAsync();
            TryDelete(KeyDirectory);
        }
    }

    private static void TryDelete(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
        catch
        {
            // Best effort cleanup only.
        }
    }
}
