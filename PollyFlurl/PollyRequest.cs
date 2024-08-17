using Flurl.Http.Configuration;
using Flurl.Util;

namespace PollyFlurl;

internal abstract class RequestWrapper : IFlurlRequest
{
    protected readonly IFlurlRequest innerRequest;

    public RequestWrapper(IFlurlRequest innerRequest)
    {
        this.innerRequest = innerRequest;
    }
    public IFlurlClient Client { get => innerRequest.Client; set => innerRequest.Client = value; }
    public HttpMethod Verb { get => innerRequest.Verb; set => innerRequest.Verb = value; }
    public Url Url { get => innerRequest.Url; set => innerRequest.Url = value; }

    public IEnumerable<(string Name, string Value)> Cookies => innerRequest.Cookies;

    public CookieJar CookieJar { get => innerRequest.CookieJar; set => innerRequest.CookieJar = value; }
    public FlurlHttpSettings Settings { get => innerRequest.Settings; set => innerRequest.Settings = value; }

    public INameValueList<string> Headers => innerRequest.Headers;

    public abstract Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default);
}

#region ResiliencePipeline

/// <summary> Wrap a flurl request pipeline</summary>
internal class PollyPipelineRequestFlurlResponse : RequestWrapper
{
    private readonly ResiliencePipeline<IFlurlResponse> pipeline;

    public PollyPipelineRequestFlurlResponse(IFlurlRequest innerRequest, ResiliencePipeline<IFlurlResponse> pipeline) : base(innerRequest)
    {
        this.pipeline = pipeline;
    }

    public override async Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default)
    {
        return await pipeline.ExecuteAsync(async cancellationToken => await innerRequest.SendAsync(verb, content, completionOption, cancellationToken));
    }
}

/// <summary> Wrap a generic pipeline </summary>
internal class PollyPipelineRequest : RequestWrapper
{
    private readonly ResiliencePipeline pipeline;

    public PollyPipelineRequest(IFlurlRequest innerRequest, ResiliencePipeline pipeline) : base(innerRequest)
    {
        this.pipeline = pipeline;
    }

    public override async Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default)
    {
        return await pipeline.ExecuteAsync(async cancellationToken => await innerRequest.SendAsync(verb, content, completionOption, cancellationToken));
    }
}

/// <summary> Wrap a http response pipeline </summary>
internal class PollyPipelineHttpResponseRequest : RequestWrapper
{
    private readonly ResiliencePipeline<HttpResponseMessage> pipeline;

    public PollyPipelineHttpResponseRequest(IFlurlRequest innerRequest, ResiliencePipeline<HttpResponseMessage> pipeline) : base(innerRequest)
    {
        this.pipeline = pipeline;
    }

    public override async Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default)
    {
        var response = await pipeline.ExecuteAsync(async cancellationToken =>
        {
            try
            {
                var response = await innerRequest.SendAsync(verb, content, completionOption, cancellationToken);
                return response.ResponseMessage;
            }
            catch (FlurlHttpException ex)
            {
                return ex.Call.Response.ResponseMessage;
            }
        });

        return new FlurlResponse(response);
    }
}

#endregion

#region Policy (legacy)

/// <summary> Wrap a flurl request policy</summary>
internal class PollyPolicyRequestFlurlResponse : RequestWrapper
{
    private readonly IAsyncPolicy<IFlurlResponse> policy;

    public PollyPolicyRequestFlurlResponse(IFlurlRequest innerRequest, IAsyncPolicy<IFlurlResponse> policy) : base(innerRequest)
    {
        this.policy = policy;
    }

    public override Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default) =>
        policy.ExecuteAsync(() => innerRequest.SendAsync(verb, content, completionOption, cancellationToken));
}

/// <summary> Wrap a generic policy </summary>
internal class PollyPolicyRequest : RequestWrapper
{
    private readonly IAsyncPolicy policy;

    public PollyPolicyRequest(IFlurlRequest innerRequest, IAsyncPolicy policy) : base(innerRequest)
    {
        this.policy = policy;
    }

    public override Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default) =>
        policy.ExecuteAsync(() => innerRequest.SendAsync(verb, content, completionOption, cancellationToken));
}

/// <summary> Wrap a http response policy </summary>
internal class PollyPolicyHttpResponseRequest : RequestWrapper
{
    private readonly IAsyncPolicy<HttpResponseMessage> policy;

    public PollyPolicyHttpResponseRequest(IFlurlRequest innerRequest, IAsyncPolicy<HttpResponseMessage> policy) : base(innerRequest)
    {
        this.policy = policy;
    }

    public override async Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default)
    {
        var response = await policy.ExecuteAsync(async () =>
        {
            try
            {
                var response = await innerRequest.SendAsync(verb, content, completionOption, cancellationToken);
                return response.ResponseMessage;
            }
            catch (FlurlHttpException ex)
            {
                return ex.Call.Response.ResponseMessage;
            }
        });

        return new FlurlResponse(response);
    }
}

#endregion