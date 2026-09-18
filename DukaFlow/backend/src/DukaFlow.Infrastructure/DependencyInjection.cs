using DukaFlow.Application.Auth.Interfaces;
using DukaFlow.Application.Menu.Interfaces;
using DukaFlow.Application.Menu.Services;
using DukaFlow.Application.Ordering.Interfaces;
using DukaFlow.Application.Restaurants.Interfaces;
using DukaFlow.Infrastructure.Auth;
using DukaFlow.Infrastructure.Authentication;
using DukaFlow.Infrastructure.Menu;
using DukaFlow.Infrastructure.Ordering;
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
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IImageStorageService, LocalImageStorageService>();

        // Phase 3 - Ordering Engine
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPublicMenuService, PublicMenuService>();
        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();

        return services;
    }
}
