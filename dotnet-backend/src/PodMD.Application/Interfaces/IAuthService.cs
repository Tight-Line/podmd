using PodMD.Application.Dtos;
using PodMD.Domain.Entities;

namespace PodMD.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> GenerateTokenAsync(ApplicationUser user);
}
