using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Common;

public static class AuthExtensions
{
    public static IServiceCollection AddKeyCloakAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication()
            .AddKeycloakJwtBearer(serviceName: "keycloak", realm: "overflow-learn", options =>
            {
                options.RequireHttpsMetadata = false;
                options.Audience = "overflow-learn";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuers =
                    [
                        "http://localhost:6001/realms/overflow-learn",
                        "http://keycloak/realms/overflow-learn",
                        "http://keycloak:8080/realms/overflow-learn",
                        "http://id.overflow.local/realms/overflow-learn",
                        "https://id.overflow.local/realms/overflow-learn",
                        "https://overflow-id.trycatchlearn.com/realms/overflow-learn",
                    ],
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddAuthorizationBuilder();

        return services;
    }
}