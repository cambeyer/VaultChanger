using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using VaultChanger.Models.Configuration;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.SecretsEngines;

namespace VaultChanger.Extensions;

public static class VaultExtensions
{
    private const string PathSeparator = "~~~";
    private const string VaultNamespaceHeaderKey = "X-Vault-Namespace";

    private static readonly Regex NamespaceExtractor = new($"{PathSeparator}(.+?){PathSeparator}", RegexOptions.Compiled);

    public static string GeneratePath(string vaultNamespace, string path) =>
        $"{PathSeparator}{WebUtility.UrlDecode(vaultNamespace)}{PathSeparator}{WebUtility.UrlDecode(path).Trim('/')}";

    public static List<TSource> Flatten<TSource>(
        this IList<TSource> source,
        Func<TSource, IList<TSource>> getChildrenFunction)
    {
        return source.Aggregate(source,
            (current, element) =>
                current.Concat(getChildrenFunction(element).Flatten(getChildrenFunction)).ToList()).ToList();
    }

    public static IEnumerable<string> Flatten<TSource>(this TSource source,
        Func<TSource, IEnumerable<string>> resultSelector, Func<TSource, IEnumerable<TSource>> nextSelector)
    {
        var result = new List<string>();
        result.AddRange(resultSelector(source));
        foreach (var nextSource in nextSelector(source))
        {
            result.AddRange(nextSource.Flatten(resultSelector, nextSelector));
        }

        return result;
    }

    public static void AddVaultClient(this IServiceCollection services, VaultConfig config)
    {
        var client = new VaultClient(
            new VaultClientSettings(config.Url, new TokenAuthMethodInfo(System.IO.File.ReadAllText(config.TokenPath)))
            {
                SecretsEngineMountPoints = new SecretsEngineMountPoints
                {
                    Transit = config.TransitMountPoint
                },
                BeforeApiRequestAction = (_, httpRequestMessage) =>
                {
                    if (httpRequestMessage.RequestUri == null) return;
                    var builder = new UriBuilder(httpRequestMessage.RequestUri);
                    var path = builder.Path;
                    var desiredNamespace = NamespaceExtractor.Match(path).Groups[1].Value;
                    if (desiredNamespace == string.Empty) return;

                    builder.Path = NamespaceExtractor.Replace(path, string.Empty);
                    httpRequestMessage.RequestUri = builder.Uri;
                    httpRequestMessage.Headers.Add(VaultNamespaceHeaderKey, desiredNamespace);
                }
            }
        );
        
        client.V1.Auth.PerformImmediateLogin().Wait();
        services.AddSingleton<IVaultClient>(client);
    }
}