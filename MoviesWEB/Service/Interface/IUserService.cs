using MoviesWEB.Models.System;
using NuGet.Protocol.Plugins;
using System.Threading.Tasks;

namespace MoviesWEB.Service.Interface
{
    public interface IUserService
    {
        Task<LoginResponse> CheckUserCredidentals(LoginRequest loginRequest);
        Task<bool> LogoutUser(string username);
        Task<bool> TryRegisterRequest(RegisterRequest registerRequest);
        Task<bool> UpdateUserProfileAsync(UserProfile userProfile);
    }
}
