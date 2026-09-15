using DukaFlow.Application.Auth.Interfaces;
using DukaFlow.Application.Restaurants.Interfaces;
using DukaFlow.Infrastructure.Auth;
using DukaFlow.Infrastructure.Authentication;
using DukaFlow.Infrastructure.Persistence;
using DukaFlow.Infrastructure.Restaurants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DukaFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DukaFlowDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRestaurantService, RestaurantService>();

        return services;
    }
}
