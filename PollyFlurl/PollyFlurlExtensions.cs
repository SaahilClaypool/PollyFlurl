using Microsoft.Extensions.Http;
using IFlurlHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;

namespace Polly.Flurl;
public static class PollyFlurlExtensions
{
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

    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy policy) => WithPolicy(new Url(request), policy);
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy policy) => WithPolicy(new FlurlRequest(request), policy);
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy policy) => WithPolicy(request, policy.AsAsyncPolicy<HttpResponseMessage>());
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