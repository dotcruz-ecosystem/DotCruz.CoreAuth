using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace DotCruz.CoreAuth.Api.Configurations;

public static class OpenApiConfiguration
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new()
                {
                    Title = "DotCruz.CoreAuth API",
                    Version = "v1",
                    Description = "Core de Autenticação Modular - DotCruz"
                };

                document.Components ??= new();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT neste formato: Bearer {token}"
                });

                document.Security ??= new List<OpenApiSecurityRequirement>();
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });

                return Task.CompletedTask;
            });

            // Mantém os nomes dos query params do doc/UI em snake_case,
            // coerente com o binding (ver SnakeCaseQueryValueProvider).
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                if (operation.Parameters is null)
                {
                    return Task.CompletedTask;
                }

                foreach (var parameter in operation.Parameters.OfType<OpenApiParameter>())
                {
                    if (parameter.In == ParameterLocation.Query && !string.IsNullOrEmpty(parameter.Name))
                    {
                        parameter.Name = JsonNamingPolicy.SnakeCaseLower.ConvertName(parameter.Name);
                    }
                }

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication MapOpenApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("DotCruz.CoreAuth API Documentation")
                       .WithTheme(ScalarTheme.DeepSpace)
                       .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            }).AllowAnonymous();
        }

        return app;
    }
}
