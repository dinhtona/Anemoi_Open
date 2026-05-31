using System.Threading.Tasks;
using Anemoi.Identity.Application.ApplicationModels.Enums;
using Anemoi.Identity.Domain.Models;

namespace Anemoi.Identity.Application.Abstractions;

public interface ISignInRepository
{
    /// <summary>
    /// Validates password and checks lockout WITHOUT writing any authentication cookie.
    /// Safe to call from MassTransit consumers where HttpContext is not available.
    /// </summary>
    Task<SignInResult> CheckPasswordSignInAsync(User user, string password, bool lockoutOnFailure);

    /// <summary>
    /// Full sign-in including cookie. Requires an active HttpContext.
    /// Only use this from HTTP request pipelines.
    /// </summary>
    Task<SignInResult> PasswordSignInAsync(User user, string password,
        bool isPersistent, bool lockoutOnFailure);

    Task SignOutAsync();
    
}