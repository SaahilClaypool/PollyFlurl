using System.Net;
using Microsoft.Extensions.Http;
using IFlurlHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;

namespace Polly.Flurl;
public static class PollyFlurlExtensions
{
    static readonly HashSet<HttpStatusCode> HTTPTransientErrorStatusCodes = new()
    {
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.BadGateway,
        HttpStatusCode.GatewayTimeout,
        HttpStatusCode.ServiceUnavailable,
    };
    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy<HttpResponseMessage> policy) => WithPolicy(new Url(request), policy);
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy<HttpResponseMessage> policy) => WithPolicy(new FlurlRequest(request), policy);
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy<HttpResponseMessage> policy)
    {
        request.Client.Configure(context =>
        {
            context.HttpClientFactory = new PollyHttpClientFactory(context.HttpClientFactory, policy);
        });
        return request;
    }

    public static IFlurlRequest WithRetry(this string request) => WithRetry(new Url(request));
    public static IFlurlRequest WithRetry(this Url request) => WithRetry(new FlurlRequest(request));
    public static IFlurlRequest WithRetry(this IFlurlRequest request) =>
        WithPolicy(request, Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode &&
                HTTPTransientErrorStatusCodes.Contains(response.StatusCode)
            )
            .RetryAsync());
}

internal class PollyHttpClientFactory : IFlurlHttpClientFactory
{
    private readonly IFlurlHttpClientFactory baseFactory;
    private readonly IAsyncPolicy<HttpResponseMessage> policy;

    public PollyHttpClientFactory(IFlurlHttpClientFactory baseFactory, IAsyncPolicy<HttpResponseMessage> policy)
    {
        this.baseFactory = baseFactory;
        this.policy = policy;
    }

    public HttpClient CreateHttpClient(HttpMessageHandler handler)
    {
        var pollyHandler = new PolicyHttpMessageHandler(policy)
        {
            InnerHandler = handler
        };
        return baseFactory.CreateHttpClient(pollyHandler);
    }

    public HttpMessageHandler CreateMessageHandler()
    {
        return new PolicyHttpMessageHandler(policy)
        {
            InnerHandler = baseFactory.CreateMessageHandler()
        };
    }
}