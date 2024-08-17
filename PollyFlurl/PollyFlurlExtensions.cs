using Polly.Retry;
using System.Net;

namespace PollyFlurl;
public static class PollyFlurlExtensions
{
    #region ResiliencePipeline

    public static IFlurlRequest WithPipeline(this string request, ResiliencePipeline<IFlurlResponse> pipeline) => WithPipeline(new Url(request), pipeline);
    public static IFlurlRequest WithPipeline(this Url request, ResiliencePipeline<IFlurlResponse> pipeline) => WithPipeline(new FlurlRequest(request), pipeline);
    public static IFlurlRequest WithPipeline(this IFlurlRequest request, ResiliencePipeline<IFlurlResponse> pipeline)
    {
        return new PollyPipelineRequestFlurlResponse(request, pipeline);
    }

    public static IFlurlRequest WithPipeline(this string request, ResiliencePipeline pipeline) => WithPipeline(new Url(request), pipeline);
    public static IFlurlRequest WithPipeline(this Url request, ResiliencePipeline pipeline) => WithPipeline(new FlurlRequest(request), pipeline);
    public static IFlurlRequest WithPipeline(this IFlurlRequest request, ResiliencePipeline pipeline)
    {
        return new PollyPipelineRequest(request, pipeline);
    }

    public static IFlurlRequest WithPipeline(this string request, ResiliencePipeline<HttpResponseMessage> pipeline) => WithPipeline(new Url(request), pipeline);
    public static IFlurlRequest WithPipeline(this Url request, ResiliencePipeline<HttpResponseMessage> pipeline) => WithPipeline(new FlurlRequest(request), pipeline);
    public static IFlurlRequest WithPipeline(this IFlurlRequest request, ResiliencePipeline<HttpResponseMessage> pipeline)
    {
        return new PollyPipelineHttpResponseRequest(request, pipeline);
    }

    static readonly HttpStatusCode[] httpStatusCodesWorthRetrying = {
        HttpStatusCode.RequestTimeout, // 408
        HttpStatusCode.InternalServerError, // 500
        HttpStatusCode.BadGateway, // 502
        HttpStatusCode.ServiceUnavailable, // 503
        HttpStatusCode.GatewayTimeout // 504
    };

    public static IFlurlRequest RetryTransientErrors(this string request) => RetryTransientErrors(new Url(request));
    public static IFlurlRequest RetryTransientErrors(this Url request) => RetryTransientErrors(new FlurlRequest(request));
    public static IFlurlRequest RetryTransientErrors(this IFlurlRequest request) => WithPipeline(request, defaultRetryPipeline.Value);

    private static readonly Lazy<ResiliencePipeline<HttpResponseMessage>> defaultRetryPipeline = new Lazy<ResiliencePipeline<HttpResponseMessage>>(() =>
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>()
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
            })
            .Build();
    });

    #endregion

    #region Policy (legacy)

    private const string WithPolicyObsoleteWarning = @"Use "".WithPipeline(ResiliencePipeline pipeline)"" instead. See Polly v8's doc for more info: https://www.pollydocs.org/migration-v8.html#configuring-strategies-in-v8";

    [Obsolete(WithPolicyObsoleteWarning)]
    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy<IFlurlResponse> policy) => WithPolicy(new Url(request), policy);
    [Obsolete(WithPolicyObsoleteWarning)]
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy<IFlurlResponse> policy) => WithPolicy(new FlurlRequest(request), policy);
    [Obsolete(WithPolicyObsoleteWarning)]
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy<IFlurlResponse> policy)
    {
        return new PollyPolicyRequestFlurlResponse(request, policy);
    }

    [Obsolete(WithPolicyObsoleteWarning)]
    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy policy) => WithPolicy(new Url(request), policy);
    [Obsolete(WithPolicyObsoleteWarning)]
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy policy) => WithPolicy(new FlurlRequest(request), policy);
    [Obsolete(WithPolicyObsoleteWarning)]
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy policy)
    {
        return new PollyPolicyRequest(request, policy);
    }

    [Obsolete(WithPolicyObsoleteWarning)]
    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy<HttpResponseMessage> policy) => WithPolicy(new Url(request), policy);
    [Obsolete(WithPolicyObsoleteWarning)]
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy<HttpResponseMessage> policy) => WithPolicy(new FlurlRequest(request), policy);
    [Obsolete(WithPolicyObsoleteWarning)]
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy<HttpResponseMessage> policy)
    {
        return new PollyPolicyHttpResponseRequest(request, policy);
    }

    #endregion
}