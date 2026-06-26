using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using DotCruz.CoreAuth.Api.Filters;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DotCruz.CoreAuth.Api.Configurations;

public static class ApiConventionsConfiguration
{
    /// <summary>
    /// Registra os controllers aplicando as convenções da API:
    /// rotas em kebab-case, body (request/response) em snake_case e
    /// query params em snake_case.
    /// </summary>
    public static IMvcBuilder AddApiConventions(this IServiceCollection services)
    {
        // Alinha a serialização "fora do MVC" (ProblemDetails, minimal APIs, etc.)
        // à mesma convenção snake_case do body.
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
        });

        return services
            .AddControllers(options =>
            {
                // Preserva o ExceptionFilter existente
                options.Filters.Add<ExceptionFilter>();

                // Rotas: token [controller]/[action] -> kebab-case.
                options.Conventions.Add(
                    new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));

                // Query params: aceita as chaves em snake_case.
                ReplaceQueryValueProvider(options.ValueProviderFactories);
            })
            .AddJsonOptions(options =>
            {
                // Body (serialização e desserialização): snake_case.
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
            });
    }

    private static void ReplaceQueryValueProvider(IList<IValueProviderFactory> factories)
    {
        var defaultFactory = factories.OfType<QueryStringValueProviderFactory>().FirstOrDefault();

        if (defaultFactory is null)
        {
            factories.Add(new SnakeCaseQueryValueProviderFactory());
            return;
        }

        var index = factories.IndexOf(defaultFactory);
        factories[index] = new SnakeCaseQueryValueProviderFactory();
    }
}

/// <summary>
/// Converte tokens de rota ([controller]/[action]) para kebab-case.
/// Ex.: "PingTest" -> "ping-test".
/// </summary>
public sealed partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        var input = value?.ToString();

        return string.IsNullOrEmpty(input)
            ? input
            : WordBoundaryRegex().Replace(input, "$1-$2").ToLowerInvariant();
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex WordBoundaryRegex();
}

/// <summary>
/// Value provider que permite ligar query params escritos em snake_case
/// (ex.: ?page_number=1) a propriedades/parâmetros em PascalCase (PageNumber).
/// </summary>
public sealed class SnakeCaseQueryValueProviderFactory : IValueProviderFactory
{
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var query = context.ActionContext.HttpContext.Request.Query;
        context.ValueProviders.Add(
            new SnakeCaseQueryValueProvider(BindingSource.Query, query, CultureInfo.InvariantCulture));

        return Task.CompletedTask;
    }
}

public sealed class SnakeCaseQueryValueProvider(
    BindingSource bindingSource,
    IQueryCollection values,
    CultureInfo culture) : QueryStringValueProvider(bindingSource, values, culture)
{
    public override bool ContainsPrefix(string prefix) => base.ContainsPrefix(ToSnakeCase(prefix));

    public override ValueProviderResult GetValue(string key) => base.GetValue(ToSnakeCase(key));

    // Converte cada segmento (separados por ".") para preservar a sintaxe
    // de binding de objetos complexos/coleções.
    private static string ToSnakeCase(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return key;
        }

        return string.Join('.', key.Split('.').Select(JsonNamingPolicy.SnakeCaseLower.ConvertName));
    }
}
